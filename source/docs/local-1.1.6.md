# ArcGIS Pro MCP 本地升级 1.1.6（仅本机 · 未发布 GitHub）

- 日期：2026-11-18
- 基线：v1.1.5（add-in SHA256 `DDF544F9F787E2ECA4EBA77F180AA27B22CD41C7E778BB5A2005565470CA8E13`，439,118 字节，112 命令 / 115 工具）
- 依据：两场三方会谈——
  - `agent_bridge\meetings\20261118-174056-runcode-call-friction-cf9ea300`（收敛稿：agent_bridge\docs\RunCode调用摩擦-收敛稿-2026-11-18.md）
  - `agent_bridge\meetings\20261118-180041-pump-status-friction-872d290b`（收敛稿：agent_bridge\docs\Python桥pump状态自查-收敛稿-2026-11-18.md）
  - 汇总：agent_bridge\docs\整改清单-RunCode与pump-2026-11-18.md
- 触发：2026-11-18 实验12首次运行的两类摩擦（① 命名参数被拒 + 路径笔误；② Pro 重启后桥未启动时 get_pump_status 返回 Failed）

## 变更清单（V-1～V-7）

| 项 | 变更 | 主要文件 |
|---|---|---|
| V-1 | **失败即教学**：toolbox 路径不存在时追加相似路径提示（“Did you mean …?”，按路径段做编辑距离匹配，忽略空格/下划线/连字符——即 `claude code app` ↔ `code_app` 这类手误）；命名参数报错追加可直接套用的调用示例。原错误前缀保留 | Commands/GeoprocessingCommands.cs |
| V-2 | **状态查询兜底**：`get_pump_status` 在桥连接**被明确拒绝**时返回结构化结果 `{running:false, bridge_listening:false, pump:"unknown", code:"BRIDGE_NOT_LISTENING", hint}`，不再抛 Failed；`execute_arcpy_code` 等执行类命令**不降级**、继续硬失败 | Bridge/PythonFallback.cs |
| V-3 | **错误语义分级**：`last_error_code` / `last_error_at`（UTC）随报错记录；消息追加 `[code=…]`（BRIDGE_NOT_LISTENING / BRIDGE_ERROR / BRIDGE_CLOSED / BRIDGE_TIMEOUT / UNSUPPORTED）；状态查询的读响应增加 5 秒超时（执行类命令保持无上限，不破坏长任务） | Bridge/PythonFallback.cs |
| V-4 | **diagnose 主动探测**：`python_fallback` 段异步探测配置端口（默认 6511，预算 300 ms，独立于转发锁），追加 `bridge_listening` / `probe_ms` / `probe_error` / `hint` 字段（只增不改） | Bridge/PythonFallback.cs、Commands/SessionCommands.cs |
| V-5 | **schema/描述修正**：`parameters` 说明改为“原生端只收 args 位置参数，命名参数仅 Python 桥端支持”；`execute_arcpy_code` / `get_pump_status` / `get_queue_status` 描述标明桥依赖；由 catalog.py 重新生成 tools.json（115 工具） | src/arcgis_pro_mcp/catalog.py、Resources/tools.json |
| V-6 | **断言加固**（修复既有回归）：http-transport 原断言恰好 112 个工具，而 tools.json 实为 115——改为“名称去重 + 必备工具子集”校验；加命令不再误红 | tests/http-transport/Program.cs |
| V-7 | **桥 down 缓存**：明确拒绝的连接结论缓存 5 秒（仅用于状态查询的快路径，执行类命令每次照常尝试），避免轮询每次等满连接超时 | Bridge/PythonFallback.cs |

- 版本号：Config.daml `version="1.1.6"`、csproj `<Version>1.1.6</Version>`、pyproject `1.1.6`、CHANGELOG 新增 1.1.6 段。
- 兼容性：命令数仍 112、工具数仍 115；`get_pump_status` 在桥未启动时由“异常”变“结构化成功（running:false）”——依赖异常判定桥状态的客户端需适配；其余行为不变。

## 构建

- 环境：dotnet SDK 10.0.300；ArcGIS Pro 3.7（net10.0-windows / ProTargetVersion=3.7）；未签名（本机 BlockAddIns 未设置，Pro 不受限加载）。
- 构建：2026-11-18 18:19，`scripts/build.ps1`（Release）。
- **ArcGISProMCP.esriAddinX**：SHA256 `96C7829E2A26E16C270722A84605F9945DA2348DFCF202C658A64EA9E012E625`，444,124 字节。
- 单文件安装器 `Install-ArcGISProMCP.cmd`：615,843 字节。
- 构建期测试：client-registration 全过；installer 脚本检查全过（parse/undefined 检查、发现与卸载、无关数据保护）；http-transport net8.0 + net10.0 全过。

## 部署与验证

1. 部署脚本 `scripts/deploy_local_116.py` 经 RunCode.pyt 在 Pro 进程内执行（真实盘）：源包哈希核对 → 备份旧包 → 复制新包 → 回算哈希 → 更新 `%LOCALAPPDATA%\ArcGIS Pro MCP\installed.json`。
   - 备份：`Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX.bak-20261118-182059`；同时归档 `release\v1.1.6-artifacts\installed-before-upgrade.esriAddinX`（即 v1.1.5，DDF544F9…）。
2. `restart_pro(save=true, delay_seconds=10)` 重启：新 PID 32476；握手 initialize 报 **1.1.6.0**；112 命令 / 115 工具；工程 MyProject_05 正常打开，地图“实验12”图层完好。
   - 备注：18:21 重启关闭阶段出现“是(Y)/否(N)/取消(C)”三按钮对话框（非“保存/不保存/取消”签名），守护按安全设计未代答，24 秒后 Pro 正常关闭并重开，工程与数据无损。若后续再遇可人工点一次。
3. 实机回归（日志目录 `release\v1.1.6-tests\`）：
   - `live_verify_116_extra.py`：**10/10**（桥停状态下）。含：initialize 1.1.6.0；get_pump_status 结构化 down；diagnose `bridge_listening=false`、`probe_ms=310`；路径笔误返回 “Did you mean …code_app…”；命名参数返回含 `Example:` 的报错；schema 文本含“positional/bridge”；execute_arcpy_code 报错含 `[code=BRIDGE_NOT_LISTENING]`。
   - `live_verify_116.py`：**26/26**（run3.log，F1–F9 全量）。含 F1 网格对齐 0.0、F2 枚举/默认值、F3 无底图、F4 栅格缩放与符号、F5 add_table 入图/重名拒绝/移除、F6 fit auto/preserve、F7 配色回读、F8 地图别名、F9 坐标系。
     - 测试脚本自身两处隔离加固（不是产品问题）：启动前清理遗留 `v116_*` 地图；F5 前移除图中已有的统计表（本工程实验12成果本就在图中）。
   - `live_verify_116_bridge_stub.py`：**4/4**（协议桩模拟桥在线）：get_pump_status 转发 `running=true`；diagnose `bridge_listening=true`；execute_arcpy_code 转发成功；桩停止后回到结构化 down。
   - **真实 Python 桥三态实测（2026-11-18 18:22–18:31，用户手工粘贴）**：① 桥未启动 → 结构化 down（code=BRIDGE_NOT_LISTENING），diagnose `bridge_listening:false`；② 桥在线 pump 未起 → get_pump_status 转发 `running:false` + start_pump 提示，纯路径 `execute_arcpy_code` 可用，访问 CURRENT 得到“需要主线程 pump”的明确报错；③ 桥泵全起 → `running:true`（message-window/MainThread），`execute_arcpy_code` 访问 CURRENT 返回工程路径。记录见 `release\v1.1.6-tests\live_verify_real_bridge_states.md`。

## 回滚

- 恢复 `%USERPROFILE%\Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX` 为 `.bak-20261118-182059`（或 `release\v1.1.6-artifacts\installed-before-upgrade.esriAddinX`），回写 installed.json（version=1.1.5、addin_sha256=DDF544F9…），重启 Pro。
- 源码回滚：删除 `release\v1.1.6-src`；`release\v1.1.5-src` 未改动。

## 构建记录

- 2026-11-18 18:19 首建（Release，未签名）：SHA256 `96C7829E2A26E16C270722A84605F9945DA2348DFCF202C658A64EA9E012E625`，444,124 字节。
- 包内：Config.daml `version="1.1.6"`；tools.json 115 工具；命令 112（无增减）。
