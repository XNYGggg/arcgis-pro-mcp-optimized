# ArcGIS Pro MCP 本地升级 1.1.13（模板库删除命令 template_delete · 仅本机）

- 日期：2026-11-19
- 基线：1.1.12（基线提交 `7a20f9e`，分支 `customer-v1.1.11-distill`；add-in SHA256 `A20E263B041E78A1F5631B1DD4EFC1B7946C05A28038751DC20C38F0AB55C6C7`，476,195 字节，122 命令 / 125 工具）
- 目标：为 `arcgis-distill` 模板库补齐删除命令（1.1.12 验收清理模板只能手工改 index/文件），新增 MCP 工具 `template_delete`。
- 分支：`customer-v1.1.13`（独立分支；发布时推送远端并打 tag `v1.1.13`）。

## 变更清单

| 项 | 变更 | 主要文件 |
|---|---|---|
| 1 | **Python 库**：`TemplateLibrary.delete(template_id, force=False)`——加锁执行；删除 spec + index 条目 + 作业单 + 无引用的去重图片；图片被其他模板引用（index 条目与 specs 目录双重扫描）时保留并回报引用者；删除不存在 id 抛 `FileNotFoundError`（CLI → `not_found`/exit 3）；`ready` 模板无 force 抛 `ValueError`（库侧兜底） | source/src/arcgis_agentkit/distill/library.py |
| 2 | **CLI**：`arcgis-distill delete <id> [--force]`；`--json` 信封；`ready` 无 force → `ready_template`/exit 5；`inbox` 不受影响 | source/src/arcgis_agentkit/distill/cli.py |
| 3 | **MCP 面**：catalog Template 组新增 `template_delete`（destructive=True）；`tools.json` 重新生成（125→126 工具）；C# 薄封装 `template_delete`（TemplateCommands.cs 注册 + 处理器） | source/src/arcgis_pro_mcp/catalog.py、source/addin/ArcGISProMCP/Commands/TemplateCommands.cs、Resources/tools.json |
| 4 | **测试**：`tests/test_distill.py` 新增 4 用例（无共享图片删除、共享图片保留、未知 id `not_found` + 重复删除、ready 需 force）→ 11/11；`test_catalog_matches_bridge.py` 的 CSHARP_ONLY 增补 `template_delete`（4/4） | source/tests/** |
| 5 | **文档**：`docs/distill.md` 与 `skills/template-distill/SKILL.md` 增加删除一节；四语 README 工具表/能力对照/版本行与 1.1.13 历史行；`docs/使用指南.md`、`docs/CHANGELOG.md`、`source/CHANGELOG.md`、`RELEASE-NOTES.md` 同步 | 见仓库 |
| 6 | **版本**：Config.daml / csproj / pyproject / `arcgis_agentkit`（MODULE_VERSION、`__version__`）统一 1.1.13；VERSION.json 126 工具 / 123 命令；install.ps1 / check.ps1 版本口径同步 | 见仓库 |
| 7 | **运行时同步**：`install/runtime/distill/arcgis_agentkit` 由 source 重新复制，逐文件一致（12 文件） | install/runtime/distill/arcgis_agentkit/** |

## 删除语义（设计要点）

- **对象**：spec（`specs/<id>.json`）、index 条目、作业单（`%LOCALAPPDATA%\ArcGIS-AgentKit\jobs\<id>.json`）、去重样本图（`images/<sha16>.<ext>`）。
- **图片共享保护**：`import` 按 sha256 去重、`bundle-import` 会复用同名图片文件，因此删除前扫描剩余 index 条目与 specs 目录；仍有引用则保留图片，返回 `removed.image_kept_for`。
- **幂等**：删除不存在的 id → `not_found`（退出码 3），重复删除安全，索引不会损坏。
- **ready 门槛**：`ready` 是评审签署过的状态，删除需 `--force`（否则 exit 5 拒绝）；CLI 在库调用前检查，库内再兜底。
- **锁**：整个删除在 `_lock()` 内完成（index 读-改-写 + 文件删除），与 `_upsert` 使用同一锁文件，不与并发导入互相踩踏。

## 构建（最终构建）

- 环境：dotnet SDK 10.0.300；ArcGIS Pro 3.7.0.1901（net10.0-windows）；Pro 自带 Python（pytest 8.4.2）。
- `scripts/build.ps1` 全过（installer 脚本检查、client-registration、HTTP transport net8.0+net10.0、单文件安装器检查）。
- **ArcGISProMCP.esriAddinX**：476,408 字节，SHA256 `4B90BB43F0BAD77DE817F19F0C342603CE9B2603C36E285066CF623B643D7419`，未签名。
- 自安装包 `Install-ArcGISProMCP.cmd`：659,248 字节，SHA256 `B1E4922F3D97F31164990420CCD829BA3293AB37B361BAE1BFEC432D9133A07F`，内嵌载荷回算一致。

## 单元测试

- `tests/test_distill.py`：**11/11**（原 7 + 新 4）。
- `tests/test_catalog_matches_bridge.py`：**4/4**（tools.json ↔ catalog 126 工具全等价；template_* 记为 C# 原生实现，CSHARP_ONLY 含 `template_delete`）。

## 打包与校验

- `tools.json` 由 `export_tool_schemas.py` 重新生成（126 工具、Template 组 11 个）；`source/README.md` 工具表由 `gen_tool_docs.py` 重生成。
- `SHA256SUMS.txt` 重新生成（232 个文件）并全量校验通过。
- `install/runtime/distill` 与 `source/src/arcgis_agentkit` 自 source 重新复制后逐文件一致；`docs/distill.md` 与 `source/docs/distill.md`、`skills/` 与 `source/skills/` 副本一致。

## 未做 / 边界

- 未做实机安装与 MCP 握手验收（本任务只要求构建 + 测试 + 包级校验）；`install.ps1` 升级保留旧模板库的既有行为不变，删除命令不影响既有模板。
- 未创建 Release、未打 tag、未推送远端。
