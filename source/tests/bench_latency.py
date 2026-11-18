"""v1.1.6 fast-path latency probe: ping, a read command, and RunCode hot/cold.

Measures wall time over MCP HTTP plus the add-in's own elapsed_seconds, so a
guard warm-up or GP launch cost shows up separately from transport overhead.
"""
import json
import statistics
import urllib.request

URL = "http://127.0.0.1:6520/mcp"
RUNCODE = (r"E:\ai工具\claude code_app\project_total\01_project_arcgispro"
           r"\.claude\arcgis_mcp_runtime\RunCode.pyt")
SCRIPT = (r"E:\ai工具\claude code_app\project_total\01_project_arcgispro"
          r"\release\v1.1.6-src\tests\bench_tiny.py")
seq = 0


def call(method, params=None, notification=False):
    global seq
    seq += 1
    payload = {"jsonrpc": "2.0", "method": method, "params": params or {}}
    if not notification:
        payload["id"] = seq
    request = urllib.request.Request(
        URL, json.dumps(payload).encode(),
        {"Content-Type": "application/json",
         "Accept": "application/json, text/event-stream",
         "MCP-Protocol-Version": "2025-03-26"})
    with urllib.request.urlopen(request, timeout=180) as response:
        if notification:
            return None
        return json.load(response)


def tool(name, arguments=None):
    body = call("tools/call", {"name": name, "arguments": arguments or {}})
    result = body.get("result", {})
    texts = [item.get("text", "") for item in result.get("content", [])
             if item.get("type") == "text"]
    text = "\n".join(texts)
    if result.get("isError"):
        raise RuntimeError(text)
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        return text


def stats(label, values):
    values = [v for v in values if v is not None]
    if not values:
        print(label + ": no data")
        return
    ordered = sorted(values)
    p50 = statistics.median(ordered)
    p95 = ordered[min(len(ordered) - 1, int(round(0.95 * (len(ordered) - 1))))]
    print("%s: n=%d p50=%.1f ms p95=%.1f ms min=%.1f max=%.1f"
          % (label, len(values), p50, p95, ordered[0], ordered[-1]))


def timed(fn):
    import time
    start = time.perf_counter()
    result = fn()
    return (time.perf_counter() - start) * 1000.0, result


def main():
    init = call("initialize", {"protocolVersion": "2025-03-26", "capabilities": {},
                               "clientInfo": {"name": "bench", "version": "1"}})
    print("version:", init.get("result", {}).get("serverInfo", {}).get("version"))
    call("notifications/initialized", notification=True)

    ping_ms = []
    for _ in range(10):
        ms, _ = timed(lambda: tool("ping"))
        ping_ms.append(ms)
    stats("ping (HTTP wall)", ping_ms)

    info_ms = []
    for _ in range(5):
        ms, _ = timed(lambda: tool("get_project_info"))
        info_ms.append(ms)
    stats("get_project_info (HTTP wall)", info_ms)

    run_wall = []
    run_server = []
    guards = []
    for i in range(4):
        ms, result = timed(lambda: tool("run_python_toolbox_tool", {
            "toolbox_path": RUNCODE, "tool_name": "RunCode", "args": [SCRIPT]}))
        run_wall.append(ms)
        run_server.append(result.get("elapsed_seconds"))
        guards.append({
            "guard": result.get("guard"),
            "warning": result.get("guard_warning"),
            "messages_tail": (result.get("messages") or [])[-1:],
        })
        print("RunCode %d: wall=%.0f ms server_elapsed=%.2f s"
              % (i + 1, ms, result.get("elapsed_seconds") or -1))
    stats("RunCode (HTTP wall)", run_wall)
    stats("RunCode (server elapsed ms)", [s * 1000.0 for s in run_server if isinstance(s, (int, float))])
    print("guard per call:", json.dumps(guards, ensure_ascii=False)[:600])


if __name__ == "__main__":
    main()
