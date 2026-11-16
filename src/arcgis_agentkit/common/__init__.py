"""Shared helpers: config, JSONL logging, exit codes, doctor base (skeleton).

Planned per docs/design-2026-11-16.md §2.5:
- state dir: %LOCALAPPDATA%\\ArcGIS-AgentKit (ARCGIS_AGENTKIT_HOME overrides)
- JSON output envelope: {"ok": true, ...} / {"ok": false, "code": ..., "message": ..., "hint": ...}
- exit codes: 0 ok, 2 usage, 3 environment, 4 runtime, 5 validation
"""
