# 更新日志

## Customer 1.16（1.14 增强版，构建号 1.1.6，2026-11-18）

- 相对 v1.1.4 累计合入 1.1.5 的 F1–F10（环境注入、describe 增强、create_map 无底图、栅格/TIN 支持、add_table、fit:auto、显式配色、epsg 生效、restart_pro）与 1.1.6 的 V-1–V-7（toolbox 路径纠错提示、get_pump_status 桥缺失时结构化返回、错误分级与读超时、diagnose 主动探测、工具描述修正、HTTP 断言加固、桥 down 缓存）。
- 规模：112 命令 / 115 工具；add-in SHA256 `96C7829E…E625`。
- 未创建 Release：不上传发行资产、不打 tag；默认分支保持 v1.1.4。

## v1.1.4（本客户版）

- add-in 升级为 ArcGIS Pro 3.7 / .NET 10 构建，工具数 113。
- 修复 `activate_map` 死锁问题（图层类命令此前会整体挂起）。
- 引入三车道任务调度器（JobScheduler）：查询/图层/地理处理分道排队，重任务不再阻塞轻命令；新增 `get_queue_status` 查询车道与排队状态。
- 修复并验证 `run_python_toolbox_tool` 首次信任弹窗下的执行流程。
- 客户版打包新增：stdio 桥（免 Python）、RunCode 工具箱、弹窗守护（含本地 MacDialog 渲染库）、Python 桥、Claude/Codex 接入脚本、自检脚本、完整源码快照。

## 上游 1.1.1 基线能力（Knight60/ArcGIS-Pro-MCP）

- 112 个 C# 原生工具，进程内执行（约 9 ms）。
- `run_geoprocessing_tool` 支持全部 GP 工具，按参数名调用。
- 可选 Python 桥提供任意 arcpy 执行（6511）。
- MCP over HTTP 于 `127.0.0.1:6520/mcp`，旧 TCP 桥 6510。
- AGPL-3.0-or-later 开源。

## 客户版打包说明（2026-11-16）

- 组件版本：add-in 1.1.4（FileVersion 1.1.4.0，SHA256 见 `SHA256SUMS.txt`）。
- 依赖审计：除 ArcGIS Pro 3.7+ 与 Windows 自带 PowerShell/.NET 外，无任何外部依赖；离线可用。
- 已知事项：add-in 未签名，首次加载需手动信任；弹窗守护为脚本式（按需启动，不常驻）。
