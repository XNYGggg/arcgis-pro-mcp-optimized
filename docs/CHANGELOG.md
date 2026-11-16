# 更新日志

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
