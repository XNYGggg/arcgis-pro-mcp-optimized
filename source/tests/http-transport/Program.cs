using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ArcGISProMCP.Bridge;

// Real transport and schemas, fake GIS dispatcher. No installed add-in is changed.
using var server = new McpHttpServer();
server.Start(16520);
using var http = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:16520/"), Timeout = TimeSpan.FromSeconds(10) };
http.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/event-stream");
var id = 0;
async Task<JsonElement> Call(string method, object parameters)
{
    var current = ++id;
    using var response = await http.PostAsJsonAsync("mcp", new { jsonrpc = "2.0", id = current, method, @params = parameters });
    response.EnsureSuccessStatusCode();
    if (response.Content.Headers.ContentType?.MediaType != "application/json") throw new Exception("Incorrect content type");
    var body = await response.Content.ReadFromJsonAsync<JsonElement>();
    if (body.GetProperty("id").GetInt32() != current) throw new Exception("Incorrect response id");
    return body;
}
var init = await Call("initialize", new { protocolVersion = "2025-03-26", capabilities = new { }, clientInfo = new { name = "test", version = "1" } });
if (init.GetProperty("result").GetProperty("protocolVersion").GetString() != "2025-03-26") throw new Exception("Protocol negotiation");
using var notification = await http.PostAsJsonAsync("mcp", new { jsonrpc = "2.0", method = "notifications/initialized" });
if (notification.StatusCode != HttpStatusCode.Accepted) throw new Exception("Notification status");
await Call("ping", new { });
var list = await Call("tools/list", new { });
var names = list.GetProperty("result").GetProperty("tools").EnumerateArray().Select(t => t.GetProperty("name").GetString()).ToArray();
// The catalog grows; asserting an exact count turned every addition into a
// red build. What matters is that names stay unique and the load-bearing
// tools are present.
if (names.Distinct().Count() != names.Length) throw new Exception("Tool catalog duplicates");
foreach (var required in new[]
{
    "get_project_info", "run_python_toolbox_tool", "run_geoprocessing_tool",
    "execute_arcpy_code", "get_pump_status", "get_queue_status", "set_environment",
})
    if (!names.Contains(required)) throw new Exception("Tool catalog missing " + required);
var result = await Call("tools/call", new { name = "get_project_info", arguments = new { } });
if (!result.GetProperty("result").GetProperty("content")[0].GetProperty("text").GetString().Contains("stub-project")) throw new Exception("Dispatch");
var error = await Call("tools/call", new { name = "missing_tool", arguments = new { } });
if (!error.GetProperty("result").GetProperty("isError").GetBoolean()) throw new Exception("Tool error");
using var get = await http.GetAsync("mcp");
if (get.StatusCode != HttpStatusCode.MethodNotAllowed) throw new Exception("GET must reject unsupported SSE");
Console.WriteLine("PASS HTTP: initialize, notification, ping, tool catalog, dispatch, tool error, GET 405");

namespace ArcGISProMCP.Bridge
{
    internal static class CommandRouter
    {
        public static Task<object> DispatchAsync(string name, Params parameters, JsonElement raw)
        {
            if (name != "get_project_info") throw new ArgumentException("Unknown command");
            return Task.FromResult<object>(new { name = "stub-project" });
        }
    }
}
