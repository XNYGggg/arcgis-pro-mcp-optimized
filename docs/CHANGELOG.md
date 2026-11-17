# 更新日志

本文件记录本仓库客户版（ArcGIS Pro MCP 优化版）的显著变更。
格式参考 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，版本号遵循 [语义化版本](https://semver.org/lang/zh-CN/)。
当前稳定线为 v1.1.x；2.x 为测试线（不提供下载入口），其条目不在本文件展开。

## [Unreleased]

### Added

- 根级 CI（GitHub Actions）：复用 `source/.github/workflows/tests.yml`，在无 ArcGIS Pro 的 Windows 环境运行客户端注册、HTTP transport（.NET 8/10）、catalog/pump/端到端 mock 与安装脚本完整性校验；新增安装包完整性作业（add-in SHA256、`VERSION.json` 一致性）；上游 HTTP 测试的 112 工具断言在 CI 临时工作区校准为 113（客户版实际工具数），提交的源码快照不变。
- GitHub Issue 表单：`bug_report.yml`、`feature_request.yml`，关闭空白 Issue，并提供排障手册/使用指南入口。

### Changed

- `docs/CHANGELOG.md` 按 Keep a Changelog 结构整理；四语 README 增加更新日志入口。
- `BRANCHES.md` 明确分支职责：`main` 为稳定维护线、`customer-v1.1.4` 为稳定版快照、`v2.0.2-dev` 为不对外开发线；不新建 LTS 分支、不承诺 LTS 支持期限。
- `docs/使用指南.md` 新增“非交互安装与退出码”说明（现有 `-SkipClients`、退出码 0/1/2；`-Quiet` 为后续版本候选，尚未提供）。

### Fixed

- 无（本轮为仓库工程化改动，不涉及运行代码）。

### Security

- 无。

## [1.1.4] - 2026-11-16

### Added

- 客户版打包新增：stdio 桥（免 Python）、RunCode 工具箱、弹窗守护（含本地 MacDialog 渲染库）、Python 桥、Claude/Codex 接入脚本、自检脚本、完整源码快照。
- 引入三车道任务调度器（JobScheduler）：查询/图层/地理处理分道排队，重任务不再阻塞轻命令；新增 `get_queue_status` 查询车道与排队状态。

### Changed

- add-in 升级为 ArcGIS Pro 3.7 / .NET 10 构建，工具数 113。

### Fixed

- 修复 `activate_map` 死锁问题（图层类命令此前会整体挂起）。
- 修复并验证 `run_python_toolbox_tool` 首次信任弹窗下的执行流程。

### Security

- 二进制未签名：首次加载需手动信任（签名需商业证书，见 Release 说明）。

## 上游基线：1.1.1（Knight60/ArcGIS-Pro-MCP）

- 112 个 C# 原生工具，进程内执行（约 9 ms）。
- `run_geoprocessing_tool` 支持全部 GP 工具，按参数名调用。
- 可选 Python 桥提供任意 arcpy 执行（6511）。
- MCP over HTTP 于 `127.0.0.1:6520/mcp`，旧 TCP 桥 6510。
- AGPL-3.0-or-later 开源。

## 附：客户版打包说明（v1.1.4）

- 组件版本：add-in 1.1.4（FileVersion 1.1.4.0，SHA256 见 `SHA256SUMS.txt`）。
- 依赖审计：除 ArcGIS Pro 3.7+ 与 Windows 自带 PowerShell/.NET 外，无任何外部依赖；离线可用。
- 已知事项：add-in 未签名，首次加载需手动信任；弹窗守护为脚本式（按需启动，不常驻）。
- 校验口径：仓库根 `SHA256SUMS.txt` 对应 v1.1.4 发布包快照，仓库内后续文档修订不回写；`install/01-check.cmd` 仅校验 `install/addin.sha256`。

[Unreleased]: https://github.com/XNYGggg/arcgis-pro-mcp-optimized/compare/v1.1.4...main
[1.1.4]: https://github.com/XNYGggg/arcgis-pro-mcp-optimized/releases/tag/v1.1.4
