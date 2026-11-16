# Changelog

## 2.0.1 — 2026-11-16

Live-test fixes found by installing 2.0.0 and driving a real ArcGIS Pro session.

- **Guard engine**: a trust dialog's text appears in the UIA tree before its
  buttons do. The 2.0.0 engine alerted immediately when the first scan found
  no 是/允许 button, producing a false 30-second alarm even though the
  auto-click succeeded a second later. The engine now waits 2.5 s for the
  button tree before alerting (`dialog_button_wait` in the JSONL log). Verified
  against a fresh, untrusted `.pyt`: detection → click 是(Y) → resolved, no alert.
- **Distill launcher**: `run-distill.ps1` used `[Parameter(ValueFromRemainingArguments)]`,
  which made PowerShell bind CLI flags such as `--out` to the common parameter
  `-OutVariable`, breaking `template_preview`. The launcher is now a simple
  script using `$args`; `template_preview` returns an image again.
- **Upgrade safety**: the runtime installer replaces the whole
  `%LOCALAPPDATA%\ArcGIS Pro MCP` directory. It now merges the user's
  `library/` into the new runtime before the swap, so distilled templates
  survive an upgrade (verified by reinstalling 2.0.1: template list stayed 1).
- Installer scripts are now kept in `packaging/customer/install/` so the
  release source snapshot contains the code that ships in the zip.
- Version bump to 2.0.1 across add-in, Python packages, installer and NuGet
  workflow. v2.0.0 remains available as a historical Release.

## 2.0.0 — 2026-11-16

The unified AgentKit release: MCP + on-demand guard + on-demand distillation.

### MCP add-in (2.0.0)

- **125 tools** (113 upstream + `guard_summon` / `guard_status` + 10 `template_*`).
- Version normalized across `Config.daml`, `ArcGISProMCP.csproj`, the Python
  package and `pyproject.toml` (previously 1.1.4 vs 0.2.0 vs 1.1.1).
- `run_python_toolbox_tool` and `run_geoprocessing_tool` calls that target a
  `.pyt` / `.tbx` / `.atbx` now summon the dialog guard automatically and settle
  it when the call returns; the result carries `guard.ready` and a
  `guard_warning` when the guard was unavailable.
- New `Bridge/GuardClient.cs` implements the file-token protocol shared with the
  Python CLI.

### Guard 2.0 (M1)

- New engine `src/arcgis_agentkit/guard/engine/guard.ps1` (PowerShell 5.1):
  - summon / settle / result handshake in `sessions/<token>/`;
  - single engine, multiple tokens (requests directory + `Local\ArcGISProDialogWatch_v2`);
  - UIA `WindowOpened` event first, 400 ms fallback poll only while armed;
  - rules externalized to `guard.schema.json` (strong / weak / deny / button lists);
  - built-in alert window: topmost, top-right, >=30 s, beep every 10 s;
  - deny-list confirmations (overwrite/delete/remove/...) are alerted and never clicked;
  - legacy switches (`-DurationSeconds`, `-AlertOnly`, `-SelfTest`, `-NotifyTest`) remain accepted.
- New CLI: `arcgis-guard doctor|summon|wrap|status|test|self-test`; `--json` everywhere;
  exit codes 0/2/3/4/5; state under `%LOCALAPPDATA%\ArcGIS-AgentKit`.
- Old `watch_pro_dialog.ps1` kept as a compatibility shim that forwards to the new engine.

### Distill 2.0 (M2)

- New pipeline: `import -> measure -> patch -> validate -> preview -> review ->
  approve -> apply`, plus `list/get/export/bundle-import/inbox`.
- Deterministic measurement (Pillow only): figure bbox, aspect, strong frame
  lines with thin-border merging, palette top colours, DPI inference.
- Validation: schema (v1.1) + geometry (frame/regions inside canvas), 3 %
  measured-canvas tolerance, typography order, colour syntax, pitfall/review gates.
- Preview renders a wireframe next to the sample figure; `template_preview`
  returns the PNG as an MCP image.
- Template library is atomic and sha256-idempotent; ready templates cannot be
  silently modified; approve requires review unless `--force` (recorded).
- Schema v1.1 is additive: existing v1 specs remain readable.

### Packaging & tests

- Customer package branch `customer-v2.0.0` (add-in 2.0.0 + new guard engine +
  bundled Distill runtime + installers + AGPL source snapshot).
- Python tests: 12 passed (guard handshake/rules + full distill pipeline and
  negative cases). Upstream C#/HTTP/installer suites updated to 125 tools, all passed.

## 0.1.0 — 2026-11-16

- Project skeleton: `main` branch established.
- Three-module layout: `mcp` (customer edition on `customer-v1.1.4`), `guard`, `distill`.
- Design document: `docs/design-2026-11-16.md`.
- CLI skeletons: `arcgis-guard` / `arcgis-distill` with `doctor` and placeholder commands.
- Template library skeleton: `library/schema.json` (v1), `library/index.json`.
