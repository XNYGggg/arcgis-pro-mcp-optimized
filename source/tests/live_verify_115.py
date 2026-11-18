"""Live verification for the local 1.1.5 upgrade (F1-F9), run against the
running add-in over MCP HTTP. Writes a JSON report to stdout.

    python tests/live_verify_115.py [--url http://127.0.0.1:6520/mcp]

Safe to re-run: scratch maps and datasets carry the v115_ prefix and are
removed at the end; the 实验12 map's symbology is restored.
"""
import argparse
import json
import urllib.request

MAP = "实验12"
GDB = (r"E:\ArcGIS_Home\project_total\MyProject_04_FirstHomework"
       r"\project\MyProject\MyProject.gdb")
SNAP = r"E:\ArcGIS_Home\数据库\数据库_02\02.gdb\土地利用栅格"
DEM = GDB + r"\DEM_实验12"
SLOPE_CLASS = "SlopeClass_实验12"
STAT_TABLE = GDB + r"\土地利用平均坡度统计_实验12"
SLOPE_PROBE = GDB + r"\verify_slope_env"

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


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--url", default="http://127.0.0.1:6520/mcp")
    args = parser.parse_args()
    mcp = Mcp(args.url)

    # -- session ---------------------------------------------------------
    init = mcp.call("initialize", {"protocolVersion": "2025-03-26",
                                  "capabilities": {},
                                  "clientInfo": {"name": "live-verify-115", "version": "1"}})
    version = init.get("serverInfo", {}).get("version")
    record("initialize reports 1.1.5.x", str(version).startswith("1.1.5"), version)
    mcp.call("notifications/initialized", notification=True)

    tools = [t["name"] for t in mcp.call("tools/list")["tools"]]
    record("tools/list has 115 tools", len(tools) == 115, len(tools))
    record("tools/list includes add_table", "add_table" in tools)

    caps = mcp.tool("get_capabilities", {})
    commands = caps.get("commands") if isinstance(caps, dict) else None
    names = caps.get("groups") if isinstance(caps, dict) else None
    flat = json.dumps(caps, ensure_ascii=False)
    record("get_capabilities includes add_table", "add_table" in flat,
           (len(commands) if isinstance(commands, list) else "see detail"))

    # -- F1: environment forwarding --------------------------------------
    applied = mcp.tool("set_environment", {"settings": {
        "cellSize": 20, "snapRaster": SNAP, "bogusKey": "x"}})
    record("set_environment forwards snapRaster+cellSize",
           "snapRaster" in applied.get("forwarded", [])
           and "cellSize" in applied.get("forwarded", []), applied.get("forwarded"))
    record("set_environment reports unknown key as unsupported",
           "bogusKey" in applied.get("unsupported", []), applied.get("unsupported"))

    env = mcp.tool("get_environment", {})
    record("get_environment lists forwarded keys",
           "snapRaster" in env.get("_forwarded", []), env.get("_forwarded"))

    # Slope at cellSize=20 snapped to the land-use grid: alignment probe.
    mcp.tool("run_geoprocessing_tool", {
        "tool_name": "sa.Slope",
        "parameters": {"in_raster": DEM, "out_raster": SLOPE_PROBE,
                       "output_measurement": "DEGREE", "z_factor": 1},
        "add_to_map": False})
    info = mcp.tool("get_raster_info", {"layer_name": SLOPE_PROBE})
    cell = info.get("cell_size_x")
    extent = info.get("extent", {})
    snap = mcp.tool("get_raster_info", {"layer_name": SNAP})
    sx, sy = snap["extent"]["xmin"], snap["extent"]["ymin"]
    # The output grid lines up with the snap raster's own grid, which is a
    # 10 m grid -- not with a 20 m grid, even when the cell size is 20.
    snap_cell = snap.get("cell_size_x") or 20
    dx = (extent.get("xmin", 0) - sx) % snap_cell
    dy = (extent.get("ymin", 0) - sy) % snap_cell
    record("F1 environment actually reaches GP (20 m, snapped)",
           abs(cell - 20) < 1e-6 and (min(dx, snap_cell - dx) < 1e-3)
           and (min(dy, snap_cell - dy) < 1e-3),
           {"cell": cell, "snap_cell": snap_cell, "offset": [dx, dy]})
    mcp.tool("run_geoprocessing_tool", {
        "tool_name": "management.Delete",
        "parameters": {"in_data": SLOPE_PROBE}})

    # Per-call override back to 10 m without touching the stored setting.
    mcp.tool("run_geoprocessing_tool", {
        "tool_name": "sa.Slope",
        "parameters": {"in_raster": DEM, "out_raster": SLOPE_PROBE,
                       "output_measurement": "DEGREE", "z_factor": 1},
        "environment": {"cellSize": 10},
        "add_to_map": False})
    info2 = mcp.tool("get_raster_info", {"layer_name": SLOPE_PROBE})
    record("F1 per-call environment override (10 m)",
           abs(info2.get("cell_size_x", 0) - 10) < 1e-6, info2.get("cell_size_x"))
    mcp.tool("run_geoprocessing_tool", {
        "tool_name": "management.Delete",
        "parameters": {"in_data": SLOPE_PROBE}})

    # -- F2: describe carries types/enums/defaults -----------------------
    desc = mcp.tool("describe_geoprocessing_tool", {"tool_name": "3d.TinRaster"})
    params = {p["name"]: p for p in desc.get("parameters", [])}
    sd = params.get("sample_distance", {})
    record("F2 describe exposes TinRaster sample_distance enum",
           len(sd.get("filter_list", [])) == 2, sd.get("filter_list"))
    record("F2 describe exposes defaults",
           params.get("sample_value", {}).get("default") == "250",
           params.get("sample_value", {}).get("default"))

    # -- F3/F8: create_map basemap / alias / conflict --------------------
    created = mcp.tool("create_map", {"name": "v115_scratch"})
    record("F3 create_map defaults to no basemap", created.get("basemap") == "none",
           created)
    layers = mcp.tool("get_layers", {"map_name": "v115_scratch",
                                     "include_basemap": True})
    record("F3 scratch map has no basemap layer",
           not any(l.get("is_basemap") for l in layers.get("layers", [])),
           [l["name"] for l in layers.get("layers", [])])

    alias = mcp.tool("create_map", {"map_name": "v115_alias"})
    record("F8 create_map accepts map_name alias", alias.get("created") == "v115_alias",
           alias)
    try:
        mcp.tool("create_map", {"name": "v115_a", "map_name": "v115_b"})
        record("F8 conflicting aliases rejected", False)
    except RuntimeError as exc:
        record("F8 conflicting aliases rejected", "differ" in str(exc), str(exc)[:120])

    mcp.tool("create_map", {"name": "v115_epsg", "epsg": 2360})
    epsg_info = mcp.tool("set_map_properties", {"map_name": "v115_epsg", "epsg": 3857})
    record("F9 set_map_properties applies epsg",
           epsg_info.get("changed", {}).get("epsg") == 3857,
           epsg_info.get("changed"))
    record("F9 reports map_note (no reprojection)",
           "not reprojected" in (epsg_info.get("map_note") or ""))

    # -- F4: raster/TIN zoom and symbology -------------------------------
    mcp.tool("activate_map", {"map_name": MAP})
    zoom = mcp.tool("zoom_to_layer", {"layer_name": "DEM_实验12", "map_name": MAP})
    record("F4 zoom_to_layer works on a raster layer",
           not zoom.get("fallback_to_view") and zoom.get("extent") is not None,
           zoom.get("extent"))
    sym = mcp.tool("get_layer_symbology", {"layer_name": "DEM_实验12",
                                          "map_name": MAP})
    record("F4 get_layer_symbology works on a raster layer",
           sym.get("renderer") is not None, sym.get("renderer"))

    # -- F6: export fit auto ---------------------------------------------
    png = r"E:\ArcGIS_Home\project_total\MyProject_04_FirstHomework\02\photo\v115_fit_auto.png"
    exp = mcp.tool("export_map_view", {
        "map_name": MAP, "output_path": png, "width": 600, "height": 400,
        "fit": "auto", "zoom_to_layer": "DEM_实验12", "return_image": False})
    record("F6 fit:auto adjusts the output size", exp.get("fit_applied") is True,
           exp.get("actual_size"))
    exp2 = mcp.tool("export_map_view", {
        "map_name": MAP, "output_path": png, "width": 600, "height": 400,
        "return_image": False})
    record("F6 default fit:preserve keeps the old exact size",
           exp2.get("actual_size") == [600, 400] and exp2.get("fit_applied") is False,
           exp2.get("actual_size"))

    # -- F7: unique-value colours ---------------------------------------
    recolor = mcp.tool("set_raster_symbology", {
        "layer_name": SLOPE_CLASS, "colorizer": "unique_values",
        "value_colors": {"1": [10, 200, 10, 100], "9": [0, 0, 0, 100]}})
    record("F7 value_colors applied and unmatched reported",
           recolor.get("value_colors_applied") == ["1"]
           and "9" in recolor.get("unmatched_values", []), recolor)
    readback = mcp.tool("get_layer_symbology", {"layer_name": SLOPE_CLASS,
                                               "map_name": MAP})
    first = (readback.get("unique_values") or [{}])[0]
    record("F7 symbology read-back shows the explicit colour",
           first.get("color") == [10, 200, 10, 100], first.get("color"))
    # Restore the experiment-12 palette.
    mcp.tool("set_raster_symbology", {
        "layer_name": SLOPE_CLASS, "colorizer": "unique_values",
        "value_colors": {"1": [142, 209, 107, 100], "2": [255, 235, 92, 100],
                         "3": [244, 154, 62, 100], "4": [205, 45, 45, 100]}})

    # -- F5: add_table ---------------------------------------------------
    added = mcp.tool("add_table", {"map_name": MAP, "path": STAT_TABLE})
    record("F5 add_table adds the statistics table",
           added.get("added") is not None, added)
    try:
        mcp.tool("add_table", {"map_name": MAP, "path": STAT_TABLE})
        record("F5 duplicate add_table rejected", False)
    except RuntimeError as exc:
        record("F5 duplicate add_table rejected", "already has" in str(exc),
               str(exc)[:120])
    removed = mcp.tool("remove_layer", {"map_name": MAP,
                                        "layer_name": "土地利用平均坡度统计_实验12"})
    record("F5 standalone table removed again", removed.get("count", 0) >= 1, removed)

    # -- cleanup ---------------------------------------------------------
    for scratch in ("v115_scratch", "v115_alias", "v115_epsg"):
        try:
            mcp.tool("remove_map", {"map_name": scratch})
        except RuntimeError:
            pass
    mcp.tool("set_environment", {"settings": {
        "cellSize": None, "snapRaster": None, "bogusKey": None}})

    failures = [r for r in RESULTS if not r["ok"]]
    print(json.dumps({"total": len(RESULTS), "failed": len(failures),
                      "results": RESULTS}, ensure_ascii=False, indent=1))
    return 1 if failures else 0


if __name__ == "__main__":
    raise SystemExit(main())
