# ArcGIS Pro AgentKit

**One repository, three parts, one goal: let AI assistants drive a live ArcGIS Pro session safely and repeatably.**

| Part | What it is | Status |
|---|---|---|
| **MCP** | ArcGIS Pro MCP add-in + Python bridges (fork of [Knight60/ArcGIS-Pro-MCP], AGPL-3.0). 113 native tools, every geoprocessing tool, optional arcpy. | Customer-ready build published on the [`customer-v1.1.4`](../../tree/customer-v1.1.4) branch |
| **Guard** | On-demand dialog guard. Nothing runs in the background; the guard is summoned only when an operation may raise ArcGIS Pro's "third-party code" trust dialog, then exits. | Skeleton (design ready) |
| **Distill** | On-demand template distillation. Measure a sample figure, guide an assistant to write a layout spec, validate, preview, review — one job, no daemon. | Skeleton (design ready) |

This `main` branch currently holds the **project skeleton** (v0.1.0). The full design is in
[docs/design-2026-11-16.md](docs/design-2026-11-16.md). The ready-to-use customer package lives on the
[`customer-v1.1.4`](../../tree/customer-v1.1.4) branch.

## Repository layout

```
src/arcgis_agentkit/    guard/ and distill/ modules (Python; CLI entry points)
skills/                 assistant-facing skills (template-distill, arcgis-pro-mapping)
library/                template library: schema.json, index.json, examples/
docs/                   design and implementation notes
tests/                  pytest smoke tests
```

## Install (development)

```powershell
python -m pip install -e .
arcgis-guard doctor
arcgis-distill doctor
```

## Licensing

AGPL-3.0-or-later. The MCP part derives from [Knight60/ArcGIS-Pro-MCP] (AGPL-3.0-or-later);
see [NOTICE](NOTICE). Not affiliated with Esri. "ArcGIS" is a trademark of Esri.

[Knight60/ArcGIS-Pro-MCP]: https://github.com/Knight60/ArcGIS-Pro-MCP
