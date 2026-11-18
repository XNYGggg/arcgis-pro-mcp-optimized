[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-Customer%201.16%20(1.14%20Enhanced)-2ea44f?style=flat-square)](VERSION.json) [![Changelog](https://img.shields.io/badge/Docs-Changelog-6f42c1?style=flat-square)](docs/CHANGELOG.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP (Optimized) · Customer 1.16 (1.14 Enhanced Edition) 🚀

One natural-language sentence lets your AI assistant drive the ArcGIS Pro project you already have open: add layers, change symbology, query attributes, run geoprocessing, export maps. Your data never leaves your machine — all traffic stays on `127.0.0.1` 🔒.

> **🚀 This branch is Customer 1.16 (1.14 Enhanced Edition, build 1.1.6)**: it stacks the 1.1.5 fixes (F1–F10) and the 1.1.6 enhancements (V-1–V-7) on top of 1.14 (v1.1.4). **The official 1.1.4 release is still in preparation — stay tuned**; this branch **ships no Release assets** — the installer lives in `install/`, and you can roll back to v1.1.4 at any time.

> **📌 Version note (2026-11-16)**: **the 1.0 line (the official v1.1.4 package) is very stable and recommended for daily work**; **2.0 is a testing line** — its optimisation is unfinished, and its GitHub Release assets and tags have been removed, so it is not downloadable for now.

> **🙏 Upstream project and original author**: this Customer Edition is based on **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)** (original author **Pisut Nakmuenwai**, AGPL-3.0-or-later), with local patches accumulated up to **v1.1.4**. Full attribution and modification notices are in `NOTICE` and `source/SOURCE-NOTICE.md`.

This package bundles **every component** needed to run — fully offline, **no Python, no pip, no internet required**, and no dependency other than ArcGIS Pro itself. Batteries included 🔋

---

## 🖥️ System requirements

| Item | Requirement |
|---|---|
| 🪟 OS | **Windows 11 x64** (64-bit Home / Pro / Enterprise; ArcGIS Pro 3.7 officially supported). **Architecture: x64 (AMD64) only — ARM64 is not supported** |
| 🗺️ ArcGIS Pro | **3.7 or later** (this add-in is built against 3.7 / .NET 10) |
| 🔑 Privileges | A normal user is enough; installation requires **no administrator** |
| 📦 Other dependencies | None (PowerShell and .NET ship with Windows / Pro) |

---

## 🚀 Five-minute quick start

1. 📂 **Unzip** this package anywhere (e.g. `D:\ArcGIS-Pro-MCP-Customer`).
2. 🛠️ **Close ArcGIS Pro**, then double-click `install\00-install.cmd`. The script installs the add-in, deploys the runtime and tries to configure Claude Desktop / Claude Code.
3. 🖥️ **Open ArcGIS Pro**. If the first load asks whether to trust the add-in, click **Yes** (safe to do 😄). Find the **MCP tab** at the top of Pro and confirm the status says **running** (port `127.0.0.1:6520`).
4. 🔌 **Connect your AI client** (see “Configuration” below), then double-click `install\01-check.cmd`; when every check reports `[OK]`, you are ready (๑•̀ㅂ•́)و✧

> If Pro is not running during the check, the port test shows a warning — open Pro and run it again 👌

---

## 🎛️ Configuration — connect your AI client

**Prerequisite**: ArcGIS Pro is open and the **MCP tab** shows **running**. Every port binds to this machine only:

| Port | Purpose |
|---|---|
| `6520` | MCP over HTTP — every AI client connects here |
| `6510` | Legacy TCP bridge (modern clients do not use it) |
| `6511` | Optional in-Pro Python bridge (`execute_arcpy_code`) |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

Equivalent manual configuration (`~/.claude.json`):

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

Remove with `claude mcp remove arcgis`; add it back with the command above.

### 🖥️ Claude Desktop (with the bundled stdio bridge)

Claude Desktop cannot speak HTTP directly and can only launch a local process — use the pure-PowerShell bridge included in the package (no Python needed):

1. **Fully quit Claude Desktop** (including the tray icon).
2. Run `install\02-configure-claude.cmd`, or edit the configuration by hand (back it up first).
3. The configuration file is `claude_desktop_config.json` in the first of these that exists:
   `%APPDATA%\Claude`, `%APPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude-3p`, `%LOCALAPPDATA%\Claude`.

```json
{
  "mcpServers": {
    "arcgis": {
      "command": "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
      "args": ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
               "C:\\Users\\<your-user-name>\\AppData\\Local\\ArcGIS Pro MCP\\stdio-bridge.ps1"]
    }
  }
}
```

4. Reopen Claude Desktop; an `arcgis` entry in the tool list means success.

### 🟢 Codex

Add this to `~/.codex/config.toml` (if an old `arcgis` section exists, replace it wholesale, including its old `env` subtable):

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

Save and restart Codex.

### 🔧 Other MCP clients

Connect over Streamable HTTP to `http://127.0.0.1:6520/mcp`, or use the generic JSON above. (Antigravity uses `serverUrl` and no `type`.)

### ✅ Verify the connection

- `install\01-check.cmd` — checks the Pro version, add-in hash, runtime, ports and client configuration.
- `netstat -ano | findstr 6520` — confirms the port is listening.
- Log: `%LOCALAPPDATA%\ArcGIS-MCP\addin.log`.

---

## 🎯 How to use it

**The basic loop**: open a project with data → say what you want → the assistant picks the tools → inspect the result (images come back inline) 🎨

| You can say | What actually happens |
|---|---|
| “Classify counties by area with a light-red to dark-green colour ramp” | `set_layer_renderer` (graduated colours) |
| “Select counties with a population over 500,000 and export them as a new feature class” | `select_features` → `export_features` |
| “Buffer the roads by 500 metres” | `run_geoprocessing_tool` (Buffer) |
| “Total the cultivated-land area per township” | `summarize_features` / `zonal_statistics` |
| “Show me the current map view” | `export_map_view` (returns a PNG) |
| “Make an A3 portrait PDF with title, legend, scale bar and north arrow” | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| “Export 12 layers in one batch with the same symbology” | `run_batch` or `execute_arcpy_code` |

**Calling by tool name**: every tool in the table below can be called by name; `run_batch` chains several commands in one round trip; when no dedicated tool fits, `execute_arcpy_code` is the fallback.

**Editing rules**: edits stay pending until `save_edits`, and pending edits lock the data (delete/overwrite will fail); `discard_edits` drops them. `delete_features` / `delete_dataset` cannot be undone — have the assistant explain the impact first.

**Batching and speed**: for highly repetitive or large work, have the assistant run one idempotent arcpy script through RunCode; for big projects, process in chunks by extent/OID with checkpoints.

---

## 🧰 All 115 MCP tools (12 groups)

Everything the assistant can do, listed by name. Click to expand 👇

<details>
<summary><b>📜 Full tool catalogue — Session 15 · Layers 19 · Data 11 · Selection 6 · Fields/Datasets 11 · Geoprocessing 7 · Symbology 6 · View 7 · Layout 16 · Raster 5 · Catalog 6 · Utilities 4</b></summary>

### Session and project (15)

| Tool | Purpose |
|---|---|
| `ping` | Check that the bridge is reachable and return the project it is attached to. |
| `get_capabilities` | List every command the bridge supports (grouped by area) to discover functionality beyond the named tools. |
| `get_queue_status` | Three-lane queue status: per-lane depth, the command running now and recent completions; check it when calls feel slow or queued. |
| `diagnose` | Connection self-check: Pro version, licence, open project, active map, map view and write access; run it first when something is wrong. |
| `get_arcgis_info` | ArcGIS Pro version, licence level, available extensions and the current project path. |
| `get_project_info` | Project paths, default geodatabase and toolbox, maps, layouts, folder and database connections. |
| `save_project` | Save the project (.aprx), or save a copy elsewhere. |
| `list_maps` | List all maps and scenes with coordinate system and layer counts. |
| `create_map` | Create a new map or scene in the project. |
| `remove_map` | Delete a map from the project. |
| `activate_map` | Open/activate a map view in the Pro UI; required before camera commands and export_map_view. |
| `set_map_properties` | Rename a map or change its coordinate system. |
| `get_map_extent` | Combined extent of every data layer in a map. |
| `get_environment` | Read the current arcpy geoprocessing environments. |
| `set_environment` | Set arcpy environments (workspace, output coordinate system, extent, mask, cell size, overwrite output, parallel factor, …). |

### Layers (19)

| Tool | Purpose |
|---|---|
| `get_layers` | List layers (with draw order and group nesting) and standalone tables in a map; start here to see what a project holds. |
| `get_layer_info` | Full detail for one layer: source, coordinate system, extent, fields, feature count, renderer and label state. |
| `add_layer` | Add data to a map from a path or service URL: feature class, shapefile, raster, table, .lyrx layer file or web service. |
| `add_web_layer` | Add a web layer by URL (Feature/Map/Image service, WMS, WMTS, vector tiles). |
| `remove_layer` | Remove a layer or standalone table from a map. |
| `rename_layer` | Rename a layer in the table of contents. |
| `duplicate_layer` | Copy a layer within the map so it can be symbolised differently. |
| `set_layer_visibility` | Show or hide one layer, several layers or every layer in the map. |
| `set_layer_transparency` | Set layer transparency (0 opaque, 100 invisible). |
| `set_layer_scale_range` | Limit the scales a layer draws at. |
| `set_definition_query` | Set a layer definition query (SQL where); an empty string clears it. |
| `move_layer` | Reorder a layer, or move it into a group layer. |
| `create_group_layer` | Create a group layer, optionally moving existing layers into it. |
| `zoom_to_layer` | Zoom the map view to a layer's extent. |
| `set_basemap` | Set the basemap: Topographic, Imagery, Imagery Hybrid, Streets, Navigation, Light Gray Canvas, Dark Gray Canvas, Terrain, Oceans, OpenStreetMap, National Geographic, … |
| `get_broken_layers` | List layers across all maps whose data source is missing. |
| `repair_layer_source` | Repoint a layer at a new workspace or dataset to fix a broken source. |
| `add_join` | Join a table to a layer on a common field. |
| `remove_join` | Remove a join from a layer. |

### Data and attributes (11)

| Tool | Purpose |
|---|---|
| `get_features` | Read attribute rows from a layer, table or dataset path, with where, field subset, ordering and WKT geometry. |
| `count_features` | Count features, optionally with a where clause. |
| `get_unique_values` | Distinct values of a field with the row count for each. |
| `get_field_statistics` | min / max / mean / median / sum / standard deviation of a numeric field. |
| `summarize_features` | Group rows by one or more fields and aggregate — the quick way to answer “how many / how much per category” without GP. |
| `insert_features` | Insert new rows into a layer or table. |
| `update_features` | Update attributes and/or geometry of rows matching a where clause. |
| `delete_features` | Delete rows matching a where clause. |
| `save_edits` | Commit pending edits; unsaved edits lock the data so deletes or overwrites fail. |
| `discard_edits` | Throw away pending edits. |
| `calculate_field` | Calculate a field, e.g. the expression "!Shape_Area! / 10000". |

### Selection (6)

| Tool | Purpose |
|---|---|
| `select_features` | Select features by SQL where clause. |
| `select_by_location` | Select features by spatial relationship to another layer. |
| `get_selection` | Report the current selection per layer (ObjectIDs and optional attribute rows). |
| `set_selection` | Select specific features by ObjectID. |
| `clear_selection` | Clear the selection on one layer or the whole map. |
| `zoom_to_selection` | Zoom the map view to the selected features. |

### Fields and datasets (11)

| Tool | Purpose |
|---|---|
| `list_fields` | List a layer's fields with type, alias, length and domain. |
| `add_field` | Add a field to a layer or table. |
| `add_fields` | Add several fields in one call. |
| `delete_field` | Delete one or more fields. |
| `alter_field` | Rename a field or change its alias/length. |
| `create_feature_class` | Create an empty feature class (project default geodatabase by default) and add it to the map. |
| `create_table` | Create an empty standalone table. |
| `create_file_geodatabase` | Create a new file geodatabase. |
| `truncate_table` | Delete every row of a table or feature class, keeping the schema. |
| `delete_dataset` | Delete a dataset from disk or a geodatabase; cannot be undone. |
| `export_features` | Export a layer honouring its selection and definition query to a new dataset. |

### Geoprocessing (7)

| Tool | Purpose |
|---|---|
| `run_geoprocessing_tool` | Run any arcpy geoprocessing tool — the universal channel for analysis; layer names resolve to live layers so selections and definition queries apply. Call describe_geoprocessing_tool first when parameter names are unknown. |
| `list_geoprocessing_tools` | Search the available geoprocessing tools by name. |
| `list_toolboxes` | List the arcpy toolbox modules and any toolboxes in the project. |
| `describe_geoprocessing_tool` | Get a tool's parameters, data types, defaults and usage text before running it. |
| `run_python_toolbox_tool` | Run a tool from a custom .pyt / .atbx / .tbx toolbox on disk. |
| `check_extension` | Check, and optionally check out, an ArcGIS extension licence. |
| `get_messages` | Messages from the most recent geoprocessing operation. |

### Symbology (6)

| Tool | Purpose |
|---|---|
| `set_layer_renderer` | Change a layer's symbology: single symbol, unique values by category, or graduated/continuous colour by a numeric field. |
| `get_layer_symbology` | Inspect a layer's renderer, class breaks, unique values and label settings. |
| `list_color_ramps` | List the colour ramps available in the project. |
| `set_layer_labeling` | Turn labels on or off and set the expression, font and halo. |
| `apply_symbology_from_layer` | Copy symbology from a .lyrx file or another layer. |
| `save_layer_file` | Save a layer with its symbology to a .lyrx file for reuse. |

### Map view (7)

| Tool | Purpose |
|---|---|
| `get_map_view` | Current camera position: centre, scale, rotation and visible extent; needs the map tab open in Pro. |
| `set_map_view` | Move the map view: set an extent, centre, scale and/or rotation. |
| `export_map_view` | Render the map view to PNG and return the image for a visual check; needs the map tab open. |
| `list_bookmarks` | List the spatial bookmarks defined on a map. |
| `create_bookmark` | Save the current view, or a given extent, as a named bookmark. |
| `apply_bookmark` | Zoom the map view to a bookmark. |
| `delete_bookmark` | Delete a map bookmark. |

### Layout and printing (16)

| Tool | Purpose |
|---|---|
| `list_layouts` | List the print layouts with page size and element counts. |
| `get_layout_info` | Inspect a layout: page setup and every element with position and size. |
| `create_layout` | Create a new layout page (by default with a map frame filling it). |
| `delete_layout` | Delete a layout from the project. |
| `add_map_frame` | Add a map frame at a page position. |
| `set_map_frame_extent` | Point a layout's map frame at a layer, an extent or a scale. |
| `add_layout_text` | Add a text element such as a title or credits. |
| `add_layout_legend` | Add a legend tied to a layout's map frame. |
| `add_layout_scale_bar` | Add a scale bar tied to the map frame. |
| `add_layout_north_arrow` | Add a north arrow tied to the map frame. |
| `add_layout_picture` | Place an image (such as a logo) on a layout. |
| `set_layout_element` | Move, resize, rename, hide or retext any layout element. |
| `delete_layout_element` | Remove an element from a layout. |
| `export_layout` | Export a layout to PDF / PNG / JPEG / SVG / TIFF; PNG/JPEG come back inline for checking. |
| `preview_layout` | Render a layout to a temporary image (nothing written) so you can check while building. |
| `export_map_series` | Export a layout's map series (map book) to a multi-page PDF. |

### Raster (5)

| Tool | Purpose |
|---|---|
| `get_raster_info` | Raster detail: bands, size, cell size, pixel type, statistics and coordinate system. |
| `set_raster_symbology` | Set a raster layer's colorizer and colour ramp. |
| `raster_calculator` | Map algebra: bind rasters to short aliases, then write a Python expression; needs Spatial Analyst. |
| `sample_raster_values` | Read raster cell values at map coordinates or a point layer's features. |
| `zonal_statistics` | Summarise raster values inside zone polygons and return the table; needs Spatial Analyst. |

### Catalog and data discovery (6)

| Tool | Purpose |
|---|---|
| `list_workspace_contents` | List the datasets inside a geodatabase or folder; defaults to the project's default geodatabase. |
| `list_folder` | List GIS files and subfolders on disk; defaults to the project home folder. |
| `describe_dataset` | Describe any dataset by path — type, geometry, coordinate system, extent, fields and row count — without adding it to a map. |
| `search_data` | Find datasets by name across the project geodatabase, home folder and folder connections — the fastest way to locate data before adding it. |
| `get_project_items` | Folder connections, database connections and toolboxes registered in the project. |
| `add_folder_connection` | Register a folder with the project so its data is easy to browse. |

### Utilities (4)

| Tool | Purpose |
|---|---|
| `execute_arcpy_code` | Run Python inside ArcGIS Pro: arcpy is already imported, arcpy.mp.ArcGISProject('CURRENT') is the open project, variables persist between calls, print output is captured and a final expression's value is returned. Use it when no dedicated tool fits. |
| `get_pump_status` | Whether the main-thread dispatcher is installed; it decides whether commands can reach the open Pro project — without it only path-based work runs. |
| `stop_pump` | Remove the main-thread dispatcher; live-project commands stop working until the user installs it again. |
| `run_batch` | Run several commands in one round trip — much faster for multi-step workflows. |

</details>

---

## 📦 What is in the package

| Path | Content | Notes |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.6 (Customer 1.16 / 1.14 Enhanced) | 115 tools (112 commands) + all GP tools; HTTP MCP 6520 / legacy TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio bridge | Adapts 6520 for clients that can only launch a local process (e.g. Claude Desktop); pure PowerShell, no Python |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode toolbox | For frequent arbitrary arcpy; shows a one-time trust dialog on first run |
| `install\runtime\guard\` | 🛡️ Dialog guard | Clicks the “third-party code” trust dialog automatically; on failure shows a topmost alert (≥30 s, with sound) |
| `install\runtime\plugin\` | 🔌 Python bridge (optional) | Started in Pro's Python window for a persistent `execute_arcpy_code` session (port 6511) |
| `install\*.cmd / *.ps1` | 🛠️ Install and self-check | Install / check / configure Claude / dialog guard / uninstall |
| `clients\` | 📁 Client setup docs | Claude Code, Claude Desktop, Codex, generic JSON |
| `docs\` | 📖 User guide, troubleshooting, working rules, changelog | Start with the user guide |
| `source\` | 📜 Full source snapshot | The source matching the shipped binaries (AGPL-3.0 compliance), including upstream LICENSE / NOTICE |

### 🔍 Dependency audit (why “fully bundled”)

- 🧩 add-in: a self-contained C# assembly that runs on the .NET Desktop Runtime 10 x64 that a supported Pro 3.7 install already provides — nothing to install.
- 🌉 stdio bridge, 🛡️ dialog guard: Windows' own PowerShell 5.1 + WinForms — nothing to install.
- 🐍 RunCode toolbox, Python bridge: they run inside ArcGIS Pro's own Python (arcpy) — no separate Python.
- ✅ **So this package has no external dependency that needs pip or a network install.** “Works offline” refers to this package's own install-and-run chain; ArcGIS Pro 3.7's own prerequisites (.NET Desktop Runtime 10 x64, WebView2 ≥132) are provided by Pro's installer.

---

## 🧪 Optional components

**🐍 RunCode toolbox** (for frequent arcpy work)
- Path: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- Have the assistant pass it to `run_python_toolbox_tool`, or add it in Pro via Insert → Toolbox → Add Toolbox.
- The first run in each Pro session shows a “third-party code” trust dialog: run `03-dialog-guard.cmd` first and the guard clicks Yes for you ✨.

**🔌 Python bridge** (when an arcpy session must persist across calls)
1. Open the Python window in ArcGIS Pro via **Analysis → Python**;
2. Paste and run (the path assumes the default install location):
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<your-user-name>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. When `CURRENT` (the open project) is needed, add one more line (it must run on the main thread):
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ Dialog guard** (recommended before RunCode or ENVI→Pro joint workflows)
```
03-dialog-guard.cmd                 # 600-second guard
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # only test the alert window (30 s)
03-dialog-guard.cmd -AlertOnly      # alert only, never clicks
```
Safety boundary: it handles only trust dialogs whose context mentions “third-party / code library”; **it never touches overwrite, delete or other confirmations** 🙅. Log: `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`.

---

## 🔒 Security notes

- 🏠 The add-in and every bridge service listen on `127.0.0.1` only — other machines on the LAN cannot connect.
- ⚖️ This software serves only this machine on `127.0.0.1`; if you modify it to serve others over a network, the AGPL-3.0 §13 source-offer obligation applies.
- 🔏 The add-in is an unsigned build: ArcGIS Pro asks you to trust it on first load — that is the normal flow; verify the package against `SHA256SUMS.txt`.
- 🕵️ The package contains no telemetry and no network reporting.
- 🧹 Uninstall: `99-uninstall.cmd` (you may keep your client configuration).

---

## 📜 Version and licence

- 🏷️ Version: **Customer 1.16 (1.14 Enhanced Edition, build 1.1.6)** (build date in `VERSION.json`; 115 tools)
- 👤 Maintainer: **Law** ([@XNYGggg](https://github.com/XNYGggg))
- 🙏 Upstream project and original author: **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)** by **Pisut Nakmuenwai** (AGPL-3.0-or-later). This package accumulates patches up to 1.1.4 on top of it (see `docs\CHANGELOG.md` and `RELEASE-NOTES.md`).
- ⚖️ License: **AGPL-3.0-or-later** (the package ships the full source snapshot and the upstream NOTICE)
- 🏷️ Trademark: “ArcGIS” is a trademark of Esri; this project is a third-party tool and is not affiliated with Esri.

🆘 FAQ in `docs\排障手册.md` (Chinese); advanced usage in `docs\使用指南.md` (Chinese). Happy mapping 🎨 (｀・ω・´)ゞ
