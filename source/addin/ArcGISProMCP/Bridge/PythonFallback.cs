using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGISProMCP.Bridge
{
    /// <summary>
    /// Forwards commands this add-in does not implement to the Python bridge.
    ///
    /// That bridge still owns arcpy -- execute_arcpy_code above all, the one
    /// tool that can do anything the typed commands do not cover. It listens
    /// on its own port so the two can run side by side, and it is optional:
    /// when it is not there the caller gets a clear message rather than a
    /// hang.
    ///
    /// v1.1.6 status-query rule: get_pump_status is answered even when the
    /// bridge is down. A refused connection is a certain "no listener", so
    /// the pump cannot be running -- returning that fact beats failing the
    /// one query whose whole job is to report it. Execution commands keep
    /// failing hard; a synthetic answer there would hide real trouble.
    /// </summary>
    internal static class PythonFallback
    {
        public const int DefaultPort = 6511;

        private const int ConnectTimeoutMs = 2000;
        private const int StatusReadTimeoutMs = 5000;
        private const int DownCacheSeconds = 5;

        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
        private static int _port = DefaultPort;
        private static int _forwarded;
        private static string _lastError;
        private static string _lastErrorCode;
        private static string _lastErrorAtUtc;

        // A refused status query is remembered briefly so polling
        // get_pump_status does not pay a connect attempt every time. Only a
        // clear connection refusal is cached -- a timeout is not certainty,
        // and execution commands never consult this cache.
        private static readonly object DownLock = new object();
        private static DateTime _downUntilUtc = DateTime.MinValue;

        public static int Port
        {
            get => _port;
            set => _port = value;
        }

        private static bool IsStatusQuery(string command)
        {
            return string.Equals(command, "get_pump_status", StringComparison.OrdinalIgnoreCase);
        }

        private static string InstallHint()
        {
            return "Start it from the ArcGIS Pro Python window with:  import mcp_bridge; "
                 + "mcp_bridge.start_server(port=" + _port + "); mcp_bridge.start_pump()";
        }

        private static bool BridgeKnownDown()
        {
            lock (DownLock) return DateTime.UtcNow < _downUntilUtc;
        }

        private static void MarkDown(bool refused)
        {
            if (!refused) return;
            lock (DownLock) _downUntilUtc = DateTime.UtcNow.AddSeconds(DownCacheSeconds);
        }

        private static void ClearDown()
        {
            lock (DownLock) _downUntilUtc = DateTime.MinValue;
        }

        private static void RecordError(string code, string message)
        {
            _lastErrorCode = code;
            _lastError = message;
            _lastErrorAtUtc = DateTime.UtcNow.ToString("o");
        }

        /// <summary>
        /// The answer get_pump_status gets when the bridge is provably down:
        /// a fact, not an error. The shape matches what the bridge itself
        /// returns so callers read the same keys in every state.
        /// </summary>
        private static object DownAnswer()
        {
            return new Dictionary<string, object>
            {
                ["running"] = false,
                ["bridge_listening"] = false,
                ["pump"] = "unknown",
                ["code"] = "BRIDGE_NOT_LISTENING",
                ["checked_at"] = DateTime.UtcNow.ToString("o"),
                ["hint"] = "Not running: the Python bridge is not listening on 127.0.0.1:"
                         + _port + ", so the pump state cannot be read. " + InstallHint(),
            };
        }

        /// <summary>
        /// What the fallback looks like from outside. With probe:true it also
        /// makes one bounded connect, so a caller can tell "bridge down" from
        /// "bridge up" even before any command has failed.
        /// </summary>
        public static object Describe(bool probe = false)
        {
            var data = new Dictionary<string, object>
            {
                ["target"] = "Python bridge (arcpy)",
                ["host"] = "127.0.0.1",
                ["port"] = _port,
                ["forwarded_commands"] = _forwarded,
                ["last_error"] = _lastError,
                ["last_error_code"] = _lastErrorCode,
                ["last_error_at"] = _lastErrorAtUtc,
                ["note"] = "Unknown commands are forwarded here, which is where "
                         + "execute_arcpy_code and any not-yet-ported command lives. "
                         + "A listening port does not imply the pump is running.",
            };
            if (probe)
            {
                var result = Probe(300);
                data["bridge_listening"] = result["listening"];
                data["probe_ms"] = result["elapsed_ms"];
                if (result.TryGetValue("error", out var error)) data["probe_error"] = error;
                data["hint"] = (bool)result["listening"]
                    ? "Bridge is listening. Ask get_pump_status for the pump itself."
                    : "Bridge is not listening. " + InstallHint();
            }
            return data;
        }

        /// <summary>
        /// One bounded TCP connect against the bridge port. It only proves
        /// that something listens -- the pump's own state needs the bridge's
        /// answer (get_pump_status).
        /// </summary>
        public static Dictionary<string, object> Probe(int budgetMs)
        {
            var watch = Stopwatch.StartNew();
            var listening = false;
            string error = null;
            try
            {
                using (var client = new TcpClient())
                {
                    var connect = client.ConnectAsync("127.0.0.1", _port);
                    try
                    {
                        if (!connect.Wait(budgetMs))
                            error = "connect timed out after " + budgetMs + " ms";
                        else
                            listening = client.Connected;
                    }
                    catch (AggregateException aggregate)
                    {
                        error = aggregate.GetBaseException().Message;
                    }
                }
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }
            watch.Stop();
            var data = new Dictionary<string, object>
            {
                ["listening"] = listening,
                ["elapsed_ms"] = watch.ElapsedMilliseconds,
            };
            if (error != null) data["error"] = error;
            return data;
        }

        public static async Task<object> SendAsync(string command, JsonElement parameters)
        {
            // Status query answered from the recent refusal cache: the port
            // refused a connection seconds ago, so the pump cannot be up.
            if (IsStatusQuery(command) && BridgeKnownDown())
                return DownAnswer();

            var request = new Dictionary<string, object>
            {
                ["id"] = 1,
                ["command"] = command,
                ["params"] = parameters.ValueKind == JsonValueKind.Undefined
                    ? new Dictionary<string, object>()
                    : (object)parameters,
            };
            var line = JsonSerializer.Serialize(request, Protocol.SerializerOptions) + "\n";

            // One request at a time: the Python bridge is single-threaded for
            // arcpy anyway, and this keeps the socket handling simple.
            await Gate.WaitAsync().ConfigureAwait(false);
            var errorRecorded = false;
            try
            {
                using (var client = new TcpClient())
                {
                    var (connected, refused, connectError) =
                        await TryConnectAsync(client).ConfigureAwait(false);
                    if (!connected)
                    {
                        MarkDown(refused);
                        RecordError("BRIDGE_NOT_LISTENING",
                            $"Python bridge not reachable on 127.0.0.1:{_port}: {connectError}");
                        errorRecorded = true;
                        if (IsStatusQuery(command))
                            return DownAnswer();
                        throw new InvalidOperationException(
                            $"'{command}' is not implemented by the ArcGIS Pro add-in, and the "
                            + $"Python bridge is not listening on 127.0.0.1:{_port} "
                            + "[code=BRIDGE_NOT_LISTENING]. " + InstallHint());
                    }
                    ClearDown();

                    using (var stream = client.GetStream())
                    {
                        var payload = Encoding.UTF8.GetBytes(line);
                        await stream.WriteAsync(payload, 0, payload.Length).ConfigureAwait(false);
                        await stream.FlushAsync().ConfigureAwait(false);

                        var read = ReadLineAsync(stream);
                        // Status reads are bounded; execute_arcpy_code and the
                        // like can legitimately run for minutes and keep the
                        // unbounded wait they always had.
                        if (IsStatusQuery(command)
                            && await Task.WhenAny(read, Task.Delay(StatusReadTimeoutMs))
                                   .ConfigureAwait(false) != read)
                        {
                            RecordError("BRIDGE_TIMEOUT",
                                $"{command}: the bridge did not answer within {StatusReadTimeoutMs} ms");
                            errorRecorded = true;
                            throw new InvalidOperationException(
                                $"'{command}': the Python bridge accepted the connection but did "
                                + $"not answer within {StatusReadTimeoutMs} ms [code=BRIDGE_TIMEOUT].");
                        }

                        var response = await read.ConfigureAwait(false);
                        _forwarded++;
                        return Unwrap(command, response);
                    }
                }
            }
            catch (Exception exception)
            {
                if (!errorRecorded) RecordError(Classify(exception), exception.Message);
                throw;
            }
            finally
            {
                Gate.Release();
            }
        }

        private static async Task<(bool connected, bool refused, string error)> TryConnectAsync(TcpClient client)
        {
            var connect = client.ConnectAsync("127.0.0.1", _port);
            try
            {
                if (await Task.WhenAny(connect, Task.Delay(ConnectTimeoutMs)).ConfigureAwait(false) != connect)
                    return (false, false, $"connect timed out after {ConnectTimeoutMs} ms");
                await connect.ConfigureAwait(false);
                return (client.Connected, false, client.Connected ? null : "connect reported no socket");
            }
            catch (SocketException exception)
            {
                return (false,
                        exception.SocketErrorCode == SocketError.ConnectionRefused,
                        exception.Message);
            }
            catch (Exception exception)
            {
                return (false, false, exception.Message);
            }
        }

        /// <summary>
        /// Names the failure so callers (and last_error_code) can tell a
        /// missing bridge from a bridge that answered with an error, and
        /// both from a command the bridge does not know.
        /// </summary>
        private static string Classify(Exception exception)
        {
            var message = exception.Message ?? string.Empty;
            if (message.IndexOf("closed the connection", StringComparison.OrdinalIgnoreCase) >= 0)
                return "BRIDGE_CLOSED";
            if (message.IndexOf("unknown command", StringComparison.OrdinalIgnoreCase) >= 0)
                return "UNSUPPORTED";
            return "BRIDGE_ERROR";
        }

        private static async Task<string> ReadLineAsync(NetworkStream stream)
        {
            var buffer = new byte[64 * 1024];
            var builder = new StringBuilder();
            while (true)
            {
                var read = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                if (read <= 0)
                    throw new InvalidOperationException("The Python bridge closed the connection.");
                builder.Append(Encoding.UTF8.GetString(buffer, 0, read));
                var text = builder.ToString();
                var newline = text.IndexOf('\n');
                if (newline >= 0)
                    return text.Substring(0, newline);
            }
        }

        /// <summary>
        /// The Python bridge answers in the same envelope we do, so unwrap it
        /// rather than nesting one response inside another.
        /// </summary>
        private static object Unwrap(string command, string response)
        {
            using (var document = JsonDocument.Parse(response))
            {
                var root = document.RootElement;
                var succeeded = root.TryGetProperty("success", out var success)
                                && success.ValueKind == JsonValueKind.True;
                if (!succeeded)
                {
                    var message = root.TryGetProperty("error", out var error)
                        ? error.GetString()
                        : "unknown error";
                    throw new InvalidOperationException(
                        $"Python bridge error in {command}: {message}");
                }
                return root.TryGetProperty("data", out var data)
                    ? JsonSerializer.Deserialize<object>(data.GetRawText(), Protocol.SerializerOptions)
                    : null;
            }
        }
    }
}
