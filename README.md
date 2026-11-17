[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-1.1.4%20(stable)-2ea44f?style=flat-square)](../../releases/tag/v1.1.4) [![Changelog](https://img.shields.io/badge/Docs-Changelog-6f42c1?style=flat-square)](docs/CHANGELOG.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]() [![Release](https://img.shields.io/github/v/release/XNYGggg/arcgis-pro-mcp-optimized?style=flat-square&label=release)](../../releases)

# 🗺️ ArcGIS Pro MCP (Optimized) · Customer Edition v1.1.4 · Stable (Recommended) 🎁

One natural-language sentence lets your AI assistant drive the ArcGIS Pro project you already have open: add layers, change symbology, query attributes, run geoprocessing, export maps. Your data never leaves your machine — all traffic stays on `127.0.0.1` 🔒.

> **📌 Version status (updated 2026-11-16)**: v1.1.4 (add-in 1.1.4, 113 tools) is the **stable · recommended** release. The other 2.0 line (v2.0.0 / v2.0.1) is a **testing** line: the optimization is unfinished, its GitHub Release assets and tags have been removed, and it is **not recommended** (NuGet 2.0.x downloads will fail); the 2.0 code now lives only on the `customer-v2.0.1` / `customer-v2.0.0` branches. See [BRANCHES.md](https://github.com/XNYGggg/arcgis-pro-mcp-optimized/blob/main/BRANCHES.md) for the branch / tag / release map.

> **🙏 Upstream project & original author**: This customer edition is based on **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)** (original author **Pisut Nakmuenwai**, AGPL-3.0-or-later), with local patches accumulated up to **v1.1.4**. Full attribution and modification notices are in `NOTICE` and `source/SOURCE-NOTICE.md` at the package root.

This customer edition bundles **every component** needed to run. It works offline and **requires no Python, no pip, and no network access** — no dependencies beyond ArcGIS Pro itself. Batteries included 🔋

---

## 🖥️ System requirements

| Item | Requirement |
|---|---|
| 🪟 OS | **Windows 11 x64** (64-bit Home/Pro/Enterprise, officially supported by ArcGIS Pro 3.7). **Architecture: x64 (AMD64) only — ARM64 is not supported.** |
| 🗺️ ArcGIS Pro | **3.7 or later** (this add-in is built against 3.7 / .NET 10) |
| 🔑 Privileges | A standard user is enough; installing needs **no administrator** rights |
| 📦 Other dependencies | None (PowerShell and .NET ship with Windows / Pro) |

---

## 🚀 Quick start (5 minutes)

1. 📂 **Extract** the package to any folder (e.g. `D:\ArcGIS-Pro-MCP-customer`).
2. 🛠️ **Close ArcGIS Pro**, then double-click `install\00-install.cmd`. The script installs the add-in, deploys the runtime, and tries to configure Claude Desktop / Claude Code.
3. 🖥️ **Open ArcGIS Pro**. If you see "add-in not trusted" on first load, click "Yes / Trust" (safe 😄). Find the **MCP tab** in Pro and confirm the status is **running** (port `127.0.0.1:6520`).
4. 🔌 **Connect your AI client** (see "Configuration" below), then double-click `install\01-check.cmd` for a self-check; all `[OK]` means you're good to go (๑•̀ㅂ•́)و✧

> If ArcGIS Pro is not running during the self-check, the port check shows a warning — open Pro and run it again 👌

---

## 🎛️ Configuration — connect your AI client

**Prerequisite**: ArcGIS Pro is open and the **MCP tab** shows **running**. All ports bind to localhost only:

| Port | Purpose |
|---|---|
| `6520` | MCP over HTTP — every AI client connects here |
| `6510` | Legacy TCP bridge (not used by modern clients) |
| `6511` | Optional in-Pro Python bridge (`execute_arcpy_code`) |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

Manual equivalent configuration (`~/.claude.json`):

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

To remove it, use `claude mcp remove arcgis`; to add it back, run the commands above.

### 🖥️ Claude Desktop (uses the bundled stdio bridge)

Claude Desktop cannot connect over HTTP directly — it only launches local processes. Use the pure-PowerShell bridge bundled in the package (no Python needed):

1. **Fully quit Claude Desktop** (including the tray icon).
2. Run `install\02-configure-claude.cmd`, or edit the configuration manually (back it up first).
3. The configuration file lives in the first **existing** directory of:
   `%APPDATA%\Claude`, `%APPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude` — file name `claude_desktop_config.json`.

```json
{
  "mcpServers": {
    "arcgis": {
      "command": "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
      "args": ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
               "C:\\Users\\<your-username>\\AppData\\Local\\ArcGIS Pro MCP\\stdio-bridge.ps1"]
    }
  }
}
```

4. Reopen Claude Desktop; once `arcgis` appears in the tool list, you're done.

### 🟢 Codex

Add the following to `~/.codex/config.toml` (if an old `arcgis` section exists, replace the whole section, including the old `env` sub-table):

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

Save and restart Codex.

### 🔧 Other MCP clients

Connect over Streamable HTTP to `http://127.0.0.1:6520/mcp`, or use the generic JSON above. (Antigravity uses `serverUrl` and no `type`.)

### ✅ Verify the connection

- `install\01-check.cmd` — checks the Pro version, add-in hash, runtime, ports, and client configuration.
- `netstat -ano | findstr 6520` — confirms the port is listening.
- Log: `%LOCALAPPDATA%\ArcGIS-MCP\addin.log`.

---

## 🎯 How to use it

**Basic loop**: open a project with data → say what you need → the assistant picks the tools → review the result (images come back inline) 🎨

| You can say | What actually happens |
|---|---|
| "Graduated colors by area for the counties, light red to dark green" | `set_layer_renderer` (graduated colors) |
| "Select counties with population above 500,000 and export them as a new feature class" | `select_features` → `export_features` |
| "Buffer the roads by 500 meters" | `run_geoprocessing_tool` (Buffer) |
| "Total cultivated land area per township" | `summarize_features` / `zonal_statistics` |
| "Show me a screenshot of the current map view" | `export_map_view` (returns PNG) |
| "Make an A3 portrait PDF with title, legend, scale bar, and north arrow" | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| "Export 12 layers in batch with the same symbology" | `run_batch` or `execute_arcpy_code` |

**Call tools by name**: every tool in the tables below can be invoked directly by name. `run_batch` chains multiple commands in a single round trip; when no dedicated tool exists, fall back to `execute_arcpy_code`.

**Edit rules**: edits stay pending until `save_edits` — while pending, the data is locked (delete/overwrite will fail); `discard_edits` throws pending edits away. `delete_features` / `delete_dataset` cannot be undone — ask the assistant to explain the impact before running them.

**Batch and speed**: for highly repetitive or large jobs, have the assistant write an idempotent arcpy script with RunCode and run it once; for big projects, process in chunks by extent/OID with checkpoints.

---

## 🎨 Mapping and map production (prompt guide)

This section explains "how to talk to the AI": you don't need to remember tool names — just cover the four elements below and the assistant will break it down into MCP tool calls.

### Say four things and get it right the first time

| Element | What to state | Example |
|---|---|---|
| Data | Layer name, data path, current selection | "the `county_boundaries` layer in the current map" |
| Operation | Classification field/method, buffer distance, statistics definition | "5 classes on `AREA`, natural breaks" |
| Style | Color scheme, font size, legend/scale-bar/north-arrow placement, basemap | "light red to dark green, legend in the lower right" |
| Output | Image/PDF/new feature class, resolution, save path | "A3 landscape PDF, 300 dpi, save to `D:\out\`" |

**Good prompt vs. bad prompt**
- ❌ "Make my map prettier" — no goal; the assistant can only guess.
- ✅ "Symbolize `landuse` by unique values on `DLMC`, put the legend in the lower right, then export an A3 landscape PDF to `D:\out\landuse.pdf` at 300 dpi."
- ❌ "Export a map" — no extent, layout, or output specified.
- ✅ "Using the `study_area` layer as the extent, scale 1:10000, produce an A4 portrait PNG titled 'Study Area Land Use', with legend, scale bar, and north arrow; send me a preview first for confirmation."

### Prompt cheat sheet by tool group

| Module (tool group) | What you can say | Tools the assistant uses |
|---|---|---|
| Session & project | "Run `diagnose` first to check the connection and current project; then save the project as `D:\proj\result.aprx`" | diagnose / save_project |
| Layers | "Move `roads` into the `basemap` group, set transparency 40%, show only at 1:50000 and closer; turn off the `POI` layer" | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| Data & attributes | "Read the first 20 rows of `parcels` sorted by area descending; summarize area by `DLMC`" | get_features / summarize_features |
| Selection | "Select counties with `POP` above 500,000 and export them to `D:\out\counties_big.shp`; then select parcels within `study_area` by location" | select_features / select_by_location / export_features |
| Fields & datasets | "Add a DOUBLE field `AREA_HA` to `parcels` and compute it with `!Shape_Area!/10000`; create a polygon feature class `result_2026` in EPSG:4547" | add_field / calculate_field / create_feature_class |
| Geoprocessing | "Buffer the roads by 500 meters and dissolve; clip the land-use raster with the `study_area` mask" | run_geoprocessing_tool / run_batch |
| Symbology | "5 classes by area, natural breaks, light red to dark green; or unique-value colors by `TYPE`" | set_layer_renderer |
| Map view | "Zoom to `study_area`, scale 1:10000, and capture a 1600×900 PNG for me" | set_map_view / zoom_to_layer / export_map_view |
| Layout & export | "Create an A3 landscape layout, title…, legend for only 3 layers, add scale bar and north arrow, export a 300 dpi PDF" | create_layout / add_layout_* / export_layout |
| Raster | "Read the DEM's bands, cell size, and statistics; compute a vegetation index with the NDVI formula" | get_raster_info / raster_calculator / zonal_statistics |
| Catalog & data discovery | "Search the project and folder connections for shapefiles whose name contains `2025`" | search_data / describe_dataset / list_folder |
| Utilities | "In one go: hide the basemap → zoom to the counties → export a PNG; use arcpy to prefix all layer names" | run_batch / execute_arcpy_code |

### Standard mapping prompt template (copy and use)

```text
Produce a map from the current project's <map name>:
- Extent: <study-area layer/extent>, scale 1:<...>
- Data: <layer1>, <layer2> (state fields and filters where needed)
- Symbology: <graduated/unique-value/single color by field>, palette <ramp or hex>
- Layout: <A3 landscape / A4 portrait>, title "<title>", legend <position>, scale bar <position>, north arrow <position>
- Output: <PDF / PNG>, <300> dpi, save to <absolute path>
- Process: check the layout first, send me a preview for confirmation, then export the final file.
```

### Batch mapping prompt

```text
For each map in `maps`, export a PDF with the same A3 landscape layout template:
use the map name as the title, keep visible layers in the legend, put the scale bar and north arrow in the lower right,
output to `D:\out\maps\` at 300 dpi. Export the first one for me to review, then batch the rest after confirmation.
```

### One-line fixes for common mishaps

- "The legend is covering the main map" → the assistant adjusts the `legend` position or the layout.
- "The page ratio is wrong / too much white space" → adjust the map frame vs. page ratio.
- "The colors are too loud / not suitable for a paper" → give a palette, hex colors, or a reference image.
- "Don't export yet, show me a preview" → the assistant runs `export_map_view` or a layout preview first.

**Effect**: state data, operation, style, and output all at once, and the assistant usually gets close to the final map in one round; afterwards just point out the differences ("move the legend right by 1 cm") instead of repeating everything.

---

## 🛡️ Using the dialog guard (1.x edition)

**What it solves**: the first time ArcGIS Pro runs a third-party `.pyt` / Python tool (such as RunCode.pyt), it shows a "third-party code library" trust dialog; unattended tasks get stuck on it. 1.1.4 has no MCP guard tools — instead the package ships a **standalone guard script**: once started, it polls Pro windows, clicks only this kind of "Yes(Y)" dialog, and exits automatically after the configured duration.

**How to start it**

```text
install\03-dialog-guard.cmd                    # 600 seconds by default
install\03-dialog-guard.cmd -DurationSeconds 120
install\03-dialog-guard.cmd -AlertOnly         # topmost alert only, no auto-click
install\03-dialog-guard.cmd -NotifyTest        # test the alert window only (30 s)
```

You can also double-click `03-dialog-guard.cmd`, or have an AI assistant that can control a terminal start it in the background.

**Prompt examples**

| You say | What the assistant does |
|---|---|
| "I'm going to run RunCode.pyt manually in Pro. Start the dialog guard for 600 seconds first; auto-click 'Yes' on the third-party code trust dialog, and if it can't click, show a topmost alert with a beep" | Runs `install\03-dialog-guard.cmd -DurationSeconds 600` in the background |
| "This time only alert, don't auto-click" | Starts with `-AlertOnly` |
| "Test whether the alert window pops up" | Starts with `-NotifyTest` (30 s) |
| "Where is the guard log?" | `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log` |

**Boundaries and behavior**
- It only handles trust dialogs whose context contains "third-party / code library / 第三方 / 代码库"; overwrite, delete, and other confirmation dialogs are never touched.
- When it cannot click the button, the topmost alert window stays for at least 30 seconds and beeps every 10 seconds.
- Each Pro process asks only once after trust is granted; it reappears after Pro restarts.
- 2.x upgraded this into the `guard_summon` / `guard_status` tools with automatic arm/settle before RunCode calls; in 1.1.4 you start the script manually.

---

## 🐍 RunCode and the Python bridge (prompt guide)

The arcpy channels in 1.1.4 come in two tiers: the **RunCode toolbox** suits one-off / batch scripts; the **Python bridge** suits interactive sessions that need variables to persist across calls.

### RunCode toolbox

Path: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`. Have the assistant pass this path to `run_python_toolbox_tool`; you can also add it yourself in Pro via "Insert → Toolbox → Add Toolbox".

**Prompt examples**
- "Use RunCode.pyt to turn on labels for every layer in the current project, label field `NAME`, font size 10."
- "Write an idempotent arcpy script and run it with RunCode to prefix every layer name with `LYR_`, skipping ones already prefixed."
- "This script is going to run in RunCode.pyt — check the parameters and paths for me first, then execute."

**First run** shows the "third-party code library" trust dialog: start the dialog guard above first, or click "Yes" once manually. Each Pro process needs trusting only once.

### Python bridge (when you need `CURRENT` or persistent variables)

1. Open the Python window from the Pro menu **Analysis → Python**;
2. Paste and run (path as in the default install location):

```python
import sys; sys.path.insert(0, r"C:\Users\<your-username>\AppData\Local\ArcGIS Pro MCP\plugin")
import mcp_bridge; print(mcp_bridge.start_server(port=6511))
```

3. When you need `CURRENT` (the open project), add one more line (must run on the main thread):

```python
print(mcp_bridge.start_pump())
```

**Prompt examples**
- "The next arcpy work needs `CURRENT`. Remind me to paste the bridge startup code into Pro's Python window; then I'll run it via `execute_arcpy_code`."
- "Fill in the bridge startup code with my username and copy it for me; I'll paste it into Pro myself."
- "Check with `get_pump_status` first whether the pump is ready; if not, remind me to paste the code."
- "Use a persistent session in three steps: read the layer, create the field, then compute — variables must persist across calls."

**Effect**: RunCode handles "run a batch script in one go"; the Python bridge handles "multi-step interaction needing the current project". Neither requires a separate Python installation.

---

## 🧪 About the distillation module (2.x feature)

1.1.4 **does not include** template distillation: there are no `template_*` tools and no `guard_summon` / `guard_status`. If you want to turn a sample figure's layout into a reusable template and keep multiple maps consistent, upgrade to **V2.01 (v2.0.1)**:

- 2.x provides 10 `template_*` tools: import / measure / patch / validate / preview / review / approve / apply / list / get;
- The template library lives in `%LOCALAPPDATA%\ArcGIS Pro MCP\library`, and upgrading preserves existing templates;
- Typical usage: "Distill `D:\samples\fig3.png` into a template named Study Area Overview; follow the import → measure → patch → validate → preview flow, show me the preview for confirmation, then review + approve."

Capability comparison between 1.1.4 and 2.x:

| Capability | 1.1.4 (this branch) | 2.0.1 (not recommended) |
|---|---|---|
| Native MCP tools | 113 (12 groups) | 125 (14 groups) |
| RunCode / Python bridge | ✅ | ✅ |
| Dialog guard | Standalone script `03-dialog-guard.cmd` | `guard_summon` / `guard_status` + automatic arm/settle before RunCode |
| Template distillation | ❌ | ✅ 10 `template_*` tools + schema v1.1 |

---

## 🧰 All 113 MCP tools (12 groups)

Everything the assistant can do, listed by name. Click to expand 👇

<details>
<summary><b>📜 Full tool list — session 15 · layers 19 · data 11 · selection 6 · fields/datasets 11 · geoprocessing 7 · symbology 6 · view 7 · layout 16 · raster 5 · catalog 6 · utilities 4</b></summary>

### Session & project (15)

| Tool | Purpose |
|---|---|
| `ping` | Check bridge connectivity and return the connected project. |
| `get_capabilities` | List every command the bridge supports (grouped by area), for discovering capabilities beyond the named tools. |
| `get_queue_status` | Three-lane queue status: per-lane depth, the command running now, and recent completions; check it first when calls feel slow or queued. |
| `diagnose` | Connection self-check: Pro version, license, open project, active map, map view, and write access; run it first when something is wrong. |
| `get_arcgis_info` | ArcGIS Pro version, license level, available extensions, and the current project path. |
| `get_project_info` | Project path, default geodatabase and toolbox, maps, layouts, folder and database connections. |
| `save_project` | Save the project (.aprx), or save a copy elsewhere. |
| `list_maps` | List all maps and scenes with coordinate system and layer counts. |
| `create_map` | Create a new map or scene in the project. |
| `remove_map` | Delete a map from the project. |
| `activate_map` | Open/activate a map's view in the Pro UI; required before camera commands and export_map_view. |
| `set_map_properties` | Rename a map or change its coordinate system. |
| `get_map_extent` | The combined extent of all data layers in a map. |
| `get_environment` | Read the current arcpy geoprocessing environment settings. |
| `set_environment` | Set arcpy environments (workspace, output coordinate system, extent, mask, cell size, overwrite output, parallel factor, etc.). |

### Layers (19)

| Tool | Purpose |
|---|---|
| `get_layers` | List the map's layers (with draw order and group nesting) and standalone tables; start here to see what's in the project. |
| `get_layer_info` | Full detail for one layer: data source, coordinate system, extent, fields, feature count, renderer, and label state. |
| `add_layer` | Add data to a map by path or service URL: feature class, shapefile, raster, table, .lyrx layer file, or web service. |
| `add_web_layer` | Add a web layer by URL (Feature/Map/Image service, WMS, WMTS, vector tiles). |
| `remove_layer` | Remove a layer or standalone table from a map. |
| `rename_layer` | Rename a layer in the table of contents. |
| `duplicate_layer` | Copy a layer within the map so it can be symbolized differently. |
| `set_layer_visibility` | Show or hide one layer, several layers, or every layer in the map. |
| `set_layer_transparency` | Set layer transparency (0 opaque, 100 fully transparent). |
| `set_layer_scale_range` | Limit the scale range a layer draws at. |
| `set_definition_query` | Set a layer's definition query (SQL where); an empty string clears it. |
| `move_layer` | Reorder a layer relative to another, or move it into a group layer. |
| `create_group_layer` | Create a group layer, optionally moving existing layers into it. |
| `zoom_to_layer` | Zoom the map view to a layer's extent. |
| `set_basemap` | Set the basemap: Topographic, Imagery, Imagery Hybrid, Streets, Navigation, Light Gray Canvas, Dark Gray Canvas, Terrain, Oceans, OpenStreetMap, National Geographic, and more. |
| `get_broken_layers` | List layers in all maps whose data sources are missing. |
| `repair_layer_source` | Repoint a layer at a new workspace or dataset to fix a broken source. |
| `add_join` | Join a table to a layer on a common field. |
| `remove_join` | Remove a layer's join. |

### Data & attributes (11)

| Tool | Purpose |
|---|---|
| `get_features` | Read attribute rows from a layer, table, or dataset path, with where, field subset, ordering, and WKT geometry. |
| `count_features` | Count features, optionally with a where clause. |
| `get_unique_values` | Distinct values of a field with the count of rows for each. |
| `get_field_statistics` | min/max/mean/median/sum/standard deviation of a numeric field. |
| `summarize_features` | Group by one or more fields and aggregate — the quick way to answer "how much per category" without geoprocessing. |
| `insert_features` | Insert new rows into a layer or table. |
| `update_features` | Update attributes and/or geometry of rows matching a where clause. |
| `delete_features` | Delete rows matching a where clause. |
| `save_edits` | Commit pending edits; unsaved edits lock the data, so delete or overwrite operations fail until then. |
| `discard_edits` | Throw away pending edits. |
| `calculate_field` | Calculate a field, e.g. the expression "!Shape_Area! / 10000". |

### Selection (6)

| Tool | Purpose |
|---|---|
| `select_features` | Select features by SQL where clause. |
| `select_by_location` | Select features by spatial relationship to another layer. |
| `get_selection` | Report the current selection (per layer, with ObjectIDs and optionally attribute rows). |
| `set_selection` | Select specific features by ObjectID. |
| `clear_selection` | Clear the selection on one layer, or on every layer in the map. |
| `zoom_to_selection` | Zoom the map view to the currently selected features. |

### Fields & datasets (11)

| Tool | Purpose |
|---|---|
| `list_fields` | List a layer's fields with type, alias, length, and domain. |
| `add_field` | Add a field to a layer or table. |
| `add_fields` | Add several fields in one call. |
| `delete_field` | Delete one or more fields. |
| `alter_field` | Rename a field, or change its alias/length. |
| `create_feature_class` | Create an empty feature class (in the project's default geodatabase by default) and add it to the map. |
| `create_table` | Create an empty standalone table. |
| `create_file_geodatabase` | Create a new file geodatabase. |
| `truncate_table` | Delete every row of a table or feature class, keeping the schema. |
| `delete_dataset` | Delete a dataset from disk or a geodatabase; cannot be undone. |
| `export_features` | Export a layer to a new dataset, honoring the current selection and definition query. |

### Geoprocessing (7)

| Tool | Purpose |
|---|---|
| `run_geoprocessing_tool` | Run any arcpy geoprocessing tool — the universal analysis channel; layer names resolve to live layers so selections and definition queries apply. Call describe_geoprocessing_tool first when parameter names are unknown. |
| `list_geoprocessing_tools` | Search available geoprocessing tools by name. |
| `list_toolboxes` | List arcpy toolbox modules and the project's toolboxes. |
| `describe_geoprocessing_tool` | Query a tool's parameters, data types, defaults, and usage text before running it. |
| `run_python_toolbox_tool` | Run a tool from a custom .pyt / .atbx / .tbx toolbox on disk. |
| `check_extension` | Check and optionally check out an ArcGIS extension license. |
| `get_messages` | Messages from the most recent geoprocessing operation. |

### Symbology (6)

| Tool | Purpose |
|---|---|
| `set_layer_renderer` | Change a layer's symbology: single symbol, unique values by category, or graduated/continuous color scheme by numeric field. |
| `get_layer_symbology` | Inspect a layer's current renderer, class breaks, unique values, and label settings. |
| `list_color_ramps` | List the color ramps available in the project. |
| `set_layer_labeling` | Turn labels on/off and set the expression, font, and halo. |
| `apply_symbology_from_layer` | Copy symbology from a .lyrx file or another layer. |
| `save_layer_file` | Save a layer with its symbology to a .lyrx file for reuse. |

### Map view (7)

| Tool | Purpose |
|---|---|
| `get_map_view` | Current camera position: center, scale, rotation, and visible extent; requires the map tab to be open in Pro. |
| `set_map_view` | Move the map view: set an extent, center point, scale, and/or rotation. |
| `export_map_view` | Render the map view to PNG and return the image for visual checking; requires the map tab to be open. |
| `list_bookmarks` | List the spatial bookmarks defined on a map. |
| `create_bookmark` | Save the current view, or a given extent, as a named bookmark. |
| `apply_bookmark` | Zoom the map view to a bookmark. |
| `delete_bookmark` | Delete a bookmark from a map. |

### Layout & export (16)

| Tool | Purpose |
|---|---|
| `list_layouts` | List the print layouts with page size and element counts. |
| `get_layout_info` | Inspect a layout: page setup and every element with position and size. |
| `create_layout` | Create a new layout page (by default with a map frame filling it). |
| `delete_layout` | Delete a layout from the project. |
| `add_map_frame` | Add a map frame to a layout at a page position. |
| `set_map_frame_extent` | Point a layout's map frame at a layer, an extent, or a scale. |
| `add_layout_text` | Add text elements such as a title or credits to a layout. |
| `add_layout_legend` | Add a legend tied to a layout's map frame. |
| `add_layout_scale_bar` | Add a scale bar tied to a map frame. |
| `add_layout_north_arrow` | Add a north arrow tied to a map frame. |
| `add_layout_picture` | Place an image such as a logo on a layout. |
| `set_layout_element` | Move, resize, rename, hide, or change the text of any layout element. |
| `delete_layout_element` | Remove an element from a layout. |
| `export_layout` | Export a layout to PDF / PNG / JPEG / SVG / TIFF; PNG/JPEG come back inline as images for checking. |
| `preview_layout` | Render a layout to a temporary image without writing a file — check it while building. |
| `export_map_series` | Export a layout's map series (map book) to a multi-page PDF. |

### Raster (5)

| Tool | Purpose |
|---|---|
| `get_raster_info` | Raster detail: bands, size, cell size, pixel type, statistics, and coordinate system. |
| `set_raster_symbology` | Set a raster layer's colorizer and color ramp. |
| `raster_calculator` | Map algebra: bind rasters to short aliases, then write a Python expression; requires the Spatial Analyst extension. |
| `sample_raster_values` | Read raster cell values at map coordinates or at a point layer's features. |
| `zonal_statistics` | Summarize raster values inside zone polygons and return a table; requires the Spatial Analyst extension. |

### Catalog & data discovery (6)

| Tool | Purpose |
|---|---|
| `list_workspace_contents` | List the datasets inside a geodatabase or folder; defaults to the project's default geodatabase. |
| `list_folder` | List GIS files and subfolders on disk; defaults to the project's home folder. |
| `describe_dataset` | Describe any dataset by path: type, geometry, coordinate system, extent, fields, and row count (without adding it to a map). |
| `search_data` | Find datasets by name across the project's geodatabase, home folder, and folder connections — the fastest way to locate data before adding it. |
| `get_project_items` | Folder connections, database connections, and toolboxes registered in the project. |
| `add_folder_connection` | Register a folder with the project so its data is easy to browse. |

### Utilities (4)

| Tool | Purpose |
|---|---|
| `execute_arcpy_code` | Run Python inside ArcGIS Pro: arcpy is already imported, arcpy.mp.ArcGISProject('CURRENT') is the open project, variables persist across calls, print output is captured, and the final expression's value is returned. Use it when no dedicated tool fits. |
| `get_pump_status` | Whether the main-thread dispatcher is installed; it decides whether commands can reach the open Pro project — without it only path-based work runs. |
| `stop_pump` | Remove the main-thread dispatcher; live-project commands stop working until the user installs it again. |
| `run_batch` | Run several commands in one round trip — much faster for multi-step workflows. |

</details>

---

## 📦 What's in the package

| Path | Contents | Notes |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.4 | 113 native tools + all GP tools; HTTP MCP 6520 / legacy TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio bridge | Bridges clients that can only launch local processes (such as Claude Desktop) to 6520; pure PowerShell, no Python needed |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode toolbox | For frequent execution of arbitrary arcpy; the first run shows a trust dialog once |
| `install\runtime\guard\` | 🛡️ Dialog guard | Auto-clicks the "third-party code library" trust dialog; shows a topmost alert on failure (≥30 s, with beeps) |
| `install\runtime\plugin\` | 🔌 Python bridge (optional) | Started in Pro's Python window; provides persistent `execute_arcpy_code` sessions (port 6511) |
| `install\*.cmd / *.ps1` | 🛠️ Install & self-check | Install / self-check / configure Claude / dialog guard / uninstall |
| `clients\` | 📁 Client setup docs | Claude Code, Claude Desktop, Codex, generic JSON |
| `docs\` | 📖 Usage guide, troubleshooting, work rules, changelog | Start with the usage guide |
| `source\` | 📜 Full source snapshot | Source corresponding to this package's binaries (an AGPL-3.0 compliance requirement), including the upstream LICENSE / NOTICE |

### 🔍 Dependency audit (why "fully bundled")

- 🧩 add-in: a self-contained C# assembly that runs on the .NET Desktop Runtime 10 x64 included with Pro 3.7's supported environment — no separate .NET install needed.
- 🌉 stdio bridge, 🛡️ dialog guard: PowerShell 5.1 + WinForms, included with Windows — nothing extra to install.
- 🐍 RunCode toolbox, Python bridge: run on the Python (arcpy) that ships with ArcGIS Pro — no separate Python installation.
- ✅ **So this package has no external dependency requiring pip or network access.** "Works offline" refers to the package's own install and run path; ArcGIS Pro 3.7's installation prerequisites (.NET Desktop Runtime 10 x64, WebView2 ≥132) are provided by Pro's own installer.

---

## 🧪 Using the optional components

**🐍 RunCode toolbox** (for frequent arcpy work)
- Path: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- Have the AI pass this path to `run_python_toolbox_tool`; or add it in Pro via "Insert → Toolbox → Add Toolbox".
- The first run in each Pro session shows the "third-party code library" trust dialog: run `03-dialog-guard.cmd` first and the guard clicks "Yes" for you ✨.

**🔌 Python bridge** (when an arcpy session must persist across calls)
1. Open the Python window from the ArcGIS Pro menu **Analysis → Python**;
2. Paste and run (the path is already written for the default install location):
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<your-username>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. When you need `CURRENT` (the open project), add one more line (must run on the main thread):
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ Dialog guard** (recommended before running RunCode / ENVI→Pro joint workflows)
```
03-dialog-guard.cmd                 # 600-second guard
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # test the alert window only (30 s)
03-dialog-guard.cmd -AlertOnly      # alert only, no auto-click
```
Safety boundary: it only handles trust dialogs whose context contains "third-party / code library / 第三方 / 代码库"; it **never touches overwrite, delete, or other confirmation dialogs** 🙅. The log is at `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`.

---

## 🔒 Security notes

- 🏠 The add-in and all bridge services listen on `127.0.0.1` only; other machines on the LAN cannot connect.
- ⚖️ This software provides local services on `127.0.0.1` only and does not serve third parties over a network; if you modify it to serve others over a network, the AGPL-3.0 §13 source-offer obligation applies.
- 🔏 The add-in is an unsigned build: ArcGIS Pro asks you to confirm trust on first load — this is the normal flow; verify `SHA256SUMS.txt` in this package.
- 🕵️ This package contains no telemetry and no network reporting.
- 🧹 Uninstall: `99-uninstall.cmd` (optionally keeping client configuration).

---

## 📜 Version & license

- 🏷️ Version: **1.1.4** (build date in `VERSION.json`; 113 tools)
- 👤 Maintainer: **Law** ([@XNYGggg](https://github.com/XNYGggg))
- 🙏 Upstream project & original author: **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)** by **Pisut Nakmuenwai** (AGPL-3.0-or-later). This package accumulates patches on top of it up to 1.1.4 (changes are listed in `docs\CHANGELOG.md` and `RELEASE-NOTES.md`).
- ⚖️ License: **AGPL-3.0-or-later** (the package includes a full source snapshot and the upstream NOTICE)
- 🏷️ Trademark: "ArcGIS" is a trademark of Esri; this project is a third-party tool and is not affiliated with Esri.

🆘 FAQ in `docs\排障手册.md`; advanced usage in `docs\使用指南.md`. Happy mapping 🎨 (｀・ω・´)ゞ

---

<!-- LDU-THANKS -->

<h2 align="center">🙏 Acknowledgements</h2>

<p align="center">
  This project is supported by <strong>Ludong University</strong>. We sincerely thank the university for its support.
</p>

<p align="center">
  <a href="https://www.ldu.edu.cn/"><img src="assets/ldu-emblem.png" width="104" alt="Ludong University emblem"></a>
</p>

<h3 align="center">Ludong University</h3>

> Founded in 1930 and located in Yantai, Shandong, Ludong University is a provincial comprehensive university that focuses on applied talent cultivation and pursues coordinated development across engineering, agriculture, science and the humanities. The university has 26 colleges, and six disciplines — including Engineering, Chemistry and Materials Science — rank among the global top 1% in ESI. Its motto is “Great Virtue, Broad Learning, Daily Renewal, Earnest Practice” (厚德、博学、日新、笃行).

<p align="center">
  📍 No. 186 Hongqi Middle Road, Zhifu District, Yantai, Shandong 264025, China<br>
  🌐 <a href="https://www.ldu.edu.cn/">Official Website</a> ·
  <a href="https://jwc.ldu.edu.cn/">Academic Affairs Office</a> ·
  <a href="https://lib.ldu.edu.cn/">Library</a> ·
  <a href="https://ehall.ldu.edu.cn/">Student Portal</a>
</p>
