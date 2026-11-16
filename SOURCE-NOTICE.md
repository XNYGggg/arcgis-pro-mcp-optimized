# SOURCE-NOTICE · 客户版 2.0.0 源码快照

本目录是与客户包 `ArcGIS-Pro-MCP-v2.0.0.zip` 中二进制对应的完整源码快照，
依据 AGPL-3.0-or-later 提供。

## 上游

- 项目：Knight60/ArcGIS-Pro-MCP
- 作者：Pisut Nakmuenwai
- 许可证：AGPL-3.0-or-later（见 `LICENSE` 与 `NOTICE`）
- 基线：1.1.1；本快照包含本地补丁 1.1.2–1.1.4 与 2.0.0。

## 2.0.0 修改摘要

1. **版本归一**：`addin/ArcGISProMCP/Config.daml`、`ArcGISProMCP.csproj`、
   `src/arcgis_pro_mcp/__init__.py`、根 `pyproject.toml` 统一为 2.0.0。
2. **守卫（新增）**：`src/arcgis_agentkit/guard/`（`guard.ps1` 引擎、规则 schema、
   Python 客户端与 CLI）；add-in 新增 `Bridge/GuardClient.cs`、
   `Commands/GuardCommands.cs`，并在 `run_python_toolbox_tool` 与
   `.pyt/.tbx/.atbx` GP 调用前后自动 arm/settle。
3. **蒸馏（新增）**：`src/arcgis_agentkit/distill/` 全流水线与 schema v1.1；
   add-in 新增 `Commands/TemplateCommands.cs`（对 `arcgis-distill` CLI 的薄封装）。
4. **工具目录**：`src/arcgis_pro_mcp/catalog.py` 增至 125 个工具；
   `addin/ArcGISProMCP/Resources/tools.json` 由该目录生成。
5. **文档与测试**：`docs/guard.md`、`docs/distill.md`、迁移说明、README 双语、
   pytest 套件（守卫握手/规则、蒸馏全流水线与负向用例）。
6. **打包**：客户版安装器新增守卫引擎与 `runtime/distill` 部署；客户包根
   LICENSE/NOTICE 与 SHA256SUMS。

## 构建对应关系

- add-in 2.0.0 由 `scripts/build.ps1` 在本机 ArcGIS Pro 3.7 / .NET 10 环境构建；
  FileVersion 2.0.0.0，SHA256 见客户包 `VERSION.json`。
- Python 包 `arcgis-pro-agentkit` 2.0.0（含 `arcgis_pro_mcp` 与 `arcgis_agentkit`）。
- 上游许可证与署名见本目录 `LICENSE`、`NOTICE`、`docs/upstream-CHANGELOG.md`。
