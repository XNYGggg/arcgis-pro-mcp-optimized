using System.Threading.Tasks;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>
    /// Two explicit guard tools for flows that do not go through the
    /// geoprocessing commands (for example an ENVI-import step that runs a
    /// Python toolbox manually, or an assistant orchestrating a long dialog
    /// sequence). The automatic summon around run_python_toolbox_tool lives in
    /// GeoprocessingCommands; these are the manual entry points.
    /// </summary>
    internal static class GuardCommands
    {
        private const string Group = "Guard";

        public static void Register()
        {
            // Inline: a guard is useful precisely when other lanes are busy.
            CommandRouter.RegisterAsync("guard_summon", Group,
                "Summon the on-demand dialog guard for a manual step. It clicks only "
                + "the third-party code library trust dialog, alerts (>=30 s) when it "
                + "cannot, and exits after timeout_s or when settled.",
                SummonAsync, lane: ExecutionLane.Inline);

            CommandRouter.Register("guard_status", Group,
                "Status of the dialog guard: engine path/version, session root and the "
                + "20 most recent summon sessions.",
                Status, runOnMct: false, lane: ExecutionLane.Inline);
        }

        private static Task<object> SummonAsync(Params parameters)
        {
            var scope = parameters.GetString("scope", "manual");
            if (scope != "op" && scope != "manual")
                scope = "manual";
            var timeout = parameters.GetInt("timeout_s", scope == "op" ? 180 : 600);
            var alertOnly = parameters.GetBool("alert_only");
            var result = GuardClient.Summon(scope, timeout, alertOnly, "guard_summon tool");
            return Task.FromResult<object>(result);
        }

        private static object Status(Params parameters)
        {
            return GuardClient.Status();
        }
    }
}
