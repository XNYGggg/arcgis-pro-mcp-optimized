# ArcGIS Pro MCP 本地升级 1.1.5（仅本机 · 未发布 GitHub）

- 日期：2026-11-17
- 基线：客户版 v1.1.4（add-in SHA256 `7e6ca097…86ad4`，110 命令 / 113 工具）
- 升级依据：三方会议 `20261117-161536-v115-upgrade-61f9adbe`（Codex、DeepSeek 评审 + 协调员裁决，材料 `release/v1.1.5-升级会谈材料-2026-11-17.md`，收敛稿 `release/v115-tmp/convergence.md`）
- 触发来源：2026-11-17 实验12（TIN→DEM→坡度坡向→重分级→分区统计）全流程实操痛点

## 变更清单（F1–F10）

| 项 | 变更 | 主要文件 |
|---|---|---|
| F1 | 环境注入贯通：`set_environment` 的键经映射表真正传给 GP（含 snapRaster），`run_geoprocessing_tool`/`run_python_toolbox_tool` 支持 per-call `environment` 覆盖；`get_environment` 返回 `_forwarded`/`_unsupported` | Commands/GeoprocessingCommands.cs、Commands/MapCommands.cs |
| F2 | `describe_geoprocessing_tool` 返回 datatype/枚举(default ValueList)/短默认值；新增资源 gp-parameter-details.json（schema_version/generated_at/arcgis_pro_version/arcpy_version），资源缺失自动降级 | Commands/GeoprocessingCommands.cs、Resources/gp-parameter-details.json、scripts/dump_gp_parameters.py |
| F3 | `create_map` 默认不再添加底图（`basemap:"none"`；`project_default` 恢复旧行为），新增 `activate`，`epsg` 真正生效 | Commands/MapCommands.cs、catalog.py |
| F4 | `zoom_to_layer`/`export_map_view?zoom_to_layer=`/`get_layer_symbology` 支持栅格与 TIN；组图层递归（深度上限 10）；失败回退并标记 | Commands/MapHelpers.cs、LayerCommands.cs、ViewCommands.cs、SymbologyCommands.cs |
| F5 | 新增 `add_table`（独立表入图；C# StandaloneTableFactory + Python 桥处理器；重名报冲突） | Commands/LayerCommands.cs、arcgis_pro_plugin/arcgis_mcp/h_layers.py、catalog.py |
| F6 | `export_map_view` 新增 `fit:"auto"`（按视图范围比例调整尺寸，±25% 内，响应 requested/actual/fit_applied）；默认 `preserve` 保持 1.1.4 行为 | Commands/ViewCommands.cs |
| F7 | `set_raster_symbology` 唯一值支持 `value_colors`/`value_labels` 显式配色与标签，未匹配值回传 `unmatched_values` | Commands/RasterCommands.cs、SymbologyCommands.cs |
| F8 | `create_map` 接受 `name`/`map_name` 双别名（两者冲突报错） | Commands/MapCommands.cs |
| F9 | `set_map_properties` 的 `epsg` 真正生效（Map.SetSpatialReference），返回 `unchanged_layers` 并明示“不重投影数据” | Commands/MapCommands.cs |
| F10 | 新增 `restart_pro` 命令（评审加固版）：先 `SaveEditsAsync`+`SaveAsync`（异常即中止、不关窗），再经 WMI `Win32_Process.Create` 脱离启动 PowerShell 助手（失败回退直接启动）；延时下限 8 秒（默认 10）保证 MCP 响应先送达；调度后拒绝第二个 restart；助手只对"保存/不保存/取消"三按钮齐全的对话框代答（其余弹窗只记日志不点），重启后校验 `command_count ≥ 112` 且 PID 已变化，写 `restart.ok` | Commands/SessionCommands.cs、Resources/restart_pro.ps1（内嵌）、catalog.py |

**不兼容变更**：`create_map` 缺省底图由 Pro 项目模板默认（通常 World Topographic）改为 `none`。需要旧行为时显式 `basemap:"project_default"`。

**保持兼容**：F3 除外的全部新增参数都有旧行为缺省；`export_map_view` 缺省 `fit:"preserve"`；工具 113 → 115（新增 `add_table`、`restart_pro`）。

## 构建

```powershell
dotnet build addin\ArcGISProMCP\ArcGISProMCP.csproj -c Release
# 部署（复制到 文档\ArcGIS\AddIns\ArcGISPro）：
dotnet build addin\ArcGISProMCP\ArcGISProMCP.csproj -c Release -p:DeployAddIn=true
```

- 构建时间：见文末记录
- ArcGISProMCP.esriAddinX SHA256：见文末记录
- 工具目录：catalog.py → scripts/export_tool_schemas.py → Resources/tools.json（115 工具）
- GP 元数据：scripts/dump_gp_parameters.py（Pro Python）→ gp-parameters.json（659 KB，4056 键）+ gp-parameter-details.json

## 部署与验证

1. 关闭 ArcGIS Pro（重启脚本 `scripts/restart_pro.ps1` 会代答保存提示并重启工程）。
2. `-p:DeployAddIn=true` 部署；校验 Documents 下 esriAddinX 的 FileVersion 为 1.1.5、SHA256 与记录一致。
3. 更新 `%LOCALAPPDATA%\ArcGIS Pro MCP\installed.json`（version=1.1.5、addin_sha256、installed_at、package_root=本目录）。
4. 重启 Pro 后：`ping` 返回 1.1.5、`get_capabilities` 112 命令、`tools/list` 115 工具；F1–F10 实机用例（见回归记录）。

## 用户端三项能力（F10 与配套脚本，2026-11-17 第 2 批）

面向客户/自动化，不依赖任何 Claude 侧权限：

1. **自重启 `restart_pro`（MCP 命令，三方评审加固版）**：参数 `save`（默认 true）、`delay_seconds`（默认 10，强制 ≥8）、`project`（默认当前工程）。执行顺序：UI 线程 `SaveEditsAsync`→`SaveAsync`（任一异常即中止，不关窗）→ 内嵌脚本写到 `%LOCALAPPDATA%\ArcGIS-MCP\runtime\restart_pro_mcp.ps1` → 经 WMI `Win32_Process.Create` 由 WmiPrvSE 拉起助手（脱离 Pro 的作业对象；失败回退直接启动）→ MCP 先回 `{"scheduled":true,...}`。助手：等待延时 → 按 PID `CloseMainWindow` → 仅当窗口内同时存在"保存/不保存/取消"三类按钮时才点"不保存"（中英文标签经 Unicode 转义；其他弹窗只记日志绝不点击）→ 注册表定位并带工程重启 → 轮询 6510，恢复后要求 `command_count ≥ 112` 且新 PID ≠ 旧 PID，通过才写 `restart.ok`（日志 `restart.log`）。调度后拒绝第二个 restart 请求；客户端断线到桥接恢复后重连。
2. **Claude Code 权限模板 `scripts/install_claude_permissions.ps1`（评审加固版）**：**默认只预览**，`-Apply` 才写；默认写项目级 `<项目>\.claude\settings.local.json`（`-Scope user` 才写全局）；`-ServerName` 可改前缀；从运行中桥接（127.0.0.1:6520）实时读工具名，桥不通回退 tools.json；**执行/破坏类 14 个工具不进白名单**（run_python_toolbox_tool、execute_arcpy_code、restart_pro、run_geoprocessing_tool、delete_*、remove_*、truncate_table 等，保留逐次确认）；规则无变化直接 no-op；`-Remove` 只撤销本脚本会生成的同名规则，不碰用户自定义规则；写前时间戳备份。Claude Desktop 侧用其自身的工具开关，不需要此脚本。
3. **路径与编码约定（用户端脚本）**：运行时脚本统一放 `%LOCALAPPDATA%\ArcGIS-MCP\runtime\`（实际路径以 API 取值，不假设纯 ASCII）；含非 ASCII 文本的 .ps1 保存为 UTF-8 with BOM，纯 ASCII 脚本免 BOM（add-in 写出 restart_pro.ps1 时会保持纯 ASCII 内容）；.cmd 启动器首行 `chcp 65001>nul`；中文工程路径一律作为参数传入。本机 ASCII 中转目录 只是本机 Claude 终端链路的 ASCII 中转约定（Claude 终端工具拒绝非 ASCII 命令），不属于产品规范；产品侧如有类似需求，应使用可配置的中转目录。

## 回滚

- 1.1.4 安装包：`release/downloads/v1.1.4-20261117/`（zip SHA256 `d475f47c…c914`），重跑 `install.ps1` 即回到 1.1.4；
- 或恢复 `%USERPROFILE%\Documents\ArcGIS\AddIns\ArcGISPro` 下的 1.1.4 esriAddinX（包内 `install/ArcGISProMCP.esriAddinX`，SHA256 `7e6ca097…86ad4`）并回写 installed.json。
- 源码回滚：删除本目录，1.1.4 快照仍在 `release/customer-v1.1.4/source`（未改动）。

## 回归记录

### 2026-11-17 第 1 轮（构建 16:36）

已验证：

- 握手：initialize 报 1.1.5.0；tools/list 114（含 add_table）；get_capabilities 111 命令。
- F1 转发：set_environment 回显 forwarded=[cellSize, snapRaster]、未知键进 unsupported；get_environment 的 `_forwarded` 一致；RunCode.pyt 入口内读回 cellSize=20、snapRaster 精确路径（工具箱入口环境生效）；per-call environment 把 cellSize 从 20 覆盖回 10 生效。
- F1 真对齐（补齐测试 `release/v1.1.5-tests/live_verify_115_supp.py`）：先把 snap 栅格 Shift 3.7/1.3 m 构成偏移网格，cellSize=20 的坡度结果原点距 snap 网格 0.0 m、距 DEM 网格 3.7 m；不带 snapRaster 的对照落回 DEM 网格（0.0 m）。原 live_verify 按 20 m 取模的断言不成立（snap 栅格是 10 m 网格），已改为按 snap 栅格像元取模。
- F2：TinRaster 的 sample_distance 枚举 2 项、sample_value 默认 250 由 describe_geoprocessing_tool 返回。
- F3：create_map 缺省 basemap="none" 且新地图无底图；project_default 参数回显正常。**限制**：本机 services.arcgisonline.com 当时超时，显式 Topographic 也不产生底图图层（另存 .aprx 副本内无底图定义），底图层实机渲染验证受网络限制（1.1.4 同样如此）。**第 2 轮网络恢复后复测：project_default 的 has_basemap=True，限制解除。**
- F6：视图比例 1.6415 下，1600×900 → 自动 1600×975（fit_applied）；800×800 与 2400×900 按设计报“超过 25%”；已匹配尺寸不再调整；默认 preserve 保持 1600×900 精确。
- F8：name/map_name 别名均可；两者冲突报错。

发现缺陷并在回归中修复（见“补丁记录”）：

- F9：set_map_properties 改 epsg 触发 CalledOnWrongThreadException。
- F5：add_table（gdb 表与 CSV 表）触发 CalledOnWrongThreadException。

待第 2 轮（Pro 重启加载修复构建后；已执行，见下节）：

- 重启后先验 `ping`：1.1.5 / 112 命令 / 115 工具，再跑后续用例（防止测到旧 add-in）；
- live_verify_115.py 全量（含 F4/F7、F5 gdb 表重复拒绝、F9 修复验证）；
- live_verify_115_supp.py 复跑（F5 CSV 应通过）；
- 冷/热耗时 p50/p95 记录；
- `restart_pro` 实机自测放在全部回归之后（首次成功前不在回归中途使用）；自测时确认 `restart.ok` 的 command_count ≥112 且 PID 变化，客户端重连正常。

### 2026-11-17 第 2 轮（构建 17:33:21，DDF544F9…）

- 先验：新进程 `ping` 报 112 命令、initialize 报 1.1.5.0、tools/list 115（含 add_table 与 restart_pro）。
- `live_verify_115.py`：**26/26 全部通过**（日志 `release/v1.1.5-tests/live_verify_115.run2b.log`）。含 F9 修复验证（epsg 3857 生效、map_note）、F5 gdb 表入图/重名拒绝/移除、F4 栅格缩放与符号、F6 fit 600×366 与 preserve 600×400、F7 配色回读、F1 网格偏移 0.0。
  - 期间发现并修复测试脚本自身一处死代码：F6 段未被使用的 `ratio_ok` 计算对列表做除法导致崩溃（已删除后复跑全绿）。
- `live_verify_115_supp.py`：**12/12 全部通过**（`live_verify_115_supp.run3.log`）。F1 严格对齐（对偏移 snap 网格偏差 0.0、对 DEM 3.7 m；无 snap 对照落 DEM 网格）、F5 CSV 表、F6 三比例，且 **F3 底图复测 `has_basemap=True`**——第 1 轮的网络限制已解除，`project_default` 恢复旧行为实机成立。
- 冷/热耗时（第 2 轮同条件）：ping p50 15 / p95 20 ms（n=10）；get_project_info p50 16 / p95 23 ms（n=5）；RunCode 工具箱首次（冷）683 ms，热跑 n=4 p50 31 / p95 40 ms。对照 v2.0.x 守卫版每次 0.6–1.6 s，1.1.5 无明显守卫开销。
- `restart_pro` 首次实机自测（17:36:29–17:37:04，日志 `%LOCALAPPDATA%\ArcGIS-MCP\restart.log`）：`save=false` + 显式工程（当时 Pro 停在起始页无工程，属最安全的首次场景）。scheduled → 2 秒关闭（PID 21368）→ 带工程重开 → 15 秒桥接恢复 → `restart.ok` 记录新 PID 2980、commands=112；MCP 客户端自动重连，后续全部调用正常。WMI 脱离与三按钮保护按设计生效（无弹窗产生）。

## 补丁记录（回归测试期间）

| 日期 | 文件 | 问题 | 修复 |
|---|---|---|---|
| 2026-11-17 | Commands/MapCommands.cs SetMapPropertiesAsync | Map 在异步工作线程解析，随后在 MCT 上调用 SetSpatialReference → CalledOnWrongThreadException | MapHelpers.ResolveMap 移入 QueuedTask.Run；回复改用 changed["map"]，不再跨线程访问 Map |
| 2026-11-17 | Commands/LayerCommands.cs AddTableAsync | 同上，CreateStandaloneTable 在 MCT 上触碰工作线程创建的 Map | ResolveMap 移入 QueuedTask.Run；重名检查改为先按数据集名（或显式 name）查冲突，重名即报 already has |
| 2026-11-17 | Commands/SymbologyCommands.cs GetSymbology | TIN 图层落到“仅名称与类型”通用提示，未按 F4 裁决返回渲染器摘要 | 新增 TinLayer 分支：经 `GetDefinition() as CIMTinLayer` 读取 Renderers，返回首个渲染器类型与数量 |
| 2026-11-17 | tests/live_verify_115.py | F1 对齐断言按输出像元 20 m 取模，snap 栅格实为 10 m 网格，误报 | 改为按 snap 栅格像元取模并回传 snap_cell |
| 2026-11-17 | Commands/SessionCommands.cs、Resources/restart_pro.ps1 | 三方评审第 2 轮：固定 3 秒延时与响应竞态、`save` 未含 SaveEditsAsync、直接 Start 可能随 Pro 作业对象被杀、保存提示盲点“否/No”、重启后无版本校验 | 延时 ≥8（默认 10）、并发拒绝、先 SaveEdits 后 Save（异常即中止）、WMI 优先+直接回退、仅三按钮齐全的保存对话框代答、restart.ok 需 command_count≥112 且 PID 变化 |
| 2026-11-17 | scripts/install_claude_permissions.ps1 | 三方评审第 2 轮：默认写全局 settings 权限面过大、-Remove 会误删同前缀自定义规则、回退链含不存在的 tools.json 路径 | 默认预览、-Apply 才写、默认项目级、-ServerName、14 个执行/破坏类工具保留确认、-Remove 精确撤销、修剪回退链 |
| 2026-11-17 | tests/test_catalog_matches_bridge.py | 每个 catalog 工具必须有 Python 处理器，restart_pro 仅 C# 实现会使该测试必红 | 新增 CSHARP_ONLY 白名单（restart_pro） |

## 构建记录

- 首建 2026-11-17 16:36（dotnet 10.0.300，0 警告 0 错误）：SHA256 `ADE6192CAA5161BC9F6AB94825C8B5F8F7EB06A2800DCE0C58F50AA45A351E34`，433,793 字节。
- 修复后重建 2026-11-17 17:07（F9/F5 线程修复）：SHA256 `BE7464E2B4C9ACC4D444E06D1E36225B989D56DDD71FC4AEBF979CA6C2CAEE56`，433,811 字节。
- 最终重建 2026-11-17 17:14（含 TIN 符号摘要）：SHA256 `D2ABAEF21227688615171FBC86230EFD495514B7ED03DA7983FB74136FAC3CD0`，434,000 字节。
- 第 2 批重建 2026-11-17 17:26（新增 restart_pro + 权限模板脚本）：SHA256 `EBB88EBE6DDB3B565466C3935353505153C7E6F615FD87EE78BC017EB7B0AD9A`，437,303 字节。
- 评审加固重建 2026-11-17 17:33:21（三方会谈第 2 轮裁决落实）：SHA256 `DDF544F9F787E2ECA4EBA77F180AA27B22CD41C7E778BB5A2005565470CA8E13`，439,118 字节；部署副本（Documents\ArcGIS\AddIns\ArcGISPro）与 `release/v1.1.5-artifacts/` 一致，真实盘 installed.json 按此哈希更新。
- 包内：Config.daml `version="1.1.5"`；ArcGISProMCP.dll 内嵌 gp-parameter-details.json 与 restart_pro.ps1
- 工具 115（新增 add_table、restart_pro）；命令 112（新增同名两项）
- GP 元数据：gp-parameters.json 659 KB（4056 键）；gp-parameter-details.json 1.01 MB（1656 个工具含枚举/短默认值，schema_version 1）
- 相对 1.1.4 源码差异（release/customer-v1.1.4/source → release/v1.1.5-src）：
  - addin/ArcGISProMCP：ArcGISProMCP.csproj、Config.daml、Commands/{Geoprocessing,Layer,Map,MapHelpers,Raster,Symbology,View,Session}Commands.cs、Resources/gp-parameter-details.json（新）、Resources/restart_pro.ps1（新）
  - arcgis_pro_plugin/arcgis_mcp/h_layers.py（add_table 处理器）
  - src/arcgis_pro_mcp/catalog.py（add_table、restart_pro 与新增参数 schema）、scripts/dump_gp_parameters.py、scripts/install_claude_permissions.ps1（新）
  - pyproject.toml（1.1.5，115 tools）、tests/live_verify_115.py（新）、docs/local-1.1.5.md（新）
