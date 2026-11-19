"""Bridge-up state check for v1.1.6, without the real Python bridge.

Starts a protocol-accurate stub on 127.0.0.1:6511 for the duration of the
run, then verifies the add-in forwards correctly while it is listening and
falls back to the structured down answer after it stops.
"""
import json
import socket
import threading
import time
import urllib.request

URL = "http://127.0.0.1:6520/mcp"


def stub_bridge(seconds=25):
    srv = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    srv.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    srv.bind(("127.0.0.1", 6511))
    srv.listen(5)
    srv.settimeout(1)
    end = time.time() + seconds
    while time.time() < end:
        try:
            conn, _ = srv.accept()
        except socket.timeout:
            continue
        try:
            data = b""
            while not data.endswith(b"\n"):
                chunk = conn.recv(4096)
                if not chunk:
                    break
                data += chunk
            request = json.loads(data.decode("utf-8"))
            command = request.get("command")
            if command == "get_pump_status":
                data_out = {"running": True, "mode": "stub", "queued_jobs": 0,
                            "hint": "stub bridge"}
            elif command == "execute_arcpy_code":
                data_out = {"output": "live-116", "result": None}
            else:
                data_out = {"echo": command}
            conn.sendall((json.dumps({"success": True, "data": data_out})
                          + "\n").encode("utf-8"))
        except Exception:
            pass
        finally:
            conn.close()
    srv.close()


class Mcp:
    def __init__(self):
        self.sequence = 0

    def call(self, method, params=None, notification=False):
        self.sequence += 1
        payload = {"jsonrpc": "2.0", "method": method, "params": params or {}}
        if not notification:
            payload["id"] = self.sequence
        request = urllib.request.Request(
            URL, json.dumps(payload).encode(),
            {"Content-Type": "application/json",
             "Accept": "application/json, text/event-stream",
             "MCP-Protocol-Version": "2025-03-26"})
        with urllib.request.urlopen(request, timeout=60) as response:
            if notification:
                return None
            body = json.load(response)
        if "error" in body:
            raise RuntimeError(body["error"])
        return body["result"]

    def tool(self, name, arguments):
        result = self.call("tools/call", {"name": name, "arguments": arguments})
        texts = [item.get("text", "") for item in result.get("content", [])
                 if item.get("type") == "text"]
        text = "\n".join(texts)
        if result.get("isError"):
            raise RuntimeError(text)
        try:
            return json.loads(text)
        except json.JSONDecodeError:
            return text


def main():
    mcp = Mcp()
    init = mcp.call("initialize", {"protocolVersion": "2025-03-26",
                                  "capabilities": {},
                                  "clientInfo": {"name": "stub-check", "version": "1"}})
    print("version:", init.get("serverInfo", {}).get("version"))
    mcp.call("notifications/initialized", notification=True)

    failures = []

    def check(name, ok, detail=""):
        print(("PASS " if ok else "FAIL ") + name + ((" -- " + str(detail)[:200]) if detail else ""))
        if not ok:
            failures.append(name)

    server = threading.Thread(target=stub_bridge, daemon=True)
    server.start()
    time.sleep(0.5)
    # The 5 s refusal cache may still say "down"; one sleep clears it.
    time.sleep(5.5)

    status = mcp.tool("get_pump_status", {})
    check("bridge-up: get_pump_status forwards and reports running",
          status.get("running") is True and status.get("mode") == "stub"
          and "bridge_listening" not in status, status)

    diag = mcp.tool("diagnose", {})
    pf = next((c.get("result") for c in diag.get("checks", [])
               if c.get("check") == "python_fallback"), {})
    check("bridge-up: diagnose reports bridge_listening=true",
          pf.get("bridge_listening") is True, pf.get("probe_ms"))

    code = mcp.tool("execute_arcpy_code", {"code": "print('live-116')"})
    check("bridge-up: execute_arcpy_code forwards", "live-116" in json.dumps(code, ensure_ascii=False), code)

    server.join(timeout=30)
    time.sleep(0.5)
    status = mcp.tool("get_pump_status", {})
    check("bridge-down again: structured down answer",
          status.get("bridge_listening") is False and status.get("running") is False
          and status.get("code") == "BRIDGE_NOT_LISTENING", status)

    print(json.dumps({"failed": failures}, ensure_ascii=False))
    return 1 if failures else 0


if __name__ == "__main__":
    raise SystemExit(main())
