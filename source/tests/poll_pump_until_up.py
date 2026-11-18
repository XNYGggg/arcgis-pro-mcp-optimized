"""Poll get_pump_status over MCP until the pump is up (or timeout)."""
import json
import sys
import time
import urllib.request

URL = "http://127.0.0.1:6520/mcp"
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
    with urllib.request.urlopen(request, timeout=30) as response:
        if notification:
            return None
        return json.load(response)


def tool(name):
    body = call("tools/call", {"name": name, "arguments": {}})
    result = body.get("result", {})
    texts = [item.get("text", "") for item in result.get("content", [])
             if item.get("type") == "text"]
    if result.get("isError"):
        raise RuntimeError("\n".join(texts))
    return json.loads("\n".join(texts))


call("initialize", {"protocolVersion": "2025-03-26", "capabilities": {},
                    "clientInfo": {"name": "pump-poll", "version": "1"}})
call("notifications/initialized", notification=True)

deadline = time.time() + 600
while time.time() < deadline:
    try:
        status = tool("get_pump_status")
        if status.get("running") is True:
            print("PUMP-UP " + json.dumps(status, ensure_ascii=False))
            sys.exit(0)
    except Exception as exc:
        print("poll error: " + str(exc)[:160])
    time.sleep(5)

print("PUMP-TIMEOUT")
sys.exit(1)
