# Changelog

Notable changes, newest first. Versions are the add-in version in
`addin/ArcGISProMCP/Config.daml`, which is what the ribbon, the installer and
the MCP handshake all report.

## 1.1.13 — 2026-11-19

### Added

- **`template_delete`** — the template library can delete a template again. The
  tool wraps a new `arcgis-distill delete <id> [--force]` command that removes
  the spec, the index entry, the job sheet and the deduplicated sample image
  when no other template still references it; a shared image is kept and
  reported (`removed.image_kept_for`). Deleting an unknown id reports
  `not_found` (exit 3) and a repeated delete is safe; an approved (`ready`)
  template needs `--force` (exit 5 otherwise); `inbox` is untouched. This
  closes the acceptance-cleanup gap where clearing a verification template
  meant hand-editing the index.
- `tests/test_distill.py` covers deletion (unshared image removed, shared
  image kept, `not_found` for unknown ids, the ready gate); the `Template`
  group in `catalog.py`, `Resources/tools.json` and the README tool tables
  now list eleven `template_*` tools (**126 tools / 123 commands**).

### Changed

- `docs/distill.md` and `skills/template-distill/SKILL.md` gained the delete
  section; the four-language READMEs, `docs/使用指南.md`, `RELEASE-NOTES.md` and
  `VERSION.json` were bumped to 1.1.13 (Config.daml / csproj / pyproject /
  `arcgis_agentkit` module version included).

## 1.1.12 — 2026-11-19

### Added

- **Template distillation, ported from 2.x (v2.0.2).** The customer line now
  ships the `arcgis_agentkit.distill` Python package (Pillow only, no other
  dependency) and ten `template_*` MCP tools: `template_list`, `template_get`,
  `template_import`, `template_measure`, `template_patch`, `template_validate`,
  `template_review`, `template_approve`, `template_preview`, `template_apply`.
  The tools are thin C# wrappers over the `arcgis-distill` CLI
  (`Commands/TemplateCommands.cs`), so the CLI and the MCP surface share one
  implementation and cannot drift.
- **Runtime `distill/` component and seeded `library/`.** `install/runtime`
  gains `distill/` (the `arcgis_agentkit` package plus `run-distill.ps1`,
  which runs the CLI on ArcGIS Pro's own `arcgispro-py3` Python) and a seeded
  empty `library/` (schema + index). The template library lives at
  `%LOCALAPPDATA%\ArcGIS Pro MCP\library`.
- `skills/template-distill/SKILL.md` (the semantic-patch work guide) and
  `docs/distill.md` (state machine, commands, library layout) travel with the
  source tree; `tests/test_distill.py` covers the full pipeline
  (import → measure → patch → validate → preview → review → approve → apply).
- `scripts/gen_tool_docs.py` knows the new `Template` group; the README tool
  tables and `Resources/tools.json` are regenerated from `catalog.py`
  (115 → 125 tools, 112 → 122 commands).

### Changed

- `install/install.ps1` copies the new `distill` and `library` directories,
  verifies the launcher and module files, and **preserves an existing template
  library across upgrades** (user templates win over the empty seed).
- `install/check.ps1` reports the distill launcher and module; the 2.x dialog
  guard is deliberately not included (1.1.x keeps its standalone
  `03-dialog-guard.cmd` script).
- `pyproject.toml`: version 1.1.12, `Pillow>=9` dependency, the
  `arcgis-distill` entry point, and `src/arcgis_agentkit` in the wheel.

## 1.1.11 — 2026-11-19

### Fixed

- **`create_feature_class` / `create_table` failure cleanup no longer guesses
  by name.** When field verification fails after creation, the map layers and
  standalone tables that referenced the new dataset are resolved by their data
  source *before* the dataset is deleted, then removed by object identity — an
  unrelated layer that merely shares the name is left alone.
- **Cleanup failures are reported, not swallowed.** A failed dataset delete or
  layer removal is appended to the tool error (`Cleanup problems: …`) so the
  caller can see which step failed.

### Changed

- `scripts/update_installed.py` no longer hardcodes this machine's paths: the
  add-in path defaults to the user's Documents AddIns folder and the package
  root to the script's own tree; both remain overridable via `MCP_*` env vars.
- The build maps the project directory to `/_/src` in compiler debug info, so
  the shipped DLL/PDB no longer carry the build machine's source paths.

## 1.1.10 — 2026-11-18

### Fixed

- **restart_pro can no longer wedge.** The save-prompt probe (UI automation)
  runs in a child PowerShell process and is abandoned after 15 s, so a busy
  ArcGIS Pro cannot block the helper indefinitely. The restart log is UTF-8
  and carries phase heartbeats (close progress, bridge wait), and the probe
  answers localized `否(N)` / `取消(C)` buttons, not just the `(&N)` form.
- **A dead restart helper no longer blocks retries.** The scheduled guard now
  expires after 10 minutes instead of rejecting every later restart_pro call
  until ArcGIS Pro is restarted by hand.

### Changed

- Local upgrade bookkeeping is one parameterised `scripts/update_installed.py`
  (positional: version, expected SHA-256, note) instead of a one-off script
  per release.
- `scripts/restart_pro.ps1` shares the bounded save-prompt probe and matches
  localized button labels too.

## 1.1.9 — 2026-11-18

### Fixed

- **Layer commands accept a dataset path.** get_layer_info, set_raster_symbology
  and repair_layer_source resolve a full dataset path to the loaded layer that
  points at it; several matches are refused with their names (I1/R1).
- **repair_layer_source repoints raster layers** through a RasterDataset, not
  just feature layers and tables (D4).
- **search_data no longer lists a dataset twice.** The default roots can name
  one geodatabase both directly and through its parent folder; each workspace
  is searched once.
- **check_extension says what it is.** It now returns
  `status:"not_inspectable", inspectable:false` instead of a bare "unknown"
  that read like a licence state.
- **GP wrapper outputs drop empty strings**, so zonal_statistics no longer
  returns a trailing "" in outputs.
- **get_features explains a raster name**: the "not found" error now says the
  name is a raster layer and points at get_raster_info / sample_raster_values
  instead of listing it among the candidates.

### Notes

- Python-toolbox cold start measures about 0.7 s once per Pro process (warm
  ~25 ms). No prewarm was added: it moves that cost rather than removing it,
  and only pays off when a batch is already known to follow.

## 1.1.8 — 2026-11-18

### Fixed

- **insert_features / update_features / delete_features work on standalone
  tables and dataset paths.** EditOperation is layer-shaped, so table rows are
  written through the Table API instead: insert/update/delete now accept a
  table in the map or a full path exactly like the read commands do. Table
  writes are immediate and say so in the result (D2, tasks test 2026-11-18).
- **sample_raster_values samples pixels directly.** `points` is finally
  accepted (`[{x,y}]` or `[[x,y]]`), point layers can be anywhere and are
  projected to the raster; values come back inline and nothing is written to
  the default geodatabase. The old sa.Sample table path stays behind
  `write_table=true` with a unique default table name (D3 + its overwrite
  side effect).
- **create_map makes a real scene.** A Scene was being created with
  MapViewingMode.Map, which Pro rejected with "3D 地图视图无法更改为地图";
  scenes now use MapViewingMode.SceneLocal (S1).
- **get_features reads joined fields.** Joins made through add_join are
  remembered; requested joined fields are resolved to their qualified
  "Table.Field" names, an unfiltered read includes them, and a missing field
  now fails with the full candidate list (J1/J2).
- **Layout elements report and verify page-space bounds.** x/y are the
  element's lower-left in page units (GetBounds), creation and
  set_layout_element read the real box back, a placement off by more than
  0.02 page units raises, and an element that runs past the page edge is
  refused (L1).

### Changed

- **run_batch reports per-step milliseconds** and states that steps run
  serially on Pro's main thread, so writes to one map or dataset cannot
  interleave.

## 1.1.7 — 2026-11-18

### Fixed

- **create_feature_class / create_table apply and verify their `fields`.**
  arcpy's CreateFeatureclass/CreateTable have no fields parameter, so the
  wrappers add each requested field with AddField and read the new dataset
  back (names, types, TEXT lengths) before reporting success. A mismatch or a
  failed AddField deletes the dataset this call created and raises; an invalid
  request is rejected before anything is created. (D1, tasks test 2026-11-18.)
- **run_batch no longer embeds images by default.** export_map_view,
  export_layout and preview_layout return the inline picture only outside a
  batch or when `return_image=true` is passed; inside run_batch they return
  the path, byte count and SHA-256. A 1.16 MB batch response becomes under a
  kilobyte. `Params.InBatch` carries the flag explicitly because an
  AsyncLocal set by run_batch did not reach handlers on the MCT dispatcher.
- **preview_layout can write its image out** with `output_path` and reports
  size/hash when it does not return the picture inline.
- **Exports wait briefly for the file to appear** before deciding whether to
  inline it, so a just-written PNG is not missed.

### Added

- **get_features pages and summarises geometry.** Results carry `truncated`
  and `next_offset`; `geometry_summary=true` returns type/extent/length/area
  instead of WKT; `geometry_precision=N` rounds WKT coordinates. The shape is
  loaded for a summary even though attributes alone would not need it.

## 1.1.6 — 2026-11-18

### Changed

- **get_pump_status answers when the Python bridge is down.** A refused
  connection now returns `running:false, bridge_listening:false` with a hint
  instead of failing, so the one query whose job is to report bridge state is
  readable precisely when the bridge is missing. Execution commands such as
  execute_arcpy_code still fail hard.
- **Bridge errors carry a code and a timestamp.** `last_error_code` /
  `last_error_at` in diagnose and `[code=...]` in the message
  (BRIDGE_NOT_LISTENING / BRIDGE_ERROR / BRIDGE_CLOSED / BRIDGE_TIMEOUT /
  UNSUPPORTED); status reads get a 5 s bound.
- **diagnose probes the bridge port** (300 ms) and reports
  `bridge_listening`, so "is the bridge up" is answerable without it.
- **Toolbox path errors suggest the intended path** ("Did you mean ...?") and
  the positional-parameters error now shows a ready-to-use example.
- **Tool schemas state the host difference**: disk toolboxes take positional
  values on the native add-in; named parameters are Python-bridge only, and
  bridge-backed tools say so in their descriptions.
- **HTTP transport test no longer asserts an exact tool count** -- uniqueness
  plus a required-tool subset instead; the exact count broke on every
  addition.

## 1.1.1 — 2026-09-07

### Fixed

- **Codex could not be registered on a machine that had not installed the
  Python relay.** The button resolved a launcher before it drew its
  confirmation dialog, so it raised *"it needs the Python relay — and
  arcgis-pro-mcp.exe is not installed"* and wrote nothing. Codex accepts a
  Streamable HTTP server as a bare `url` in `config.toml`, with no `transport`
  key, so it now connects directly to the add-in like every other HTTP client.
  An existing `command`/`args` registration is replaced in place, including its
  `env` subtable — a table holding both `command` and `url` is rejected by
  Codex outright.
- **Claude Desktop had the same dead end.** It can only launch a server, so the
  add-in now writes the missing piece itself: a PowerShell bridge under
  `%LOCALAPPDATA%\ArcGIS Pro MCP` that forwards to the same HTTP endpoint.
  Windows PowerShell ships with Windows, so no client needs Python, Node or pip
  any more.
- **Uninstalling failed with a `CommandNotFoundException`** after deleting the
  package. `Remove-EmptyAddInFolders` had lost its definition while both call
  sites remained, and the installer runs with `$ErrorActionPreference = 'Stop'`.
- **The installer refused to run on ArcGIS Pro newer than 3.7.** Older than 3.7
  is a real barrier — the references are 13.7 — but newer is not. Below 3.7 it
  refuses; above, it warns and installs.
- **Claude Desktop was reported as not installed, and would have been given a
  config file it never reads.** The Microsoft Store build is a packaged app, so
  what it sees as `%APPDATA%` is really its own
  `%LOCALAPPDATA%\Packages\Claude_*\LocalCache\Roaming`. The catalog now
  resolves whichever of the two layouts is on the machine.
- **Everything that named a port named the wrong one.** The Status button's
  caption, the toggle's tooltip and `ping`'s `port` field all reported 6510 —
  the legacy TCP bridge, which no AI client uses — while the address clients
  actually need, `http://127.0.0.1:6520/mcp`, appeared only on the second line
  of a dialog you had to click. An assistant asked where it was connected duly
  told its user 6510.
- **`ping` always reported `project_path: null`**, even with a project open,
  because it does not run on the Main CIM Thread and the field was hard-coded.
  A null there reads as *no project open*, so assistants told people to open a
  project they already had open. It now reads the project, and omits the key
  rather than inventing a null if that read is not possible.
- `-CheckOnly` reported a machine as *Ready* when nothing was installed.
- `README.md` claimed ArcGIS Pro 3.3+; the package has required 3.7 since the
  .NET 10 move.

### Changed

- Builds go to `artifacts/candidates/pro-<version>/` and do not deploy. `dist/`
  receives only a build that has been installed, loaded by ArcGIS Pro and
  checked over HTTP, and now carries the `build-report.json` that identifies it.
- `build.ps1` runs the transport tests on both frameworks and the installer
  tests against the package it just produced.
- The build refuses references that do not match the selected Pro version, and
  stamps `desktopVersion` from it.
- The Python package pins `mcp>=1.6.0,<2`.

## 1.1 — 2026-08-30

- Relicensed to AGPL-3.0-or-later, with a separate commercial licence and an
  additional permission for linking against the ArcGIS Pro SDK.
- Ribbon rebuilt: one start/stop toggle, a status button, one button per AI
  client showing whether it is already registered, and an Info button.
- Single-file installer: a `.cmd`/PowerShell polyglot with the add-in embedded,
  so there is nothing else to download and no execution-policy prompt.
- ArcGIS Pro 3.7 and .NET 10.

## 1.0 — 2026-08-29

First public release. Withdrawn and retagged: it went out under MIT, which was
not the intended licence.
