using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ArcGISProMCP.Bridge;

namespace ArcGISProMCP.Commands
{
    /// <summary>
    /// Thin MCP wrappers over the arcgis-distill CLI (design section 4.5).
    ///
    /// All business logic lives in one place, the Python package, so the CLI
    /// and the MCP tools cannot drift. If the CLI is not installed on this
    /// machine the tool fails with a hint instead of pretending to work.
    /// </summary>
    internal static class TemplateCommands
    {
        private const string Group = "Template";

        public static void Register()
        {
            CommandRouter.Register("template_list", Group,
                "List distilled layout templates in the local library, optionally "
                + "filtered by status or tag.",
                List, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_get", Group,
                "Read one distilled template spec (layout, palette, typography, pitfalls).",
                Get, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_import", Group,
                "Import a sample map figure into the template library as a draft.",
                Import, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_measure", Group,
                "Measure a template's sample figure (aspect, frame lines, palette, DPI) "
                + "and write the measured block.",
                Measure, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_patch", Group,
                "Submit semantic fields (regions, legend, typography, pitfalls) as a "
                + "validated merge; protected fields such as measured are rejected.",
                Patch, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_validate", Group,
                "Run schema and geometric consistency checks on a template.",
                Validate, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_review", Group,
                "Record a human review decision for a validated template.",
                Review, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_approve", Group,
                "Approve a reviewed, validated template so it becomes ready to apply.",
                Approve, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_preview", Group,
                "Render the template as a wireframe PNG next to the sample figure.",
                Preview, runOnMct: false, lane: ExecutionLane.Inline);
            CommandRouter.Register("template_apply", Group,
                "Write a ready template into a mapping context file for the assistant.",
                Apply, runOnMct: false, lane: ExecutionLane.Inline);
        }

        // --- handlers --------------------------------------------------------

        private static object List(Params parameters)
        {
            var args = new List<string> { "list" };
            var status = parameters.GetString("status");
            if (!string.IsNullOrWhiteSpace(status)) { args.Add("--status"); args.Add(status); }
            var tag = parameters.GetString("tag");
            if (!string.IsNullOrWhiteSpace(tag)) { args.Add("--tag"); args.Add(tag); }
            return RunJson(args);
        }

        private static object Get(Params parameters)
        {
            return RunJson(new List<string> { "get", parameters.Require("template_id") });
        }

        private static object Import(Params parameters)
        {
            var args = new List<string>
            {
                "import", parameters.Require("image"),
                "--name", parameters.Require("name"),
            };
            foreach (var tag in parameters.GetStringList("tags") ?? new List<string>())
            {
                args.Add("--tag");
                args.Add(tag);
            }
            var width = parameters.GetOptionalDouble("width_in");
            if (width.HasValue)
            {
                args.Add("--width-in");
                args.Add(width.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            var id = parameters.GetString("template_id");
            if (!string.IsNullOrWhiteSpace(id)) { args.Add("--id"); args.Add(id); }
            return RunJson(args);
        }

        private static object Measure(Params parameters)
        {
            var args = new List<string> { "measure", parameters.Require("template_id") };
            var width = parameters.GetOptionalDouble("width_in");
            if (width.HasValue)
            {
                args.Add("--width-in");
                args.Add(width.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            if (parameters.GetBool("force")) args.Add("--force");
            return RunJson(args);
        }

        private static object Patch(Params parameters)
        {
            var templateId = parameters.Require("template_id");
            var args = new List<string> { "patch", templateId };
            var patchFile = parameters.GetString("patch_file");
            string temporary = null;
            try
            {
                if (string.IsNullOrWhiteSpace(patchFile))
                {
                    var patch = parameters.GetObject("patch");
                    if (patch == null || patch.Count == 0)
                        throw new ArgumentException(
                            "template_patch needs 'patch' (object) or 'patch_file' (path).");
                    temporary = Path.Combine(Path.GetTempPath(), "arcgis-agentkit",
                        "patch-" + Guid.NewGuid().ToString("N") + ".json");
                    Directory.CreateDirectory(Path.GetDirectoryName(temporary));
                    File.WriteAllText(temporary,
                        JsonSerializer.Serialize(patch), new UTF8Encoding(false));
                    patchFile = temporary;
                }
                args.Add("--file");
                args.Add(patchFile);
                return RunJson(args);
            }
            finally
            {
                if (temporary != null)
                {
                    try { File.Delete(temporary); } catch { }
                }
            }
        }

        private static object Validate(Params parameters)
        {
            return RunJson(new List<string> { "validate", parameters.Require("template_id") });
        }

        private static object Review(Params parameters)
        {
            var args = new List<string>
            {
                "review", parameters.Require("template_id"),
                "--by", parameters.Require("by"),
            };
            var notes = parameters.GetString("notes");
            if (!string.IsNullOrWhiteSpace(notes))
            {
                args.Add("--notes");
                args.Add(notes);
            }
            return RunJson(args);
        }

        private static object Approve(Params parameters)
        {
            var args = new List<string> { "approve", parameters.Require("template_id") };
            if (parameters.GetBool("force")) args.Add("--force");
            return RunJson(args);
        }

        private static object Preview(Params parameters)
        {
            var templateId = parameters.Require("template_id");
            var outPath = parameters.GetString("out");
            var temporary = false;
            if (string.IsNullOrWhiteSpace(outPath))
            {
                outPath = Path.Combine(Path.GetTempPath(), "arcgis-agentkit",
                    "preview-" + Guid.NewGuid().ToString("N") + ".png");
                temporary = true;
            }
            try
            {
                var args = new List<string> { "preview", templateId, "--out", outPath };
                var width = parameters.GetInt("width", 900);
                args.Add("--width");
                args.Add(width.ToString());
                var payload = RunJson(args);
                if (!File.Exists(outPath))
                    throw new InvalidOperationException("template_preview produced no PNG.");
                var data = new Dictionary<string, object>(payload)
                {
                    ["image_base64"] = Convert.ToBase64String(File.ReadAllBytes(outPath)),
                    ["image_format"] = "png",
                };
                return data;
            }
            finally
            {
                if (temporary)
                {
                    try { File.Delete(outPath); } catch { }
                }
            }
        }

        private static object Apply(Params parameters)
        {
            var args = new List<string> { "apply", parameters.Require("template_id") };
            var outPath = parameters.GetString("out");
            if (!string.IsNullOrWhiteSpace(outPath))
            {
                args.Add("--out");
                args.Add(outPath);
            }
            return RunJson(args);
        }

        // --- CLI plumbing ----------------------------------------------------

        private sealed class CliResult
        {
            public int ExitCode;
            public string Stdout;
            public string Stderr;
        }

        private static Dictionary<string, object> RunJson(List<string> arguments)
        {
            var result = RunCli(arguments);
            var line = (result.Stdout ?? string.Empty)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault();
            Dictionary<string, object> payload = null;
            if (!string.IsNullOrWhiteSpace(line))
            {
                try
                {
                    payload = JsonSerializer.Deserialize<Dictionary<string, object>>(line);
                }
                catch (JsonException) { }
            }

            if (result.ExitCode != 0)
            {
                var message = payload != null && payload.TryGetValue("message", out var value)
                    ? value?.ToString()
                    : (string.IsNullOrWhiteSpace(result.Stderr) ? result.Stdout : result.Stderr);
                var hint = payload != null && payload.TryGetValue("hint", out var hintValue)
                    ? hintValue?.ToString() : null;
                throw new InvalidOperationException(
                    (message ?? "arcgis-distill failed").Trim()
                    + (string.IsNullOrWhiteSpace(hint) ? string.Empty : " Hint: " + hint));
            }
            if (payload == null)
            {
                throw new InvalidOperationException(
                    "arcgis-distill returned no JSON. Is the CLI up to date? "
                    + (result.Stderr ?? string.Empty).Trim());
            }
            return payload;
        }

        private static CliResult RunCli(List<string> arguments, int timeoutMs = 240000)
        {
            arguments.Add("--json");
            var (fileName, cliArguments, environment, display) = Resolve(arguments);
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                Arguments = GuardClient.Quote(cliArguments),
            };
            if (environment != null)
            {
                foreach (var pair in environment)
                    startInfo.Environment[pair.Key] = pair.Value;
            }

            using (var process = new Process { StartInfo = startInfo })
            {
                if (!process.Start())
                    throw new InvalidOperationException("could not start " + display);
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(true); } catch { }
                    throw new InvalidOperationException(
                        "arcgis-distill timed out after " + timeoutMs / 1000 + " s.");
                }
                return new CliResult
                {
                    ExitCode = process.ExitCode,
                    Stdout = stdout.GetAwaiter().GetResult(),
                    Stderr = stderr.GetAwaiter().GetResult(),
                };
            }
        }

        private static (string, List<string>, Dictionary<string, string>, string) Resolve(
            List<string> arguments)
        {
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var launcher = Environment.GetEnvironmentVariable("ARCGIS_AGENTKIT_DISTILL_LAUNCHER");
            if (string.IsNullOrWhiteSpace(launcher))
            {
                var candidate = Path.Combine(local, "ArcGIS Pro MCP", "distill",
                                             "run-distill.ps1");
                if (File.Exists(candidate)) launcher = candidate;
            }
            if (!string.IsNullOrWhiteSpace(launcher) && File.Exists(launcher))
            {
                var scriptArgs = new List<string>
                {
                    "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", launcher,
                };
                scriptArgs.AddRange(arguments);
                return (GuardClient.PowerShelPath(), scriptArgs, null, launcher);
            }

            var python = Environment.GetEnvironmentVariable("ARCGIS_AGENTKIT_PYTHON");
            if (string.IsNullOrWhiteSpace(python))
            {
                var programFiles = Environment.GetEnvironmentVariable("ProgramW6432")
                                   ?? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                python = Path.Combine(programFiles, "ArcGIS", "Pro", "bin", "Python",
                                      "envs", "arcgispro-py3", "python.exe");
            }
            var modulePath = Environment.GetEnvironmentVariable("ARCGIS_AGENTKIT_PYTHONPATH");
            if (string.IsNullOrWhiteSpace(modulePath))
            {
                var candidate = Path.Combine(local, "ArcGIS Pro MCP", "distill");
                if (Directory.Exists(candidate)) modulePath = candidate;
            }
            if (File.Exists(python) && !string.IsNullOrWhiteSpace(modulePath)
                && Directory.Exists(modulePath))
            {
                var pythonArgs = new List<string> { "-m", "arcgis_agentkit.distill" };
                pythonArgs.AddRange(arguments);
                var environment = new Dictionary<string, string>
                {
                    ["PYTHONPATH"] = modulePath,
                    ["PYTHONUTF8"] = "1",
                    ["PYTHONIOENCODING"] = "utf-8",
                };
                return (python, pythonArgs, environment, python + " -m arcgis_agentkit.distill");
            }

            var onPath = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                .Split(Path.PathSeparator)
                .SelectMany(directory => new[] { "arcgis-distill.exe", "arcgis-distill.cmd" }
                    .Select(name => Path.Combine(directory.Trim('"'), name)))
                .FirstOrDefault(File.Exists);
            if (onPath != null)
            {
                if (onPath.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
                {
                    var cmdArgs = new List<string> { "/c", onPath };
                    cmdArgs.AddRange(arguments);
                    return (Path.Combine(Environment.GetEnvironmentVariable("ComSpec")
                                         ?? @"C:\Windows\System32\cmd.exe"), cmdArgs,
                            null, onPath);
                }
                return (onPath, arguments, null, onPath);
            }

            throw new InvalidOperationException(
                "arcgis-distill is not installed. Install the AgentKit Python package "
                + "(pipx install arcgis-pro-agentkit) or set ARCGIS_AGENTKIT_DISTILL_LAUNCHER "
                + "to the bundled run-distill.ps1.");
        }
    }
}
