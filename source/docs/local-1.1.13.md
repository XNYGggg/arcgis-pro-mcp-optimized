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
| 4 | **测试**：`tests/test_distill.py` 新增 7 用例（无共享图片删除、共享图片保留、未知 id `not_found` + 重复删除、ready 需 force、路径型 id 拒绝、孤儿索引清理、超长 id 撞名后可删除）→ 14/14；`test_catalog_matches_bridge.py` 的 CSHARP_ONLY 增补 `template_delete`（4/4） | source/tests/** |
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
- **ArcGISProMCP.esriAddinX**：432,750 字节，SHA256 `CDBAEE33A5360518E29A0DC3EF2174ADB8D983D44A15DFA563663CAC21EB327B`，未签名。
- 自安装包 `Install-ArcGISProMCP.cmd`：600,551 字节，SHA256 `9646CEF6FA1B49862B5D6D3575408B1360F378FCF3DDE3F3EBD33D1B8E523BD8`，内嵌载荷回算一致。

## 单元测试

- `tests/test_distill.py`：**14/14**（原 7 + 新 7）。
- `tests/test_catalog_matches_bridge.py`：**4/4**（tools.json ↔ catalog 126 工具全等价；template_* 记为 C# 原生实现，CSHARP_ONLY 含 `template_delete`）。

## 打包与校验

- `tools.json` 由 `export_tool_schemas.py` 重新生成（126 工具、Template 组 11 个）；`source/README.md` 工具表由 `gen_tool_docs.py` 重生成。
- `SHA256SUMS.txt` 重新生成（232 个文件）并全量校验通过。
- `install/runtime/distill` 与 `source/src/arcgis_agentkit` 自 source 重新复制后逐文件一致；`docs/distill.md` 与 `source/docs/distill.md`、`skills/` 与 `source/skills/` 副本一致。

## 实机部署与验收（2026-11-19，真实终端）

- 安装：真实终端运行 `install/install.ps1 -SkipClients`（经 ASCII 路径中转脚本调用）——add-in 与运行时均更新，旧 add-in 备份 `ArcGISProMCP.esriAddinX.bak-20261119-213144`、旧运行时备份 `ArcGIS Pro MCP.bak-20261119-213144`，**旧模板库保留**；installed.json 记录 add-in SHA256 `CDBAEE33…327B`。
- 重启与握手：`restart_pro` 保存并重开 MyProject_95.aprx；MCP initialize `serverInfo.version = 1.1.13.0`；`ping` → `command_count = 123`；`tools/list` → **126 工具**，Template 组 11 个（含 `template_delete`）。
- 蒸馏全链路（经已安装启动器 `%LOCALAPPDATA%\ArcGIS Pro MCP\distill\run-distill.ps1`，临时库 E:\mcp113\lib）：doctor → `{"version":"1.1.13","status":"ready"}`；import → measure → delete 全部 exit 0，delete 返回 `removed{index_entry,spec,image,job}` 且 `errors: []`；list 计数归 0；**重复删除 → `not_found`/exit 3**。
- 官方自检 `install/check.ps1`：**22 通过 / 2 警告（6520 端口 http.sys 归属显示）/ 0 失败**；新增的版本门槛与 `tools/list` 探测均生效（首轮发现探测代码用 Invoke-RestMethod 解析对象导致误报，已改为 Invoke-WebRequest 原文匹配后复检通过）。
- 回滚：add-in 备份见上；运行时备份见上；两者任一恢复后重启 Pro 即回到 1.1.12。

## 未做 / 边界

- 未在 3.7 以外的 Pro 版本验证；未创建 Gitee 发行版（本轮只发 GitHub）。
