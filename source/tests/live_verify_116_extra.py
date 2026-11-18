"""v1.1.6-specific live checks: status query without the bridge, error-path
teaching, and schema text. Run against the installed add-in over MCP HTTP.

    python tests/live_verify_116_extra.py [--url http://127.0.0.1:6520/mcp]

Adaptive by design: some checks differ depending on whether the Python bridge
is currently listening, and both states are legitimate.
"""
import argparse
import json
import urllib.request

RESULTS = []


def record(name, ok, detail=""):
    RESULTS.append({"check": name, "ok": ok, "detail": detail})
    print(("PASS " if ok else "FAIL ") + name + ((" -- " + str(detail)[:300]) if detail else ""))


class Mcp:
    def __init__(self, url):
        self.url = url
        self.sequence = 0

    def call(self, method, params=None, notification=False):
        self.sequence += 1
        payload = {"jsonrpc": "2.0", "method": method, "params": params or {}}
        if not notification:
            payload["id"] = self.sequence
        request = urllib.request.Request(
            self.url, json.dumps(payload).encode(),
            {"Content-Type": "application/json",
             "Accept": "application/json, text/event-stream",
             "MCP-Protocol-Version": "2025-03-26"})
        with urllib.request.urlopen(request, timeout=180) as response:
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


RUNCODE = (r"E:\ai工具\claude code_app\project_total\01_project_arcgispro"
           r"\.claude\arcgis_mcp_runtime\RunCode.pyt")
TYPO_RUNCODE = (r"E:\ai工具\claude code app\project_total\01_project_arcgispro"
                 r"\.claude\arcgis_mcp_runtime\RunCode.pyt")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--url", default="http://127.0.0.1:6520/mcp")
    args = parser.parse_args()
    mcp = Mcp(args.url)

    init = mcp.call("initialize", {"protocolVersion": "2025-03-26",
                                  "capabilities": {},
                                  "clientInfo": {"name": "live-verify-116-extra", "version": "1"}})
    version = init.get("serverInfo", {}).get("version")
    record("initialize reports 1.1.6.x", str(version).startswith("1.1.6"), version)
    mcp.call("notifications/initialized", notification=True)

    # -- V1: the status query answers in both bridge states ---------------
    try:
        status = mcp.tool("get_pump_status", {})
        down_shape = status.get("bridge_listening") is False and status.get("running") is False
        up_shape = isinstance(status.get("running"), bool) and "bridge_listening" not in status
        record("V1 get_pump_status answers in both bridge states", down_shape or up_shape, status)
        record("V1 down answer carries code+hint",
               (not down_shape) or (status.get("code") == "BRIDGE_NOT_LISTENING"
                                    and bool(status.get("hint"))),
               {"shape": "down" if down_shape else "up", "code": status.get("code")})
    except RuntimeError as exc:
        record("V1 get_pump_status answers in both bridge states", False, str(exc)[:160])
        record("V1 down answer carries code+hint", False)

    # -- V2: diagnose probes the bridge port ------------------------------
    diag = mcp.tool("diagnose", {})
    pf = next((c.get("result") for c in diag.get("checks", [])
               if c.get("check") == "python_fallback"), {})
    record("V2 diagnose reports bridge_listening", isinstance(pf.get("bridge_listening"), bool), pf)
    probe_ms = pf.get("probe_ms")
    record("V2 probe stays within budget",
           isinstance(probe_ms, (int, float)) and probe_ms <= 3000, probe_ms)

    # -- V3: a path typo gets a suggestion --------------------------------
    try:
        mcp.tool("run_python_toolbox_tool",
                 {"toolbox_path": TYPO_RUNCODE, "tool_name": "RunCode", "args": []})
        record("V3 typo path rejected with suggestion", False, "no error raised")
    except RuntimeError as exc:
        text = str(exc)
        record("V3 typo path rejected with suggestion",
               "No toolbox" in text and "Did you mean" in text and "code_app" in text,
               text[:220])

    # -- V3b: named parameters get a ready-to-use example -----------------
    try:
        mcp.tool("run_python_toolbox_tool",
                 {"toolbox_path": RUNCODE, "tool_name": "RunCode",
                  "parameters": {"code_file": "x.py"}})
        record("V3b named parameters get an example", False, "no error raised")
    except RuntimeError as exc:
        text = str(exc)
        record("V3b named parameters get an example",
               "positional values" in text and "Example:" in text and "args=[" in text,
               text[:220])

    # -- V4: schema text states the host difference -----------------------
    tools = {t["name"]: t for t in mcp.call("tools/list")["tools"]}
    toolbox_desc = json.dumps(tools.get("run_python_toolbox_tool", {}), ensure_ascii=False)
    record("V4 run_python_toolbox_tool schema says native takes positional values",
           "positional" in toolbox_desc and "bridge" in toolbox_desc, toolbox_desc[:200])
    pump_desc = tools.get("get_pump_status", {}).get("description", "")
    record("V4 get_pump_status description names the bridge dependency",
           "bridge" in pump_desc.lower(), pump_desc[:200])

    # -- V5: execution commands still fail hard without the bridge --------
    if pf.get("bridge_listening") is False:
        try:
            mcp.tool("execute_arcpy_code", {"code": "print(1)"})
            record("V5 execution command fails hard without bridge", False, "no error raised")
        except RuntimeError as exc:
            record("V5 execution command fails hard without bridge",
                   "BRIDGE_NOT_LISTENING" in str(exc), str(exc)[:200])
    else:
        run = mcp.tool("execute_arcpy_code", {"code": "print('live-116')"})
        record("V5 execution command works with bridge up",
               "live-116" in json.dumps(run, ensure_ascii=False), run)

    failures = [r for r in RESULTS if not r["ok"]]
    print(json.dumps({"total": len(RESULTS), "failed": len(failures),
                      "results": RESULTS}, ensure_ascii=False, indent=1))
    return 1 if failures else 0


if __name__ == "__main__":
    raise SystemExit(main())
