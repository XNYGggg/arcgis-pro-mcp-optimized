# ArcGIS Pro MCP 本地升级 1.1.12（合入 2.x 模板蒸馏模块 · 仅本机）

- 日期：2026-11-19
- 基线：1.1.11（add-in SHA256 `0ABB872FBCD6FB65AA12F118501216F2BD3F305A9C6C68537870A5EDDF306DF9`，469,634 字节，112 命令 / 115 工具）
- 来源：`release/v2.0.2-opt`（v2.0.2-dev @ 0aa42aa 及其后仅 README 的提交；蒸馏模块与 2026-11-18 最新 v2 完全一致，已用 `git diff` 核实无差异）
- 目标：把 2.x 的"模板蒸馏"模块（`arcgis_agentkit.distill` + 10 个 `template_*` 工具 + 运行时 `distill/` + 模板库）整体移植进客户 1.1.x 线，覆盖 1.1.11"本版不含"的状态。

## 变更清单

| 项 | 变更 | 主要文件 |
|---|---|---|
| 1 | **Python 包移植**：`arcgis_agentkit`（`common/` + `distill/`，不含 2.x 弹窗守护 guard）；仅依赖 Pillow（Pro 自带环境即有）；`MODULE_VERSION`/`__version__` 统一为 1.1.12 | source/src/arcgis_agentkit/** |
| 2 | **MCP 工具面**：catalog.py 新增 Template 组 10 个工具（list/get/import/measure/patch/validate/review/approve/preview/apply）；`tools.json` 由 `export_tool_schemas.py` 重新生成（115→125 工具） | source/src/arcgis_pro_mcp/catalog.py、source/addin/ArcGISProMCP/Resources/tools.json |
| 3 | **C# 工具实现**：`TemplateCommands.cs` 作为 `arcgis-distill` CLI 的薄封装（自 2.x 逐字移植，仅把 `GuardClient.Quote/PowerShelPath` 内联为本文件私有助手，避免引入 guard 依赖）；`CommandRegistry` 注册 | source/addin/ArcGISProMCP/Commands/TemplateCommands.cs、CommandRegistry.cs |
| 4 | **运行时**：`install/runtime/distill/`（`arcgis_agentkit` + `run-distill.ps1`，用 Pro 自带 Python 运行）；`install/runtime/library/` 空库种子（schema+index）；库实际位置 `%LOCALAPPDATA%\ArcGIS Pro MCP\library` | install/runtime/distill/**、install/runtime/library/** |
| 5 | **安装器**：复制清单新增 `distill`/`library`；必需文件校验新增启动器与 CLI；**升级保留旧模板库**（用户已蒸馏模板不丢）；版本标记 1.1.12 | install/install.ps1、install/check.ps1 |
| 6 | **测试与文档**：`tests/conftest.py`、`tests/test_distill.py`（2.x 原样）；`test_catalog_matches_bridge.py` 的 CSHARP_ONLY 增补 10 个 template 工具；`skills/template-distill/SKILL.md`、`docs/distill.md`；`gen_tool_docs.py` 增加 Template 组标题 | source/tests/**、source/skills/**、source/docs/distill.md、source/scripts/gen_tool_docs.py |
| 7 | **版本与依赖**：Config.daml / csproj / pyproject 统一 1.1.12；pyproject 依赖新增 `Pillow>=9`、入口点 `arcgis-distill`、wheel 包列表加 `src/arcgis_agentkit` | Config.daml、ArcGISProMCP.csproj、pyproject.toml |
| 8 | **包级文档口径**：四语 README 模板蒸馏从"❌ 本版不含"改为"✅ 已包含（1.1.12）"，工具目录补 Template 组（125 工具/13 组）；RELEASE-NOTES / docs/CHANGELOG / docs/使用指南 / VERSION.json 同步 | README×4、RELEASE-NOTES.md、docs/CHANGELOG.md、docs/使用指南.md、VERSION.json |

## 构建（最终构建）

- 环境：dotnet SDK 10.0.300；ArcGIS Pro 3.7.0.1901（net10.0-windows）；Python 3.13.9（仅用于生成 tools.json/README 表格与测试）。
- `scripts/build.ps1` 全过（installer 脚本检查、client-registration、HTTP transport net8.0+net10.0、单文件安装器检查）。
- **ArcGISProMCP.esriAddinX**：476,195 字节，SHA256 `A20E263B041E78A1F5631B1DD4EFC1B7946C05A28038751DC20C38F0AB55C6C7`，未签名。
- 自安装包 `Install-ArcGISProMCP.cmd`：658,962 字节，SHA256 `F1ACCDCABA406617816B7318BEBE33732968D3065332BB5FA451791A34657BB9`，内嵌载荷回算一致。
- 运行时装配：`install/runtime/distill/`（14 个文件）+ `library/`（2 个种子文件）纳入安装清单；`install.ps1` / `check.ps1` / `run-distill.ps1` 均通过 PowerShell 语法解析。

## 单元测试（构建与移植验证）

- `tests/test_distill.py`：7/7 通过（新增非图片拒绝、force 批准评审完整性）（import 去重、measure 确定性、patch 保护字段拒绝、validate/approve 门槛、preview 渲染、apply 输出）。
- `tests/test_catalog_matches_bridge.py`：4/4 通过（新增 tools.json↔catalog 全等价）（125 工具均有实现；template_* 记为 C# 原生实现）。
- `tools.json` 由 catalog.py 生成后断言 125 个工具、Template 组 10 个。

## 批量蒸馏验证（2026-11-19，50 张专家级地图图件）

- 语料：Wikimedia Commons 抓取 50 张专家级地图图件，10 类 × 5（地形晕渲/人口/气候/地质/土地覆盖/历史地图/海洋测深/水系流域/城市规划/专题图集），许可记于 `release/distill-verify-20261119/manifest.json`。
- 运行环境：ArcGIS Pro 自带 Python 3.13.13 / Pillow 12.2.0（与客户版 `run-distill.ps1` 同路径）；独立测试库，未触碰用户库。
- 结果：import 50/50；measure 49/50（唯一失败为图集封面页，工具确定性拒绝 `invalid_input`，语义正确）；patch/validate/preview 49/49；measure 0.11–0.17 s/张；全流程 20.2 s。
- 确定性：10 张重测关键字段 10/10 一致；去重：重复导入 5/5 同 id、库增长 0；`-m` 子进程入口冒烟 3/3。
- 负向：缺失 id → `not_found`/exit 3；patch 保护字段 → exit 2；approve 缺评审拒绝、`--force` 记录；import 非图片在**入库前**被拒（PIL 校验，`invalid_input`，退出码 4；修正轮落实）。
- 证据：`release/distill-verify-20261119/`（batch-report.json、patches/、previews/、蒸馏验证报告.md、抓取与批量脚本）。

## 三方会谈（评审，第 1 轮）

- 评审方式：Codex（codex exec，high）+ DeepSeek Harness（dsh headless，系统环境变量 `Claude调用harness` 密钥直连），并发独立审阅移植与验证证据。
- 第 1 轮结论（双方一致）：**需修正后通过**。收敛后的修正点与落实：

| # | 评审指出的问题 | 修正 |
|---|---|---|
| 1 | 安装器先换 add-in 再暂存运行时；后段失败会留下「新 add-in + 旧运行时」 | install.ps1 重排：先暂存运行时并通过完整性校验、先保留旧库，再原子替换 add-in，最后提交运行时；运行时提交失败连带回滚 add-in；目录移动改用 `[IO.Directory]::Move`（目标已存在即失败，失败清理后还原） |
| 2 | run-distill.ps1 只查 Program Files，非默认安装的 Pro 找不到 Python | 启动器优先读注册表 `HKLM\SOFTWARE\ESRI\ArcGISPro InstallDir`，再回退 Program Files 候选 |
| 3 | `approve --force` 把模板置 ready 但评审记录缺 by/date，与 validate/apply 判定不一致 | force 批准时补写完整评审（`by=force`、`date=当天`，已有字段保留）；新增行为测试锁定 |
| 4 | `import` 接受非图片文件，仅入元数据 | 入库前 PIL 打开校验，不可读图片直接 `invalid_input`（退出码 4），不再产生垃圾记录；新增测试锁定 |
| 5 | check.ps1 只查文件存在，未实际探测蒸馏组件 | 新增「模板蒸馏组件」实探：Pro 自带 Python 导入 Pillow + `run-distill.ps1 doctor` |
| 6 | tools.json 过期会静默漏工具，测试把 template_* 记入 CSHARP_ONLY 后不再校验 | 新增 `tools.json ↔ catalog.py` 全等价测试；`tests` 现共 11 项全过 |
| 7 | 负向用例污染语料库（`not-an-image` 与 force 批准的 #01 留在库里） | 负向用例移入独立一次性库；语料库重跑后保持 50 模板纯净（49 validated + 1 imported 封面页，封面页 measure 被正确拒绝）；批量 20.0 s（第 1 轮实测；最终轮 20.2 s） |
| 8 | 发布口径偏差：四语 README 徽标/标题仍 1.1.11；45–50 张表述；dist 单文件安装器范围不明；distill.md 的 `inbox --list` 不存在；文档路径只在 source/；校验清单含自身 | README×4 徽标/标题/简介/上游补丁统一 1.1.12；改为「50 张实测（49 全绿，1 幅封面页被正确拒绝）」；RELEASE-NOTES 注明 `install/00-install.cmd` 为完整入口、`source/dist` 单文件只装 add-in；`docs/distill.md` 修正 `inbox [--process-new]`；包根补 `docs/distill.md` 与 `skills/template-distill/SKILL.md`；SHA256SUMS 生成排除自身 |

| 9 | 第 2 轮：安装器回滚仍有两条缺口（旧运行时移 bak 在 try 外；首次安装失败会留下新 add-in） | 提交阶段整体纳入 try：移 bak、提交运行时任一步失败都还原运行时；有 add-in 备份则还原，首次安装（无备份）则把新 add-in 移出为 `.failed-<时间戳>`，机器恢复原状；check.ps1 对 doctor 改为同时要求 exit=0 且 status=ready（doctor 的 ok 在库不可写时仍为 true） |
| 10 | 第 2 轮：确定性复测把 10 份模板重置为 measured，终库 39/10/1 与报告不符 | 复测后对 10 份重新 validate；批量脚本输出 `library_status`（终库 49 validated + 1 imported）与 `revalidated_after_determinism=10` |
| 11 | 第 2 轮：SHA256SUMS 生成早于最后文档修改（7 项不符）；ping/握手缺原始报文；tools.json 测试只比名称 | 清单最后重生成并全量校验；原始报文存 `report/evidence/`（initialize / ping / tools-list）；tools.json 测试升级为 name+description+inputSchema+returnsImage 全等价 |

| 12 | 第 3 轮：catch 无条件删除 `$runtimeDst`——若「旧运行时→bak」本身失败，会删掉仍在原位的旧运行时（含用户模板库） | 按移动结果决定清理：仅 `$movedToBak` 为真才清理半成品 dst 并还原 bak；首次移动失败时旧运行时原样保留；回滚各步独立 try，失败只追加 WARNING、不中断后续回滚 |
| 13 | 第 3 轮：doctor 的 JSON 解析异常走 Warn（整体仍可 exit 0）；复验只有汇总无逐条记录；文档时长与实测不符 | 解析异常改为 Fail；批量报告新增 `revalidation[]` 逐条记录（10/10 ok）；文档时长统一为 20.2 s |

| 14 | 第 4 轮：回滚中空 catch 吞异常、失败文案总是“已回滚”、未给备份路径；若上次回滚把库留在备份里，下次升级会装空库 | 回滚各步异常统一追加 WARNING，最终文案按有无 WARNING 区分“已回滚/回滚未完全成功”并打印 add-in 与运行时备份路径；升级时若现役库缺失，自动改用最近一次 `*.bak-*\library` 备份中的库并提示 |

## 实机部署与验收（2026-11-19，真实终端）

- **部署通道（重要事实）**：本轮发现 Claude Code 的 Bash 工具把 `%LOCALAPPDATA%` 做了沙箱虚拟化（`dangerouslyDisableSandbox` 也不绕过）：在 Bash 里跑的安装器与哈希核对全部落在虚拟视图，ArcGIS Pro 看不到。于是改用**用户真实终端**（Terminal 面板）执行：add-in 部分先前已在 Bash 中真实完成（Documents 不虚拟）；运行时由真实终端运行 `install/install.ps1 -SkipClients` 一次通过（暂存 → 完整性校验 → `[IO.Directory]::Move` 提交），`%LOCALAPPDATA%\\ArcGIS Pro MCP` 真实落盘，installed.json 写入（version=1.1.12、addin_sha256=A20E…C6C7）。
- 真实终端辅助脚本需 **UTF-8 BOM + ASCII 路径**（PS 5.1 读无 BOM 的 UTF-8 中文脚本会乱码；终端命令行不允许非 ASCII 字符）。
- 启动器验证（真实终端）：`run-distill.ps1 doctor` → `{"ok": true, "version": "1.1.12", "status": "ready"}`（library / schema / index / Pillow 12.2.0 / writable 全绿）；`list` → 空库 ok。
- 重启与握手：经 `restart_pro`（先保存 MyProject_95.aprx）→ 重开后 MCP initialize `serverInfo.version = 1.1.12.0`；`ping` → `command_count = 122`；`tools/list` → **125 工具，含 10 个 `template_*`**。
- **MCP 全流程实机验收**（真实终端、Pro 自带 Python 3.13.13 + Pillow 12.2.0）：template_import → measure（aspect 0.7、dpi 104）→ patch → validate（0 错 0 警）→ preview（PNG 1,500,605 B）→ review → approve（ready）→ apply（context 2,837 B）→ list（1）→ 清理（0）——**11/11 PASS**；验收模板随后从用户库删除，库恢复为空（0 条）。
- 官方自检 `install/check.ps1`：**21 通过 / 2 警告 / 0 失败**（警告为 6520 端口的 http.sys 归属显示；新增「模板蒸馏组件」实探全绿：Pro Python Pillow 12.2.0、distill doctor ok）。
- 取证说明：Bash 沙箱虚拟视图中残留的运行时/备份目录不在真实盘上；真实盘由本次部署首次建立 1.1.12 运行时。
- 回滚：add-in 旧版备份 `Documents\\ArcGIS\\AddIns\\ArcGISPro\\ArcGISProMCP.esriAddinX.bak-20261119-153526`（1.1.11，469,634 B）；真实盘此前并无该运行时目录（历史安装同样只落在沙箱视图），回退为 1.1.11 无需恢复运行时文件；如需完整运行时快照可恢复 `release/isolation-1.1.11-20261119/runtime/ArcGIS Pro MCP`。
