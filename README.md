[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-Customer%201.1.13-2ea44f?style=flat-square)](VERSION.json) [![Changelog](https://img.shields.io/badge/Docs-Changelog-6f42c1?style=flat-square)](docs/CHANGELOG.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP (Optimized) · Customer 1.1.13 (1.14 Enhanced Edition + Template Distillation) 🚀

One natural-language sentence lets your AI assistant drive the ArcGIS Pro project you already have open: add layers, change symbology, query attributes, run geoprocessing, export maps. Your data never leaves your machine — all traffic stays on `127.0.0.1` 🔒.

> **🚀 This is Customer 1.1.13 (1.14 Enhanced Edition + template distillation + template deletion, build 1.1.13) — the official, tested-and-stable, recommended release.** It stacks every fix accumulated since 1.14 (v1.1.4): 1.1.5 (F1–F10), 1.1.6 (V-1–V-7), the P0–P2 defects confirmed by the 2026-11-18 live test (1.1.7–1.1.9), 1.1.10's restart self-healing, 1.1.11's cleanup fixes, 1.1.12's template-distillation module ported from 2.x and 1.1.13's template deletion. The **GitHub Release `v1.1.13` ships the offline package** (same content as the `install/` tree in this repository); **v1.1.11 has been retired — its release and tag were removed — and this version replaces it as the recommended download**; v1.1.4 stays available as the previous stable release for rollback.

> **📌 Version note (2026-11-19)**: **Customer 1.1.13 is the current official release of the 1.x stable line — tested and recommended for daily work**; v1.1.11 is retired (its release and tag were removed) and fully superseded by 1.1.13; v1.1.4 remains the previous stable release for rollback. **2.0 is a testing line** — its optimisation is unfinished, and its GitHub Release assets and tags have been removed, so it is not downloadable for now. 2.0 code lives only on the `customer-v2.0.1` / `customer-v2.0.0` branches.

> **🙏 Upstream project and original author**: this Customer Edition is based on **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)** (original author **Pisut Nakmuenwai**, AGPL-3.0-or-later), with local patches accumulated up to **v1.1.13**. Full attribution and modification notices are in `NOTICE` and `source/SOURCE-NOTICE.md`.

This package bundles **every component** needed to run — fully offline, **no Python, no pip, no internet required**, and no dependency other than ArcGIS Pro itself. Batteries included 🔋

---

## 🆚 What is new versus 1.14 (v1.1.4)

Everything below was merged after the v1.1.4 customer package. Tool count 113 → **126** (1.1.5 added `add_table` and `restart_pro`; 1.1.12 merged the 2.x template distillation module — 10 new `template_*` tools; 1.1.13 added `template_delete`); the add-in binary is now ``669E1B74…8E99`` (432,751 bytes).

| Build | Type | Highlights |
|---|---|---|
| **1.1.5** | F1–F10 | `set_environment` values now really reach GP (per-call overrides); `describe_geoprocessing_tool` returns enums and defaults; `create_map` adds no basemap by default (`project_default` restores the old behaviour); raster/TIN zoom and symbology reads; **new `add_table`**; `export_map_view` gains `fit:"auto"`; `set_raster_symbology` accepts explicit colours/labels; `set_map_properties` epsg takes effect; **new `restart_pro`** |
| **1.1.6** | V-1–V-7 | Toolbox path typos get "Did you mean …" suggestions; named-argument errors list usable examples; `get_pump_status` returns a **structured down state** (`running:false, bridge_listening:false`) instead of a failure when the Python bridge is off; error codes (`BRIDGE_NOT_LISTENING / BRIDGE_ERROR / BRIDGE_CLOSED / BRIDGE_TIMEOUT / UNSUPPORTED`) with `last_error_code/at`; 5 s read timeout on status queries; `diagnose` actively probes port 6511 (`bridge_listening` / `probe_ms`); tool descriptions corrected (positional-only on the native host, bridge dependency noted); HTTP transport assertions hardened; bridge-down result cached for 5 s |
| **1.1.7** | P0 | `create_feature_class` / `create_table` `fields` **now actually work** — each field is added with `AddField` after creation and verified via `describe_dataset` (name/type/length); a failed or rejected field deletes the half-built dataset. `run_batch` strips images to path/bytes/sha by default (no more base64 flooding the context). `get_features` pages (`truncated` / `next_offset`) and offers `geometry_summary` / `geometry_precision`. `preview_layout` accepts `output_path` / `return_image` |
| **1.1.8** | P1 | `insert_features` / `update_features` / `delete_features` accept standalone tables and dataset paths (Table API for tables, edit session for layers). `sample_raster_values` samples pixels natively — `points` / `point_layer`, inline values, no `raster_samples` side-effect table (`write_table` keeps the old path). `create_map` SCENE fixed (`SceneLocal`). `add_join` survives GP re-registration (JoinRegistry + client-side join, with candidate suggestions). Layout element `x/y` unified to bottom-left origin, verified via `GetBounds`, out-of-range rejected. `run_batch` reports per-step milliseconds and serial execution. GP outputs added to the map wait and retry |
| **1.1.9** | P2 | `get_layer_info` / `set_raster_symbology` / `repair_layer_source` accept dataset paths (unique match resolves, multiple matches list layer names). `repair_layer_source` gained a raster branch (`RasterDataset.ReplaceDataSource`). `search_data` de-duplicates. `check_extension` reports `inspectable:false` semantics (Pro does not expose checkout to add-ins). GP outputs filter empty strings. `get_features` suggests `get_raster_info` when given a raster layer name |
| **1.1.10** | Restart self-healing | `restart_pro` probes the save dialog in a child process with a hard 15 s timeout (give up and log on timeout); button matching accepts localised `是(Y) / 否(N) / 取消(C)`; `restart.log` is UTF-8 with phase heartbeats; TCP connect has a 5 s timeout; it still never force-kills Pro. The restart scheduling flag expires after 10 minutes so a crashed assistant no longer blocks new restart requests. Deployment normalised to `scripts/update_installed.py` (positional, refuses hash mismatch) |
| **1.1.11** | Cleanup fixes | Failure cleanup of `create_feature_class` / `create_table` now resolves the map layers and standalone tables that reference the half-created dataset *before* deleting it and removes them by object identity — an unrelated layer that merely shares the name is left alone; cleanup failures are appended to the tool error instead of being swallowed. `scripts/update_installed.py` no longer hardcodes machine paths, and the build maps source paths out of the shipped DLL/PDB |
| **1.1.12** | Template distillation | Merges the 2.x template distillation module: 10 `template_*` tools + the `arcgis-distill` CLI (runs on Pro's own Python, no extra dependency) + a local template library (`%LOCALAPPDATA%\ArcGIS Pro MCP\library`, kept on upgrades); new `skills/template-distill` and `docs/distill.md`; tool count 115→125, commands 112→122 |
| **1.1.13** | Template deletion | The template library gains `template_delete` — deletes a template's spec, index entry, job sheet and its deduplicated sample image (kept while another template still references it); approved (ready) templates need `force`; tool count 125→126, commands 122→123 |

Verification on this machine (live, **ArcGIS Pro 3.7 only**): 1.1.7 regression 21/21 PASS; 1.1.8 two consecutive rounds with zero failures; 1.1.9 P2 10/10 plus a P1 re-run with zero failures; 1.1.10 handshake `1.1.10.0`; 1.1.11 handshake `1.1.11.0`, ping `command_count=112`, the D1 fields regression passes every assertion that applies to the current test project (four legacy assertions reference layers from a retired project), and `get_features` paging / summary / precision were re-verified live. Other Pro versions are not covered.

1.1.13 (template deletion): build passed end to end (client-registration / installer / HTTP on net8.0+net10.0); `tests/test_distill.py` 13/13 (the six delete cases cover image removal, shared-image safety, not_found, the ready gate, path-like ids and orphan index entries) and `tests/test_catalog_matches_bridge.py` 4/4 passed; `tools.json` and `catalog.py` are fully equivalent at 126 tools / 123 commands. The 1.1.12 live acceptance (handshake, 125 tools, the full `template_*` flow 11/11, self-check 21 passed / 0 failed) is recorded in `docs/CHANGELOG.md`. Details are in `docs/CHANGELOG.md` and `RELEASE-NOTES.md`. 1.1.13 (template deletion) live acceptance (2026-11-19, Pro 3.7, real terminal): the installed build handshakes as `1.1.13.0`, `ping` returns **123** commands, `tools/list` advertises **126** tools including `template_delete`; a full import → measure → delete round-trip through the installed `run-distill.ps1` passed (repeat delete → `not_found`/exit 3), and the official self-check reports **22 passed / 0 failed**. 1.1.13 (template deletion) live acceptance (2026-11-19, Pro 3.7, real terminal): the installed build handshakes as `1.1.13.0`, `ping` returns **123** commands, `tools/list` advertises **126** tools including `template_delete`; a full import → measure → delete round-trip through the installed `run-distill.ps1` passed (repeat delete → `not_found`/exit 3), and the official self-check reports **22 passed / 0 failed**.

---

## 🖥️ System requirements

| Item | Requirement |
|---|---|
| 🪟 OS | **Windows 11 x64** (64-bit Home / Pro / Enterprise; ArcGIS Pro 3.7 officially supported). **Architecture: x64 (AMD64) only — ARM64 is not supported** |
| 🗺️ ArcGIS Pro | **3.7** (built against Pro 3.7 / .NET 10; live-verified on 3.7 only — later versions are untested) |
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

Add to `~/.codex/config.toml` (replace any old `arcgis` block entirely, including an old `env` sub-table):

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

Restart Codex afterwards.

### 🔧 Other MCP clients

Connect over Streamable HTTP to `http://127.0.0.1:6520/mcp`, or use the generic JSON above. (Antigravity uses `serverUrl` and no `type`.)

### ✅ Verify the connection

- `install\01-check.cmd` — checks the Pro version, add-in hash, runtime, ports and client configuration.
- `netstat -ano | findstr 6520` — confirm the port is listening.
- Log: `%LOCALAPPDATA%\ArcGIS-MCP\addin.log`.

---

## 🎯 How to use it

**The basic loop**: open a project with data → say what you want → the assistant picks the tools → look at the result (images come back inline) 🎨

| You can say | What actually happens |
|---|---|
| “Classify the counties by area, light red to dark green” | `set_layer_renderer` (graduated colours) |
| “Select counties with population over 500k and export them as a new feature class” | `select_features` → `export_features` |
| “Buffer the roads by 500 metres” | `run_geoprocessing_tool` (Buffer) |
| “Sum the farmland area per township” | `summarize_features` / `zonal_statistics` |
| “Take a screenshot of the current map view” | `export_map_view` (returns a PNG) |
| “Make an A3 portrait PDF with title, legend, scale bar and north arrow” | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| “Export 12 layers with the same symbology in one go” | `run_batch` or `execute_arcpy_code` |
| “Create an empty feature class with fields NAME, AREA_HA and CODE” | `create_feature_class` (fields are really created and verified) |

**Call by tool name**: every tool below can be called by name by your client; `run_batch` strings several commands into one round trip; when no dedicated tool fits, fall back to `execute_arcpy_code`.

**Editing rules**: edits stay pending until `save_edits`, and pending edits lock the data (deletes/overwrites fail); `discard_edits` throws them away. `delete_features` / `delete_dataset` cannot be undone — have the assistant explain the impact first.

**Batch and speed**: for repetitive or large work, ask the assistant to write an idempotent arcpy script for RunCode and run it once; for big projects, chunk by extent/OID with checkpoints. Inside `run_batch`, image results are automatically reduced to path/bytes/sha so they don't flood the conversation.

---

## 🧩 The three modules: what each does, how to use it, and how to ask

The project is designed as “one repository, three modules”; each stands alone, and in practice they are used together:

| Module | One-line purpose | When to use it | Status in this build |
|---|---|---|---|
| 🧩 **MCP add-in** | The only road between AI and Pro: 126 MCP tools plus two arcpy channels (RunCode / Python bridge) | All daily operations; batch scripts; interactive sessions that need `CURRENT` | ✅ Works out of the box |
| 🛡️ **Dialog guard** | Clicks the “third-party code library” trust dialog for you; falls back to a topmost ringing reminder | The first RunCode.pyt run; unattended batch jobs | ✅ Standalone script `03-dialog-guard.cmd` |
| 🧪 **Template distillation** | Distils a sample map image into a reusable layout template for consistent batch output | Many maps with one layout; reproducing paper/report figures | ✅ Included since 1.1.12 (ported from 2.x) |

### 🧩 Module one: the MCP add-in (the core)

- **What it does**: the only channel between AI and Pro — layers, symbology, geoprocessing, map production all go through it. arcpy runs through two channels: **RunCode toolbox** for one-off / batch scripts; **Python bridge** for sessions that need persistent variables or `CURRENT`.
- **How to use it**:
  1. After installation, open Pro and confirm the **MCP tab** says **running** (`127.0.0.1:6520`);
  2. RunCode: have the assistant call `run_python_toolbox_tool` with the path `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`;
  3. Python bridge (optional): Pro menu **Analysis → Python**, paste the bridge start-up code (port 6511; add `start_pump()` when you need `CURRENT` — see “RunCode and the Python bridge” below).
- **How to ask**: state **object + operation + output**; for batch jobs add “do one first so I can confirm”.
  - ✅ “Classify `counties` by `AREA` into 5 natural-break classes, light red to dark green, legend bottom-right, export an A3 landscape PDF to `D:\out\`, 300 dpi.”
  - ✅ “Use RunCode with an idempotent script: turn on labels for every layer, field `NAME`, size 10; skip layers already labelled.”
  - ✅ “This arcpy needs `CURRENT`: fill in the bridge start-up code with my user name and send it to me; I'll paste it into Pro myself.”
  - ❌ “Process the data for me.” — no object, operation or output; the assistant can only keep asking.

### 🛡️ Module two: the dialog guard (only needed for RunCode)

- **What it does**: the first time Pro runs a third-party `.pyt` it shows a “third-party code library” trust dialog; unattended jobs hang there. The guard clicks only that one “Yes(Y)”; if it cannot click it raises a topmost reminder (at least 30 s, ringing every 10 s); overwrite / delete and other confirmation dialogs are **never touched**.
- **How to use it** (details in “Using the dialog guard” below):
  ```text
  install\03-dialog-guard.cmd                    # 600 seconds by default
  install\03-dialog-guard.cmd -DurationSeconds 120
  install\03-dialog-guard.cmd -AlertOnly         # remind only, do not click
  ```
  Each Pro process asks once; restart Pro and you need the guard again.
- **How to ask**: put “start the guard” and “what to run” in the same sentence.
  - ✅ “I'm going to run RunCode.pyt: start the dialog guard in the background for 600 s, click Yes on the trust dialog, and ring a topmost reminder if you can't.”
  - ✅ “This time remind only, do not click.”
  - ❌ “Dismiss all dialogs for me.” — the guard handles trust dialogs only; other confirmations are yours to decide.

### 🧪 Module three: template distillation (included since 1.1.12, ported from 2.x)

- **What it does**: distils a sample finished map into a reusable layout template; later batch exports reuse the template for a consistent, reproducible look.
- **How to use it**: **included in this build** (since 1.1.12, ported from 2.x) — just tell the assistant “distil this figure into a template”; it then drives the 11 `template_*` tools through `import → measure → patch → validate → preview → review → approve → apply`. Deterministic steps run in `arcgis-distill`; semantic fields are written by the assistant per `skills/template-distill/SKILL.md`; the template library lives in `%LOCALAPPDATA%\ArcGIS Pro MCP\library` and upgrades keep it.
- **How to ask**: name the template and spell out the flow and confirmation points.
- **Cleaning up**: `template_delete` (or `arcgis-distill delete <id> [--force]`) removes a template — its spec, index entry, job sheet and its sample image when no other template references it; an approved (ready) template needs `force`.
  - ✅ “Distil `D:\samples\fig3.png` into a template named 'Study area overview': import → measure → patch → validate → preview; send me the preview first, then review + approve after I confirm.”
  - ✅ “Use the 'Study area overview' template to export every map in `maps` as A3 PDF, title = map name, output to `D:\out\maps\`.”
  - ❌ “Do it like this picture.” — no template name, flow or confirmation points; the layout cannot be reproduced.

### 🧭 How to phrase prompts (applies to all three modules)

1. **Name the module**: plain operations need no name (MCP by default); say “use **RunCode**” for scripts; “use the **Python bridge**” for persistent sessions; start the **dialog guard** first for unattended runs; say “use the **template**” for consistent layouts.
2. **Be concrete about objects**: layer names, field names and paths in backticks, not “that layer” or “the data from earlier”.
3. **Pin down the output**: format (PNG / PDF / new feature class), size or resolution, save path.
4. **Leave a confirmation point**: for batch, overwrite or delete work, ask for “one first” or “explain the impact first”.

> For the full four elements and templates of map-production prompts, see the next section.

---

## 🎨 Mapping and map production (prompt guide)

This section explains “how to talk to the AI” in full: you don't need tool names — state the four elements and the assistant breaks it into MCP calls.

### Say four things and get it right the first time

| Element | What to state | Example |
|---|---|---|
| Data | Layer names, data paths, current selection | “the `county boundaries` layer in the current map” |
| Operation | Classification field/method, buffer distance, statistics | “5 natural-break classes on `AREA`” |
| Style | Colours, font sizes, legend/scale-bar/north-arrow positions, basemap | “light red to dark green, legend bottom-right” |
| Output | Image/PDF/new feature class, resolution, save path | “A3 landscape PDF, 300 dpi, to `D:\out\`” |

**Good prompts vs bad prompts**
- ❌ “Beautify the map for me” — no goal; the assistant can only guess.
- ✅ “Symbolise `land use` by unique values of `DLMC`, legend bottom-right, then export an A3 landscape PDF to `D:\out\landuse.pdf` at 300 dpi.”
- ❌ “Make a map” — no extent, layout or output.
- ✅ “Using the `study area` layer as the extent, scale 1:10000, produce an A4 portrait PNG titled 'Study area land use', with legend, scale bar and north arrow; send me a preview first.”

### Prompt cheat sheet by tool group

| Module (tool group) | You could say | The assistant uses |
|---|---|---|
| Session & project | “Run `diagnose` first to see the connection and current project; then save the project to `D:\proj\result.aprx`” | diagnose / save_project |
| Layers | “Move `roads` into the `base` group, transparency 40%, visible only beyond 1:50000; turn off `POI`” | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| Data & attributes | “Read the first 20 rows of `parcels` sorted by area descending; summarise `DLMC` area by class” | get_features / summarize_features |
| Selection | “Select counties with `POP` over 500k and export them to `D:\out\counties_big.shp`; then select parcels inside `study area` by location” | select_features / select_by_location / export_features |
| Fields & datasets | “Add a DOUBLE field `AREA_HA` to `parcels` and calculate `!Shape_Area!/10000`; create an EPSG:4547 polygon feature class `result_2026`” | add_field / calculate_field / create_feature_class |
| Geoprocessing | “Buffer the roads by 500 m and dissolve; clip the land-use raster with the `study area` mask” | run_geoprocessing_tool / run_batch |
| Symbology | “5 classes on area, natural breaks, light red to dark green; or unique values by `TYPE`” | set_layer_renderer |
| Map view | “Zoom to `study area`, scale 1:10000, give me a 1600×900 PNG” | set_map_view / zoom_to_layer / export_map_view |
| Layout & printing | “Create an A3 landscape layout, title …, legend with only 3 layers, scale bar and north arrow, export a 300 dpi PDF” | create_layout / add_layout_* / export_layout |
| Raster | “Read the DEM's bands, cell size and statistics; compute NDVI” | get_raster_info / raster_calculator / zonal_statistics |
| Catalog & discovery | “Search the project and folder connections for shapefiles with `2025` in the name” | search_data / describe_dataset / list_folder |
| Utilities | “In one go: hide the basemap → zoom to the counties → export a PNG; prefix all layer names with arcpy” | run_batch / execute_arcpy_code |

### Standard map-production prompt template (copy and use)

```text
Produce a map from the <map name> in the current project:
- Extent: <study area layer/extent>, scale 1:<...>
- Data: <layer 1>, <layer 2> (fields and filters if needed)
- Symbology: <graduated by field/unique values/single colour>, palette <ramp or hex>
- Layout: <A3 landscape / A4 portrait>, title "<title>", legend <position>, scale bar <position>, north arrow <position>
- Output: <PDF / PNG>, <300> dpi, save to <absolute path>
- Flow: check the layout first, send me the preview, export the final file after I confirm.
```

### Batch map-production prompt

```text
For every map in `maps`, export a PDF with the same A3 landscape layout:
title = map name, legend = visible layers, scale bar and north arrow bottom-right,
output to `D:\out\maps\`, 300 dpi. Export the first one for review, then batch after I confirm.
```

### One-line fixes for common mishaps

- “The legend covers the main map” → the assistant adjusts the `legend` coordinates or the layout.
- “The page proportion is wrong / too much white space” → adjust the map frame vs page ratio.
- “The colours are too loud / not paper-style” → give a ramp, hex values or a reference image.
- “Don't export yet — show me a preview” → the assistant uses `export_map_view` or the layout preview first.

**Effect**: state data, operation, style and output in one go and the assistant usually gets close to the final figure in one round; later changes only need the difference (“move the legend right by 1 cm”).

---

## 🛡️ Using the dialog guard (1.x edition)

**What it solves**: the first time ArcGIS Pro runs a third-party `.pyt` / Python tool (such as RunCode.pyt) it shows a “third-party code library” trust dialog; unattended jobs hang. This build has no MCP guard tool; the package ships a **standalone guard script** instead: it polls Pro's windows, clicks only that “Yes(Y)”, and exits after the configured duration.

**How to start it**

```text
install\03-dialog-guard.cmd                    # 600 seconds by default
install\03-dialog-guard.cmd -DurationSeconds 120
install\03-dialog-guard.cmd -AlertOnly         # topmost reminder only, no clicking
install\03-dialog-guard.cmd -NotifyTest        # test the reminder window (30 s)
```

You can also double-click `03-dialog-guard.cmd`, or have an AI assistant that can run terminal commands start it in the background.

**Prompt examples**

| You say | What the assistant does |
|---|---|
| “I'm going to run RunCode.pyt manually in Pro. Start the dialog guard for 600 s first; click Yes on the third-party trust dialog, and ring a topmost reminder if you can't” | runs `install\03-dialog-guard.cmd -DurationSeconds 600` in the background |
| “This time remind only, do not click” | starts it with `-AlertOnly` |
| “Test whether the reminder window can pop up” | starts `-NotifyTest` (30 s) |
| “Where is the guard log?” | `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log` |

**Boundaries and effects**
- Only handles trust dialogs with “third-party / code library / 第三方 / 代码库” context; overwrite, delete and other confirmations are never touched.
- When the button cannot be found, a topmost reminder stays at least 30 s and rings every 10 s.
- Each Pro process asks once; you need the guard again after restarting Pro.
- 2.x turned the same capability into `guard_summon` / `guard_status` with automatic arm/settle around RunCode calls; this 1.x build needs the script started manually.

---

## 🐍 RunCode and the Python bridge (prompt guide)

The arcpy channels have two tiers: the **RunCode toolbox** for one-off/batch scripts; the **Python bridge** for interactive sessions that need persistent variables.

### RunCode toolbox

Path: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`. Have the assistant call `run_python_toolbox_tool` with that path; you can also add it yourself via “Insert → Toolbox → Add Toolbox” in Pro.

**Prompt examples**
- “Use RunCode.pyt to turn on labels for every layer in the current project, field `NAME`, size 10.”
- “Write an idempotent arcpy script and run it with RunCode to prefix every layer name with `LYR_`; skip ones already prefixed.”
- “This script will run inside RunCode.pyt — check the parameters and paths first, then execute.”

**The first run** shows the “third-party code library” trust dialog: start the dialog guard above, or click Yes once yourself. One trust per Pro process.

### Python bridge (when you need `CURRENT` or persistent variables)

1. Open the Python window via **Analysis → Python** in Pro;
2. Paste and run (path at the default install location):

```python
import sys; sys.path.insert(0, r"C:\Users\<your-user-name>\AppData\Local\ArcGIS Pro MCP\plugin")
import mcp_bridge; print(mcp_bridge.start_server(port=6511))
```

3. Add one more line when you need `CURRENT` (the current project; must run on the main thread):

```python
print(mcp_bridge.start_pump())
```

**Prompt examples**
- “The next arcpy needs `CURRENT`. Remind me to paste the bridge start-up code into Pro's Python window, then run it with `execute_arcpy_code`.”
- “Fill in the bridge start-up code with my user name and copy it to me; I'll paste it into Pro.”
- “Run `get_pump_status` to see whether the pump is ready; if not, remind me to paste.”
- “Use a persistent session in three steps: read the layers, create the field, calculate — variables must survive across calls.”

**Effect**: RunCode handles “one batch script in one go”; the Python bridge handles “multi-step interaction against the current project”; neither needs a separate Python installation.

---

## 🧪 About the distillation module (included since 1.1.12)

Since **1.1.12** this branch **includes** template distillation (ported wholesale from 2.x, with `template_delete` added in 1.1.13): 11 `template_*` tools + the `arcgis-distill` CLI + a local template library. The guard tools (`guard_summon` / `guard_status`) unique to 2.x are still not in this line — the dialog guard stays a standalone script (`03-dialog-guard.cmd`).

- 11 `template_*` tools: import / measure / patch / validate / preview / review / approve / apply / list / get / delete;
- the template library lives in `%LOCALAPPDATA%\ArcGIS Pro MCP\library`; upgrades keep existing templates;
- deterministic steps (measure / validate / preview / import) run in `arcgis-distill` on Pro's own Python with no extra dependency; semantic fields are written by the assistant per `skills/template-distill/SKILL.md`; the tools themselves need no API key;
- typical use: “Distil `D:\samples\fig3.png` into a template named Study-area overview; run import → measure → patch → validate → preview, show me the preview, then review + approve.”
- cleaning up: `arcgis-distill delete <id> [--force]` removes the template together with its unreferenced sample image; a ready template needs `force`.

Capability comparison between 1.1.13 and 2.x:

| Capability | 1.1.13 (this branch) | 2.x |
|---|---|---|
| Native MCP tools | 126 (13 groups) | 125 (14 groups, incl. 2 guard tools) |
| RunCode / Python bridge | ✅ | ✅ |
| Dialog guard | Standalone script `03-dialog-guard.cmd` | `guard_summon` / `guard_status` + automatic arm/settle around RunCode |
| Template distillation | ✅ 11 `template_*` tools + schema v1.1 | ✅ Same |

---

## 🧰 All 126 MCP tools (13 groups)

Everything the assistant can do, listed by name. Click to expand 👇

<details>
<summary><b>📜 Full tool catalogue — Session 16 · Layers 20 · Data 11 · Selection 6 · Fields/Datasets 11 · Geoprocessing 7 · Symbology 6 · View 7 · Layout 16 · Raster 5 · Catalog 6 · Utilities 4 · Template distillation 11</b></summary>

### Session and project (16)

| Tool | Purpose |
|---|---|
| `ping` | Check that the bridge is reachable and return the project it is attached to. |
| `get_capabilities` | List every command the bridge supports (grouped by area) to discover functionality beyond the named tools. |
| `get_queue_status` | Three-lane queue status: per-lane depth, the command running now and recent completions; check it when calls feel slow or queued. |
| `diagnose` | Connection self-check: Pro version, licence, open project, active map, map view and write access; also actively probes the Python bridge port (`bridge_listening` / `probe_ms`). Run it first when something is wrong. |
| `get_arcgis_info` | ArcGIS Pro version, licence level, available extensions and the current project path. |
| `get_project_info` | Project paths, default geodatabase and toolbox, maps, layouts, folder and database connections. |
| `save_project` | Save the project (.aprx), or save a copy elsewhere. |
| `list_maps` | List all maps and scenes with coordinate system and layer counts. |
| `create_map` | Create a new map or scene in the project (no basemap by default; `project_default` restores one). |
| `remove_map` | Delete a map from the project. |
| `activate_map` | Open/activate a map view in the Pro UI; required before camera commands and export_map_view. |
| `set_map_properties` | Rename a map or change its coordinate system (`epsg` takes effect). |
| `get_map_extent` | Combined extent of every data layer in a map. |
| `get_environment` | Read the current arcpy geoprocessing environments. |
| `set_environment` | Set arcpy environments (workspace, output coordinate system, extent, mask, cell size, overwrite output, parallel factor, snap raster, …); values really reach GP and can be overridden per call. |
| `restart_pro` | Restart ArcGIS Pro after saving and reopening the project — how a rebuilt add-in gets loaded without touching the desktop; the reply returns before Pro closes and clients reconnect when the bridge answers. |

### Layers (20)

| Tool | Purpose |
|---|---|
| `get_layers` | List layers (with draw order and group nesting) and standalone tables in a map; start here to see what a project holds. |
| `get_layer_info` | Full detail for one layer: source, coordinate system, extent, fields, feature count, renderer and label state. Also accepts a dataset path (1.1.9+). |
| `add_layer` | Add data to a map from a path or service URL: feature class, shapefile, raster, table, .lyrx layer file or web service. |
| `add_table` | Add a standalone table (geodatabase table or folder table) to a map. |
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
| `repair_layer_source` | Repoint a layer at a new workspace or dataset to fix a broken source (including raster layers, 1.1.9+). |
| `add_join` | Join a table to a layer on a common field (client-side join registry; survives GP re-registration). |
| `remove_join` | Remove a join from a layer. |

### Data and attributes (11)

| Tool | Purpose |
|---|---|
| `get_features` | Read attribute rows from a layer, table or dataset path, with where, field subset, ordering and geometry. Pages report `truncated` and `next_offset`; geometry can be a compact summary or full WKT with a precision. |
| `count_features` | Count features, optionally with a where clause. |
| `get_unique_values` | Distinct values of a field with the row count for each. |
| `get_field_statistics` | min / max / mean / median / sum / standard deviation of a numeric field. |
| `summarize_features` | Group rows by one or more fields and aggregate — the quick way to answer “how many / how much per category” without GP. |
| `insert_features` | Insert new rows into a layer, standalone table or dataset path. |
| `update_features` | Update attributes and/or geometry of rows matching a where clause (layers and standalone tables). |
| `delete_features` | Delete rows matching a where clause (layers and standalone tables). |
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
| `create_feature_class` | Create an empty feature class (project default geodatabase by default) and add it to the map; requested fields are added after creation and verified — a mismatch deletes the new dataset and fails. |
| `create_table` | Create an empty standalone table; requested fields are added after creation and verified — a mismatch deletes the new table and fails. |
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
| `describe_geoprocessing_tool` | Get a tool's parameters, data types, defaults, enums and usage text before running it. |
| `run_python_toolbox_tool` | Run a tool from a custom .pyt / .atbx / .tbx toolbox on disk; the native host takes positional parameters only. A mistyped path gets a “Did you mean …” suggestion. |
| `check_extension` | Report whether the add-in can inspect an extension licence (`inspectable:false` on Pro — run the tool itself to learn whether a licence is missing). |
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
| `export_map_view` | Render the map view to PNG and return the image for a visual check; needs the map tab open. Supports `fit:"auto"` to match the view's aspect ratio. |
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
| `set_layout_element` | Move, resize, rename, hide or retext any layout element (page coordinates use the bottom-left origin; out-of-range positions are rejected). |
| `delete_layout_element` | Remove an element from a layout. |
| `export_layout` | Export a layout to PDF / PNG / JPEG / SVG / TIFF; PNG/JPEG come back inline for checking. |
| `preview_layout` | Render a layout to a temporary image (nothing written unless `output_path` is given) so you can check while building. |
| `export_map_series` | Export a layout's map series (map book) to a multi-page PDF. |

### Raster (5)

| Tool | Purpose |
|---|---|
| `get_raster_info` | Raster detail: bands, size, cell size, pixel type, statistics and coordinate system. |
| `set_raster_symbology` | Set a raster layer's colorizer and colour ramp; accepts explicit colours and labels. |
| `raster_calculator` | Map algebra: bind rasters to short aliases, then write a Python expression; needs Spatial Analyst. |
| `sample_raster_values` | Read raster cell values at map coordinates (`points`) or a point layer's features — inline, with no side-effect table unless `write_table=true`. |
| `zonal_statistics` | Summarise raster values inside zone polygons and return the table; needs Spatial Analyst. |

### Catalog and data discovery (6)

| Tool | Purpose |
|---|---|
| `list_workspace_contents` | List the datasets inside a geodatabase or folder; defaults to the project's default geodatabase. |
| `list_folder` | List GIS files and subfolders on disk; defaults to the project home folder. |
| `describe_dataset` | Describe any dataset by path — type, geometry, coordinate system, extent, fields and row count — without adding it to a map. |
| `search_data` | Find datasets by name across the project geodatabase, home folder and folder connections — the fastest way to locate data before adding it; results are de-duplicated. |
| `get_project_items` | Folder connections, database connections and toolboxes registered in the project. |
| `add_folder_connection` | Register a folder with the project so its data is easy to browse. |

### Utilities (4)

| Tool | Purpose |
|---|---|
| `execute_arcpy_code` | Run Python inside ArcGIS Pro: arcpy is already imported, arcpy.mp.ArcGISProject('CURRENT') is the open project, variables persist between calls, print output is captured and a final expression's value is returned. Use it when no dedicated tool fits. Answered by the Python bridge (port 6511). |
| `get_pump_status` | Whether the main-thread dispatcher is installed. When the Python bridge is not running it returns a structured down state (`running:false, bridge_listening:false`) with a code and hint instead of an error. |
| `stop_pump` | Remove the main-thread dispatcher; live-project commands stop working until the user installs it again. |
| `run_batch` | Run several commands in one round trip — much faster for multi-step workflows. Image results are reduced to path/bytes/sha by default, and each step reports its milliseconds. |

### Template distillation (11)

| Tool | What it does |
|---|---|
| `template_list` | Lists layout templates in the local library, optionally filtered by status or tag. |
| `template_get` | Reads one template's full spec (layout geometry, palette, typography, regions, pitfalls and review record). |
| `template_import` | Imports a sample map figure into the library as a draft, deduplicated by image sha256; next step is template_measure. |
| `template_measure` | Measures the sample figure — figure bbox, aspect, strong frame lines, palette and DPI — into the un-editable measured block. |
| `template_patch` | Submits semantic fields (regions / legend / typography / palette / pitfalls) as a validated merge; protected fields such as measured are rejected. |
| `template_validate` | Runs schema and geometric consistency checks (frame/regions inside the canvas, measured tolerance ≤3%, typography order, colours, pitfalls). |
| `template_review` | Records a human review decision (reviewer + notes) for a validated template. |
| `template_approve` | Approves a reviewed, validated template to ready; refuses on validation errors or a missing review unless force is set (recorded). |
| `template_preview` | Renders the template as a wireframe PNG side by side with the sample figure for visual checking. |
| `template_apply` | Writes a ready template into a mapping context JSON (layout / palette / typography) for the assistant to compose the map with. |
| `template_delete` | Deletes a template from the library — its spec, index entry, job sheet and its sample image when no other template references it; approved (ready) templates need force. |
</details>

---

## 📦 What is in the package

| Path | Content | Notes |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.13 | 126 MCP tools + all GP tools; HTTP MCP 6520 / legacy TCP 6510 |
| `install\runtime\distill\` | 🧪 Template distillation | `arcgis-distill` CLI + the `arcgis_agentkit` package on Pro's own Python; template library at `%LOCALAPPDATA%\ArcGIS Pro MCP\library` (kept on upgrades) |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio bridge | For clients that can only launch a local process (e.g. Claude Desktop); pure PowerShell, no Python |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode toolbox | For frequent arbitrary arcpy; asks for trust once on first run |
| `install\runtime\guard\` | 🛡️ Dialog guard | Clicks the “third-party code library” trust dialog; topmost reminder (≥30 s, ringing) on failure |
| `install\runtime\plugin\` | 🔌 Python bridge (optional) | Started in Pro's Python window; provides the `execute_arcpy_code` persistent session (port 6511) |
| `install\*.cmd / *.ps1` | 🛠️ Install & self-check | Install / check / configure Claude / dialog guard / uninstall |
| `clients\` | 📁 Client setup notes | Claude Code, Claude Desktop, Codex, generic JSON |
| `docs\` | 📖 Usage guide, troubleshooting, work rules, changelog | Start with the usage guide |
| `source\` | 📜 Full source snapshot | The source behind these binaries (AGPL-3.0 compliance), with upstream LICENSE / NOTICE and the build products in `source\dist\` |

### 🔍 Dependency audit (why “fully bundled” holds)

- 🧩 add-in: a self-contained C# assembly running on the .NET Desktop Runtime 10 x64 that **ArcGIS Pro 3.7 lists among its system requirements** — nothing extra to install *for this package*.
- 🌉 stdio bridge, 🛡️ dialog guard: Windows' built-in PowerShell 5.1 + WinForms — nothing extra to install.
- 🐍 RunCode toolbox, Python bridge: run inside ArcGIS Pro's own Python (arcpy) — no separate Python needed.
- ✅ **So the package has no external dependency that needs pip or internet.** “Works offline” refers to this package's own install and run chain. ArcGIS Pro 3.7's system requirements (Microsoft .NET Desktop Runtime 10 x64 and WebView2) are Esri's prerequisites — if either is missing on this machine, install it as Esri's system requirements describe (the Pro installer normally prompts for or installs them).

---

## 🧪 Using the optional components

**🐍 RunCode toolbox** (frequent arcpy)
- Path: `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- Have the AI call `run_python_toolbox_tool` with that path; or add it in Pro via “Insert → Toolbox → Add Toolbox”.
- Each Pro session asks for trust on first run: run `03-dialog-guard.cmd` first and the guard clicks Yes for you ✨.

**🔌 Python bridge** (when arcpy state must survive across calls)
1. Open the Python window via **Analysis → Python** in ArcGIS Pro;
2. Paste and run (path at the default install location):
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<your-user-name>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. Add one more line when you need `CURRENT` (must run on the main thread):
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ Dialog guard** (recommended before RunCode / ENVI→Pro joint workflows)
```
03-dialog-guard.cmd                 # 600 s guard
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # test the reminder window (30 s)
03-dialog-guard.cmd -AlertOnly      # remind only, do not click
```
Safety boundary: handles only trust dialogs with “third-party / code library / 第三方 / 代码库” context; **never touches overwrite, delete or other confirmations** 🙅. Log: `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`.

---

## 🔒 Security notes

- 🏠 The add-in and all bridge services listen on `127.0.0.1` only; other machines on the LAN cannot connect.
- ⚖️ This software serves only this machine on `127.0.0.1` and provides no network service to third parties; if you modify it to serve others over a network, the AGPL-3.0 §13 source-offer obligation applies.
- 🔏 The add-in is an unsigned build: ArcGIS Pro asks you to confirm trust on first load, which is the normal flow; verify the package against `SHA256SUMS.txt`.
- 🕵️ This package contains no telemetry and no network reporting.
- 🧹 Uninstall: `99-uninstall.cmd` (you may keep the client configuration).

---

## 📜 Version and licence

- 🏷️ Version: **1.1.13** (Customer 1.1.13, 1.14 Enhanced Edition + template distillation; build date in `VERSION.json`; 126 tools)
- 👤 Maintainer: **Law** ([@XNYGggg](https://github.com/XNYGggg))
- 🙏 Upstream project and original author: **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**, author **Pisut Nakmuenwai** (AGPL-3.0-or-later). This package accumulates local patches up to 1.1.12 (see `docs\CHANGELOG.md` and `RELEASE-NOTES.md`).
- ⚖️ Licence: **AGPL-3.0-or-later** (the package ships the full source snapshot and upstream NOTICE)
- 🏷️ Trademarks: “ArcGIS” is a trademark of Esri; this project is a third-party tool and is not affiliated with Esri.

🆘 Troubleshooting: `docs\排障手册.md`; advanced usage: `docs\使用指南.md`. Happy mapping 🎨 (｀・ω・´)ゞ

---

<!-- LDU-THANKS -->

<h2 align="center">🙏 Acknowledgements</h2>

<p align="center">
  This project's research and development was supported by <strong>Ludong University</strong> (鲁东大学), to which we extend our sincere thanks.
</p>

<p align="center">
  <a href="https://www.ldu.edu.cn/"><img src="assets/ldu-emblem.png" width="104" alt="Ludong University emblem"></a>
</p>

<h3 align="center">鲁东大学 · Ludong University</h3>

> Ludong University, founded in 1930 and located in Yantai, Shandong, is a provincial comprehensive university centred on applied talent development with coordinated development of engineering, agriculture, science and liberal arts. It now has 26 colleges; six disciplines including Engineering, Chemistry and Materials Science rank in the global ESI top 1%, and it upholds the motto “厚德、博学、日新、笃行” (virtue, broad learning, daily renewal, earnest practice).

<p align="center">
  📍 186 Hongqi Middle Road, Zhifu District, Yantai, Shandong 264025, China<br>
  🌐 <a href="https://www.ldu.edu.cn/">University site</a> ·
  <a href="https://jwc.ldu.edu.cn/">Academic affairs</a> ·
  <a href="https://lib.ldu.edu.cn/">Library</a> ·
  <a href="https://ehall.ldu.edu.cn/">Online services</a>
</p>
