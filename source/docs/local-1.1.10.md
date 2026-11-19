# ArcGIS Pro MCP 本地升级 1.1.10（重启自愈 · 工程化收尾 · 仅本机）

- 日期：2026-11-19
- 基线：1.1.9（add-in SHA256 `1574BF0D5D1CB5D600506F02F82781EF9DC784E68C18CE3E29D1EE4000C6F39C`，466,234 字节，112 命令 / 115 工具）
- 依据：2026-11-18 三方会谈过程报告五项整改——restart_pro 偶发挂起、部署脚本被权限分类器拦截、测试脚本 scratch 依赖、CopyRaster 参数名、终端 6 标签上限

## 变更清单

| 项 | 变更 | 主要文件 |
|---|---|---|
| 1 | **restart_pro 超时自愈**：保存框探测改子进程 Job + 15 秒硬超时（超时放弃并记日志）；按钮匹配支持本地化 `否(N)`/`是(Y)`/`取消(C)`（原 `(&N)` 形式漏配）；restart.log 改 UTF-8 并加阶段心跳（关闭进度/Responding/桥等待）；TCP 连接 5 秒超时；保持"不强杀 Pro" | addin/ArcGISProMCP/Resources/restart_pro.ps1、scripts/restart_pro.ps1 |
| 2 | **调度标志过期**：`_restartScheduled` 加时间戳，10 分钟后可接管——助手挂掉后不再永久拒绝新 restart 请求 | Commands/SessionCommands.cs |
| 3 | **测试脚本加固**：9 个实机回归脚本 `os.makedirs(SCRATCH)` + `MCP_TEST_SCRATCH`/`MCP_TEST_URL`/`MCP_TEST_PROJ_GDB`/`MCP_TEST_OUTPUT` 可覆盖 | output/mcp_tests_20261118/*.py |
| 4 | **部署归一**：`update_installed.py <version> <sha256> [note]` 位置参数版（哈希不符拒写、刷新 installed_at）；BUILD.md 增"Recording a local upgrade"；项目 CLAUDE.md 增"终端与部署操作约定" | scripts/update_installed.py、BUILD.md、CLAUDE.md |
| 5 | **避坑库**：新增 B.2.10——GP 参数名与界面叫法不一致（CopyRaster→`out_rasterdataset` 等） | reference/GIS_SKILL_V1.0/knowledge/references/29_避坑库.md |
| 6 | 版本 1.1.9→1.1.10 | Config.daml、ArcGISProMCP.csproj、pyproject.toml、CHANGELOG.md |

## 构建

- 环境：dotnet SDK 10.0.300；ArcGIS Pro 3.7.0.1901（net10.0-windows）。
- `scripts/build.ps1` 全过：installer 脚本检查、client-registration、HTTP transport（net8.0 + net10.0）、单文件安装器检查。
- **ArcGISProMCP.esriAddinX**：467,650 字节，SHA256 `767D53D75DA6D2BCDF440BBA6E7BDE731F96092602EA8CAFE578AB2B1698F1A7`，未签名（本机 Add-in security=0，可加载）。

## 部署与验证（2026-11-19 11:43–11:47，真实会话）

1. 备份：`Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX.bak-20261119-deploy110`（1.1.9，1574BF0D，466,234 字节）。
2. 官方 `install_addin.ps1 -AddInPath <候选> -Reinstall` 安装；certutil 回读目标哈希 767D53D7… 一致。
3. 簿记：`update_installed.py 1.1.10 767D53D7…` 写真实 installed.json（version=1.1.10、addin_size=467,650、installed_at=2026-11-19T11:45:37+0800）。**注意**：此前真实 installed.json 停留在 1.1.7（3B3CA9A5），与已安装包（1.1.9）不一致，本次一并修正。
4. `scripts/restart_pro.ps1`（本次修复后首次实战）启动 Pro 并打开 MyProject_05：桥 20 秒内监听、工程 20 秒内应答、退出码 0。
5. MCP initialize 握手：serverInfo `1.1.10.0`；ping command_count=112；project_path 正确指向 MyProject_05.aprx。

## 回滚

- 恢复 `Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX` 为 `.bak-20261119-deploy110`；
- installed.json 回写 version=1.1.9、addin_sha256=`1574BF0D…`、addin_size=466234；
- 重启 Pro。
