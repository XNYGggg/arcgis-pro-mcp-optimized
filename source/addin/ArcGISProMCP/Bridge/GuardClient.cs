using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;

namespace ArcGISProMCP.Bridge
{
    /// <summary>
    /// Client for the on-demand dialog guard (AgentKit guard engine 2.0).
    ///
    /// The engine is a short-lived PowerShell process. Arming and settling are
    /// a file handshake, so the same protocol is used by the Python CLI and
    /// this add-in:
    ///
    ///   sessions/&lt;token&gt;/ready.json   engine armed
    ///   sessions/&lt;token&gt;/settle       operation finished
    ///   sessions/&lt;token&gt;/result.json  engine summary on exit
    ///
    /// A missing engine or a slow arm never blocks the operation: the lease
    /// comes back not-ready with a warning that the caller puts in its result.
    /// </summary>
    internal static class GuardClient
    {
        public const string Version = "2.0.1";

        private static readonly string[] GuardedToolboxExtensions =
            { ".pyt", ".tbx", ".atbx" };

        public static string SessionRoot
        {
            get
            {
                var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(local, "ArcGIS-AgentKit", "guard", "sessions");
            }
        }

        public static string EnginePath
        {
            get
            {
                var candidates = new List<string>();
                var explicitEngine = Environment.GetEnvironmentVariable("ARCGIS_AGENTKIT_GUARD_ENGINE");
                if (!string.IsNullOrWhiteSpace(explicitEngine)) candidates.Add(explicitEngine);

                var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                candidates.Add(Path.Combine(local, "ArcGIS-AgentKit", "guard", "engine", "guard.ps1"));
                candidates.Add(Path.Combine(local, "ArcGIS Pro MCP", "guard", "guard.ps1"));

                // Development checkout: addin/bin/... is not a stable anchor, so
                // also accept an engine beside the add-in assembly.
                try
                {
                    var beside = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "guard", "guard.ps1");
                    candidates.Add(beside);
                }
                catch { }

                return candidates.FirstOrDefault(File.Exists);
            }
        }

        public static bool ShouldGuardToolbox(string toolboxPath)
        {
            if (string.IsNullOrWhiteSpace(toolboxPath)) return false;
            // A tool name is "C:\x\RunCode.pyt\Run"; the extension lives on the
            // directory part, not on the whole string.
            var directory = Path.GetDirectoryName(toolboxPath);
            var extension = Path.GetExtension(directory ?? string.Empty);
            if (string.IsNullOrEmpty(extension))
                extension = Path.GetExtension(toolboxPath);
            return GuardedToolboxExtensions.Any(
                item => string.Equals(item, extension, StringComparison.OrdinalIgnoreCase));
        }

        public static GuardLease Arm(string reason, int timeoutSeconds = 180)
        {
            return ArmScope("op", reason, timeoutSeconds, alertOnly: false);
        }

        public static GuardLease ArmScope(string scope, string reason, int timeoutSeconds,
                                          bool alertOnly)
        {
            var token = DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" +
                        Guid.NewGuid().ToString("N").Substring(0, 8);
            var engine = EnginePath;
            if (engine == null)
            {
                return new GuardLease(token, SessionRoot, false,
                    "guard engine not found; the trust dialog must be clicked by hand");
            }

            var sessionDir = Path.Combine(SessionRoot, token);
            try
            {
                Directory.CreateDirectory(Path.Combine(SessionRoot, "requests"));
                var arguments = new List<string>
                {
                    "-NoProfile", "-ExecutionPolicy", "Bypass",
                    "-File", engine,
                    "-Token", token,
                    "-Scope", scope,
                    "-TimeoutSeconds", Math.Max(10, timeoutSeconds).ToString(),
                    "-SessionRoot", SessionRoot,
                };
                var schema = Path.Combine(Path.GetDirectoryName(engine) ?? string.Empty,
                                          "guard.schema.json");
                if (File.Exists(schema))
                {
                    arguments.Add("-ConfigPath");
                    arguments.Add(schema);
                }
                if (alertOnly) arguments.Add("-AlertOnly");
                if (!string.IsNullOrWhiteSpace(reason))
                {
                    arguments.Add("-Reason");
                    arguments.Add(reason);
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = PowerShelPath(),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = SessionRoot,
                    Arguments = Quote(arguments),
                };
                using (var process = Process.Start(startInfo))
                {
                    // Deliberately not waited for or killed: the engine must
                    // outlive this operation. Wait only for the ready handshake.
                }
            }
            catch (Exception exception)
            {
                return new GuardLease(token, SessionRoot, false,
                    $"guard engine could not start: {exception.Message}");
            }

            var readyPath = Path.Combine(sessionDir, "ready.json");
            var deadline = DateTime.UtcNow.AddMilliseconds(2000);
            while (DateTime.UtcNow < deadline)
            {
                if (File.Exists(readyPath))
                    return new GuardLease(token, SessionRoot, true, null);
                Thread.Sleep(50);
            }
            if (File.Exists(readyPath))
                return new GuardLease(token, SessionRoot, true, null);
            return new GuardLease(token, SessionRoot, false,
                "guard did not become ready in time; the trust dialog may need a manual click");
        }

        public static Dictionary<string, object> Summon(string scope, int timeoutSeconds,
                                                        bool alertOnly, string reason)
        {
            var lease = ArmScope(scope, reason, timeoutSeconds, alertOnly);
            return new Dictionary<string, object>
            {
                ["ok"] = lease.Ready,
                ["token"] = lease.Token,
                ["ready"] = lease.Ready,
                ["engine"] = EnginePath,
                ["engine_version"] = Version,
                ["scope"] = scope,
                ["timeout_s"] = timeoutSeconds,
                ["alert_only"] = alertOnly,
                ["session"] = lease.SessionDir,
                ["warning"] = lease.Warning,
                ["log"] = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ArcGIS-AgentKit", "logs", "guard.jsonl"),
            };
        }

        public static Dictionary<string, object> Status()
        {
            var sessions = new List<object>();
            try
            {
                if (Directory.Exists(SessionRoot))
                {
                    foreach (var directory in Directory.GetDirectories(SessionRoot)
                                 .OrderByDescending(item => item)
                                 .Take(20))
                    {
                        var entry = new Dictionary<string, object>
                        {
                            ["token"] = Path.GetFileName(directory),
                        };
                        foreach (var name in new[] { "ready.json", "settle", "result.json" })
                        {
                            var path = Path.Combine(directory, name);
                            if (!File.Exists(path)) continue;
                            entry[name] = true;
                            if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    using (var document = JsonDocument.Parse(File.ReadAllText(path)))
                                        entry[Path.GetFileNameWithoutExtension(name)] =
                                            JsonSerializer.Deserialize<object>(document.RootElement.GetRawText());
                                }
                                catch { }
                            }
                        }
                        sessions.Add(entry);
                    }
                }
            }
            catch (Exception exception)
            {
                sessions.Add(new Dictionary<string, object> { ["error"] = exception.Message });
            }

            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return new Dictionary<string, object>
            {
                ["ok"] = true,
                ["engine"] = EnginePath,
                ["engine_found"] = EnginePath != null,
                ["engine_version"] = Version,
                ["session_root"] = SessionRoot,
                ["log"] = Path.Combine(local, "ArcGIS-AgentKit", "logs", "guard.jsonl"),
                ["sessions"] = sessions,
            };
        }

        internal static string PowerShelPath()
        {
            var root = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows";
            var candidate = Path.Combine(root, "System32", "WindowsPowerShell", "v1.0",
                                         "powershell.exe");
            return File.Exists(candidate) ? candidate : "powershell.exe";
        }

        internal static string Quote(IEnumerable<string> arguments)
        {
            return string.Join(" ", arguments.Select(argument =>
                "\"" + (argument ?? string.Empty).Replace("\"", "\\\"") + "\""));
        }
    }

    /// <summary>One armed guard. Settle when the operation finishes.</summary>
    internal sealed class GuardLease : IDisposable
    {
        private bool _settled;

        public string Token { get; }
        public string SessionRoot { get; }
        public bool Ready { get; }
        public string Warning { get; }
        public string SessionDir => Path.Combine(SessionRoot, Token);

        public GuardLease(string token, string sessionRoot, bool ready, string warning)
        {
            Token = token;
            SessionRoot = sessionRoot;
            Ready = ready;
            Warning = warning;
        }

        public void Settle(string outcome = "ok")
        {
            if (_settled) return;
            _settled = true;
            try
            {
                Directory.CreateDirectory(SessionDir);
                File.WriteAllText(Path.Combine(SessionDir, "settle"),
                    "{\"outcome\":\"" + outcome + "\",\"at\":" +
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "}");
            }
            catch { }
        }

        public void Dispose() => Settle();
    }
}
