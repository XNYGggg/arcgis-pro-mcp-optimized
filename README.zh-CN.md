# ArcGIS Pro AgentKit · ArcGIS Pro 助手工具箱

**一个仓库，三个模块，一个目标：让 AI 助手安全、可复现地驱动正在运行的 ArcGIS Pro。**

| 模块 | 内容 | 状态 |
|---|---|---|
| **MCP** | ArcGIS Pro MCP add-in + Python 桥（衍生自 [Knight60/ArcGIS-Pro-MCP]，AGPL-3.0）。113 个原生工具、全部地理处理工具、可选 arcpy。 | 客户版已发布在 [`customer-v1.1.4`](../../tree/customer-v1.1.4) 分支 |
| **Guard** | 按需弹窗守卫。平时零进程；只在可能触发“第三方代码库”信任框的操作前被召唤，操作结束即退出。 | 骨架（设计已定稿） |
| **Distill** | 按需模板蒸馏。测量样本图 → 助手按规范写版式 spec → 校验 → 预览 → 评审，一次作业，无守护进程。 | 骨架（设计已定稿） |

本 `main` 分支当前是**项目骨架**（v0.1.0）。完整设计见
[docs/design-2026-11-16.md](docs/design-2026-11-16.md)；开箱即用的客户版在
[`customer-v1.1.4`](../../tree/customer-v1.1.4) 分支。

## 目录

```
src/arcgis_agentkit/    guard / distill 模块（Python，CLI 入口）
skills/                 给助手的技能说明（template-distill、arcgis-pro-mapping）
library/                模板库：schema.json、index.json、examples/
docs/                   设计与实施文档
tests/                  pytest 冒烟测试
```

## 开发安装

```powershell
python -m pip install -e .
arcgis-guard doctor
arcgis-distill doctor
```

## 许可

AGPL-3.0-or-later。MCP 部分衍生自 [Knight60/ArcGIS-Pro-MCP]（AGPL-3.0-or-later），
署名见 [NOTICE](NOTICE)。本项目与 Esri 无隶属关系；“ArcGIS”是 Esri 的商标。

[Knight60/ArcGIS-Pro-MCP]: https://github.com/Knight60/ArcGIS-Pro-MCP
