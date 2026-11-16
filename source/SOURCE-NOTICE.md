# 源码快照说明（source/）

本目录是客户版二进制对应的完整源码快照，用于满足 AGPL-3.0 的源码提供义务，也便于审计与二次开发。

## 来源与修改

- 上游项目：**Knight60/ArcGIS-Pro-MCP**（作者 Pisut Nakmuenwai），许可证 **AGPL-3.0-or-later**。
- 本快照基于上游 1.1.1 源码，并包含本地累计补丁至 **1.1.4**：
  - `activate_map` 死锁修复；
  - 三车道 `JobScheduler`（查询/图层/地理处理）与 `get_queue_status`；
  - 其他运行时修复（详见 `CHANGELOG.md`）。
- add-in 二进制（`../install/ArcGISProMCP.esriAddinX`）由本目录源码构建（Pro 3.7 / .NET 10，`desktopVersion=3.7`）。

## 目录

- `addin/` —— C# add-in 源码
- `src/` —— Python stdio MCP 服务
- `arcgis_pro_plugin/` —— Pro 内 Python 桥
- `dist/`、`docs/`、`scripts/`、`tests/` —— 上游构建产物/文档/脚本/测试
- `LICENSE`、`NOTICE` —— 上游许可证与署名（必须保留）

## 许可

本快照与其衍生作品按 **AGPL-3.0-or-later** 分发。任何修改与再分发须遵守该许可证，并保留上游署名与修改声明。
