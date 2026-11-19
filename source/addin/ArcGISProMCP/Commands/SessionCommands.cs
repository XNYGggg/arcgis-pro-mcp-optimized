using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using ArcGISProMCP.Clients;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>Session, project and map-level commands.</summary>
    internal static class SessionCommands
    {
        private const string Group = "project";

        public static void Register()
        {
            // No lane slot: each step is dispatched on its own and queues
            // normally. Holding a slot here would queue the batch behind the
            // very work it is waiting for.
            CommandRouter.RegisterAsync("run_batch", Group,
                "Run several commands in one round trip.", RunBatchAsync,
                lane: ExecutionLane.Inline);

            // Answered without touching the object model, so they stay
            // responsive even when ArcGIS Pro is busy.
            CommandRouter.Register("ping", Group,
                "Check that the bridge is reachable.", Ping, runOnMct: false,
                lane: ExecutionLane.Inline);

            CommandRouter.Register("get_capabilities", Group,
                "List every command this bridge supports.",
                _ => CommandRouter.Describe(), runOnMct: false,
                lane: ExecutionLane.Inline);

            CommandRouter.Register("get_queue_status", Group,
                "Queue depth, the running command and recent history per lane.",
                _ => JobScheduler.Describe(), runOnMct: false,
                lane: ExecutionLane.Inline);

            CommandRouter.Register("diagnose", Group,
                "Self-check: ArcGIS Pro version, project, active map and view.", Diagnose);

            CommandRouter.RegisterAsync("restart_pro", Group,
                "Restart ArcGIS Pro, saving and reopening the project first -- "
                + "the way to load a rebuilt add-in without touching the desktop.",
                RestartProAsync);

            CommandRouter.Register("get_arcgis_info", Group,
                "ArcGIS Pro version and the current project.", GetArcGisInfo);

            CommandRouter.Register("get_project_info", Group,
                "Project paths, default geodatabase, maps and layouts.", GetProjectInfo);

            CommandRouter.Register("list_maps", Group,
                "List all maps with coordinate system and layer counts.", ListMaps);

            CommandRouter.Register("get_map_extent", Group,
                "The combined extent of a map's layers.", GetMapExtent);
        }

        private static object Ping(Params parameters)
        {
            var server = MCPModule.Current?.Server;
            var result = new Dictionary<string, object>
            {
                ["pong"] = true,
                ["implementation"] = "addin",
                ["pid"] = Process.GetCurrentProcess().Id,
                // The address every AI client connects on. Named so nobody has
                // to guess which of the three ports is theirs -- reporting the
                // TCP bridge's number here once had an assistant tell its user
                // the wrong port in perfectly good faith.
                ["mcp_url"] = McpClientCatalog.HttpUrl,
                ["tcp_bridge_port"] = server?.Port,
                ["command_count"] = CommandRouter.Count,
            };

            // Ping does not run on the MCT, so reading the project could throw.
            // If it does, leave the key out: a null here reads as "no project
            // open", which is a different and wrong answer.
            try { result["project_path"] = Project.Current?.URI; }
            catch (Exception) { }

            return result;
        }

        private static object GetArcGisInfo(Params parameters)
        {
            var project = Project.Current;

            // The SDK assembly version is 13.x while the product is 3.x, so
            // read the version off ArcGISPro.exe -- that is the number people
            // mean when they say which ArcGIS Pro they are on.
            string version = null, assemblyVersion = null;
            try
            {
                version = System.Diagnostics.Process.GetCurrentProcess()
                    .MainModule?.FileVersionInfo?.ProductVersion;
            }
            catch { /* fall back to the assembly version below */ }
            try
            {
                assemblyVersion = System.Reflection.Assembly
                    .GetAssembly(typeof(Project))?.GetName().Version?.ToString();
            }
            catch { }

            return new Dictionary<string, object>
            {
                ["product"] = "ArcGIS Pro",
                ["version"] = version ?? assemblyVersion,
                ["sdk_assembly_version"] = assemblyVersion,
                ["project_path"] = project?.URI,
                ["project_name"] = project?.Name,
                ["default_geodatabase"] = project?.DefaultGeodatabasePath,
                ["home_folder"] = project?.HomeFolderPath,
                ["implementation"] = "addin",
            };
        }

        private static object GetProjectInfo(Params parameters)
        {
            var project = MapHelpers.RequireProject();
            var maps = project.GetItems<MapProjectItem>().Select(item =>
            {
                var map = item.GetMap();
                return new Dictionary<string, object>
                {
                    ["name"] = map.Name,
                    ["map_type"] = map.MapType.ToString(),
                    ["layer_count"] = map.GetLayersAsFlattenedList().Count,
                    ["table_count"] = map.GetStandaloneTablesAsFlattenedList().Count,
                    ["spatial_reference"] = map.SpatialReference?.Name,
                };
            }).ToList();

            return new Dictionary<string, object>
            {
                ["path"] = project.URI,
                ["name"] = project.Name,
                ["home_folder"] = project.HomeFolderPath,
                ["default_geodatabase"] = project.DefaultGeodatabasePath,
                ["default_toolbox"] = project.DefaultToolboxPath,
                ["maps"] = maps,
                ["layouts"] = project.GetItems<LayoutProjectItem>()
                                     .Select(item => item.Name).ToList(),
                ["active_map"] = MapView.Active?.Map?.Name,
            };
        }

        private static object ListMaps(Params parameters)
        {
            return MapHelpers.RequireProject().GetItems<MapProjectItem>().Select(item =>
            {
                var map = item.GetMap();
                return new Dictionary<string, object>
                {
                    ["name"] = map.Name,
                    ["map_type"] = map.MapType.ToString(),
                    ["spatial_reference"] = MapHelpers.Describe(map.SpatialReference),
                    ["layer_count"] = map.GetLayersAsFlattenedList().Count,
                    ["table_count"] = map.GetStandaloneTablesAsFlattenedList().Count,
                    ["bookmark_count"] = map.GetBookmarks().Count,
                };
            }).ToList();
        }

        private static object GetMapExtent(Params parameters)
        {
            var map = MapHelpers.ResolveMap(parameters);
            ArcGIS.Core.Geometry.Envelope combined = null;

            foreach (var layer in map.GetLayersAsFlattenedList().OfType<BasicFeatureLayer>())
            {
                try
                {
                    var extent = layer.QueryExtent();
                    if (extent == null || extent.IsEmpty) continue;
                    combined = combined == null
                        ? extent
                        : combined.Union(extent);
                }
                catch
                {
                    // A broken or empty layer should not sink the whole answer.
                }
            }

            return new Dictionary<string, object>
            {
                ["map"] = map.Name,
                ["extent"] = MapHelpers.Describe(combined),
            };
        }

        private static int _restartScheduled;
        private static long _restartScheduledAtTicks;

        // Close (240s) + bridge (240s) + delay (10s) + margin: a helper that
        // died without cleaning up must not block restarts forever.
        private static readonly TimeSpan RestartGuardLifetime =
            TimeSpan.FromMinutes(10);

        /// <summary>
        /// Saves, then hands the close/reopen to a detached PowerShell helper:
        /// nothing inside ArcGIS Pro can survive its own shutdown to bring the
        /// app back, and the helper also survives the add-in being unloaded.
        /// </summary>
        private static async Task<object> RestartProAsync(Params parameters)
        {
            if (Interlocked.CompareExchange(ref _restartScheduled, 1, 0) != 0)
            {
                var scheduledAt = new DateTime(
                    Interlocked.Read(ref _restartScheduledAtTicks),
                    DateTimeKind.Utc);
                var age = scheduledAt.Ticks == 0
                    ? TimeSpan.Zero
                    : DateTime.UtcNow - scheduledAt;
                if (age < RestartGuardLifetime)
                    throw new ArgumentException(
                        "A restart is already scheduled; wait for ArcGIS Pro "
                        + "to come back" + (scheduledAt.Ticks == 0 ? "."
                            : " (if the helper died, retry after "
                              + DateTime.UtcNow
                                  .Add(RestartGuardLifetime - age)
                                  .ToString("yyyy-MM-dd HH:mm:ss") + " UTC)."));
                // Stale guard from a helper that never finished: take it over.
                if (Interlocked.CompareExchange(ref _restartScheduled, 0, 1) != 0
                    || Interlocked.CompareExchange(ref _restartScheduled, 1, 0) != 0)
                    throw new ArgumentException(
                        "A restart is already scheduled; wait for ArcGIS Pro to come back.");
            }
            Interlocked.Exchange(ref _restartScheduledAtTicks,
                DateTime.UtcNow.Ticks);

            var save = parameters.GetBool("save", true);
            // The reply needs time to leave the HTTP socket before the window
            // starts closing; anything under a few seconds races the response.
            var delay = Math.Max(8, parameters.GetInt("delay_seconds", 10));
            var projectPath = parameters.GetString("project");
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                try { projectPath = Project.Current?.URI; }
                catch { projectPath = null; }
            }

            try
            {
                if (save)
                {
                    if (Project.Current == null)
                        throw new InvalidOperationException(
                            "No project is open, so there is nothing to save.");
                    var saved = await MapCommands.SaveAllOnUiThread()
                        .ConfigureAwait(false);
                    if (!saved)
                        throw new InvalidOperationException(
                            "ArcGIS Pro did not confirm the save; restart "
                            + "cancelled so nothing is discarded.");
                }

                var runtime = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "ArcGIS-MCP", "runtime");
                Directory.CreateDirectory(runtime);
                var script = Path.Combine(runtime, "restart_pro_mcp.ps1");
                using (var stream = typeof(SessionCommands).Assembly
                           .GetManifestResourceStream(
                               "ArcGISProMCP.Resources.restart_pro.ps1"))
                {
                    if (stream == null)
                        throw new InvalidOperationException(
                            "This build is missing the embedded restart script.");
                    using (var reader = new StreamReader(stream))
                        File.WriteAllText(script, reader.ReadToEnd(),
                            new UTF8Encoding(false));
                }

                var pid = Process.GetCurrentProcess().Id;
                var helper = BuildHelperCommand(script, pid, delay, projectPath);
                // WMI first: the helper is then a child of WmiPrvSE and
                // survives even when ArcGIS Pro sits in a kill-on-close job.
                var spawned = TrySpawnViaWmi(helper) || SpawnDirect(helper);
                if (!spawned)
                    throw new InvalidOperationException(
                        "Could not start the restart helper.");

                return new Dictionary<string, object>
                {
                    ["scheduled"] = true,
                    ["saved"] = save,
                    ["restart_in_seconds"] = delay,
                    ["project"] = projectPath,
                    ["script"] = script,
                    ["note"] = "ArcGIS Pro closes and reopens; the helper "
                        + "verifies the bridge and writes "
                        + "%LOCALAPPDATA%\\ArcGIS-MCP\\restart.ok when done.",
                };
            }
            catch
            {
                Interlocked.Exchange(ref _restartScheduled, 0);
                throw;
            }
        }

        private static string BuildHelperCommand(string script, int pid,
                                                 int delay, string project)
        {
            var command = "-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden "
                + $"-File \"{script}\" -ProPid {pid} -DelaySeconds {delay}";
            if (!string.IsNullOrWhiteSpace(project))
                command += $" -Project \"{project}\"";
            return "powershell.exe " + command;
        }

        private static bool TrySpawnViaWmi(string helperCommand)
        {
            try
            {
                var quoted = "'" + helperCommand.Replace("'", "''") + "'";
                var outer = "$ErrorActionPreference='Stop'; $cmd = " + quoted
                    + "; $r = Invoke-CimMethod -ClassName Win32_Process "
                    + "-MethodName Create -Arguments @{ CommandLine = $cmd }; "
                    + "Write-Output ('ReturnValue=' + $r.ReturnValue "
                    + "+ ';ProcessId=' + $r.ProcessId)";
                var encoded = Convert.ToBase64String(
                    Encoding.Unicode.GetBytes(outer));
                var start = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -ExecutionPolicy Bypass "
                        + "-WindowStyle Hidden -EncodedCommand " + encoded,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                };
                using (var process = Process.Start(start))
                {
                    if (process == null) return false;
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit(20000);
                    return output.Contains("ReturnValue=0");
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool SpawnDirect(string helperCommand)
        {
            var start = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = helperCommand.Substring("powershell.exe ".Length),
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            return Process.Start(start) != null;
        }

        private static object Diagnose(Params parameters)
        {
            var checks = new List<Dictionary<string, object>>();
            var ok = true;

            void Check(string name, Func<object> probe)
            {
                var entry = new Dictionary<string, object> { ["check"] = name };
                try
                {
                    entry["result"] = probe();
                    entry["status"] = "ok";
                }
                catch (Exception exception)
                {
                    entry["status"] = "failed";
                    entry["error"] = $"{exception.GetType().Name}: {exception.Message}";
                    ok = false;
                }
                checks.Add(entry);
            }

            Check("addin", () => new Dictionary<string, object>
            {
                ["implementation"] = "C# add-in -- commands run on the MCT, no Python "
                                   + "dispatcher needed",
                ["commands"] = CommandRouter.Count,
                ["port"] = MCPModule.Current?.Server?.Port,
            });

            Check("project", () =>
            {
                var project = MapHelpers.RequireProject();
                return new Dictionary<string, object>
                {
                    ["path"] = project.URI,
                    ["default_gdb"] = project.DefaultGeodatabasePath,
                    ["map_count"] = project.GetItems<MapProjectItem>().Count(),
                };
            });

            Check("active_map", () =>
            {
                var map = MapHelpers.ResolveMap(parameters);
                return new Dictionary<string, object>
                {
                    ["map"] = map.Name,
                    ["layers"] = map.GetLayersAsFlattenedList().Count,
                };
            });

            Check("map_view", () =>
            {
                var view = MapView.Active;
                if (view == null)
                    throw new InvalidOperationException(
                        "No active map view -- camera and export commands need one.");
                return new Dictionary<string, object> { ["scale"] = view.Camera?.Scale };
            });

            Check("python_fallback", () => PythonFallback.Describe(probe: true));

            return new Dictionary<string, object>
            {
                ["checks"] = checks,
                ["ok"] = ok,
                ["command_count"] = CommandRouter.Count,
            };
        }

        /// <summary>
        /// Runs a list of commands in order. Each step is dispatched exactly as
        /// if it had arrived on its own, so a step this add-in does not
        /// implement still reaches the Python bridge.
        ///
        /// The saving is the round trip, not the dispatch: a command costs
        /// milliseconds here, and the socket round trip costs more than that.
        /// </summary>
        private static async Task<object> RunBatchAsync(Params parameters)
        {
            // AsyncLocal, not a static flag: concurrent batches from different
            // clients are fine; only a batch nested in its own steps is not.
            if (BatchDepth.Value > 0)
                throw new InvalidOperationException(
                    "run_batch cannot contain another run_batch.");
            BatchDepth.Value++;

            try
            {
                var continueOnError = parameters.GetBool("continue_on_error");
                var results = new List<Dictionary<string, object>>();
                var failed = 0;
                var step = 0;

                foreach (var item in parameters.GetObjectList("commands"))
                {
                    step++;
                    var command = item.Require("command");
                    var stepParameters = item.Raw("params");

                    var entry = new Dictionary<string, object>
                    {
                        ["step"] = step,
                        ["command"] = command,
                    };

                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    try
                    {
                        entry["data"] = await CommandRouter.DispatchAsync(
                            command, new Params(stepParameters, inBatch: true),
                            stepParameters)
                            .ConfigureAwait(false);
                        entry["success"] = true;
                    }
                    catch (Exception exception)
                    {
                        failed++;
                        entry["success"] = false;
                        entry["error"] = Unwrap(exception).Message;
                        entry["ms"] = stopwatch.ElapsedMilliseconds;
                        results.Add(entry);

                        if (!continueOnError)
                            return new Dictionary<string, object>
                            {
                                ["results"] = results,
                                ["completed"] = results.Count,
                                ["failed"] = failed,
                                ["stopped_at"] = step,
                                ["message"] = $"Step {step} ({command}) failed and the "
                                              + "batch stopped. Pass continue_on_error "
                                              + "to run the rest anyway.",
                            };
                        continue;
                    }

                    entry["ms"] = stopwatch.ElapsedMilliseconds;
                    results.Add(entry);
                }

                return new Dictionary<string, object>
                {
                    ["results"] = results,
                    ["completed"] = results.Count,
                    ["failed"] = failed,
                    ["execution"] = "serial",
                    ["note"] = "Steps run one at a time on Pro's main thread; writes "
                             + "to the same map or dataset therefore cannot interleave.",
                };
            }
            finally
            {
                BatchDepth.Value--;
            }
        }

        private static readonly AsyncLocal<int> BatchDepth = new AsyncLocal<int>();

        /// <summary>
        /// An awaited handler surfaces as an AggregateException whose message
        /// says nothing useful; the caller wants the message underneath.
        /// </summary>
        private static Exception Unwrap(Exception exception)
        {
            while (exception is AggregateException aggregate
                   && aggregate.InnerExceptions.Count == 1)
                exception = aggregate.InnerExceptions[0];
            return exception;
        }
    }
}
