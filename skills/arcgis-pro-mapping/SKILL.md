---
name: arcgis-pro-mapping
description: Drive a live ArcGIS Pro session to produce finished maps (planned; skeleton only).
---

# ArcGIS Pro Mapping（规划中）

本技能将指导助手通过本项目的 MCP 模块驱动 ArcGIS Pro 制图：加数据、配符号与标注、套用布局模板、批量导出。

流程要点（详见 `docs/design-2026-11-16.md` 与客户版 `docs/使用指南.md`）：

1. 通过 MCP（HTTP `127.0.0.1:6520/mcp`）连接本机 ArcGIS Pro；
2. 原生工具优先，其次 GP 工具，最后用 RunCode / arcpy 批量；
3. 需要时以 `arcgis-distill apply` 套用已蒸馏的版式模板；
4. 出图前自检，导出后人工验收。
