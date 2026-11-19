# ArcGIS Pro MCP 本地升级 1.1.11（发布前三方评审修正 · 仅本机）

- 日期：2026-11-19
- 基线：1.1.10（add-in SHA256 `767D53D75DA6D2BCDF440BBA6E7BDE731F96092602EA8CAFE578AB2B1698F1A7`，467,650 字节，112 命令 / 115 工具）
- 依据：2026-11-19 客户包发布前三方评审（Codex 结论「修正后可以发布」，DeepSeek 首轮超时）——失败清理按对象身份、清理失败上报、脚本去本机路径、DLL/PDB 去构建路径、1.1.4 措辞修正

## 变更清单

| 项 | 变更 | 主要文件 |
|---|---|---|
| 1 | **失败清理按对象身份**：`DeleteHalfCreatedAsync` 在删除数据集前按数据源解析引用它的图层/独立表，删除后按对象身份移除；不再按名称匹配（同名异源不再误删）；删除失败/移除失败写入 `Cleanup problems` 并随工具错误报告 | addin/ArcGISProMCP/Commands/GpWrapperCommands.cs |
| 2 | **脚本去本机路径**：`update_installed.py` 默认从用户目录与脚本位置推导，`MCP_*` 环境变量可覆盖 | scripts/update_installed.py |
| 3 | **构建 PathMap**：`<PathMap>$(MSBuildProjectDirectory)=/_/src</PathMap>`，DLL/PDB 中不再包含构建机源码路径 | addin/ArcGISProMCP/ArcGISProMCP.csproj |
| 4 | 版本 1.1.10→1.1.11 | Config.daml、ArcGISProMCP.csproj、pyproject.toml、CHANGELOG.md |

## 构建

- 环境：dotnet SDK 10.0.300；ArcGIS Pro 3.7.0.1901（net10.0-windows）。
- `scripts/build.ps1` 全过：installer 脚本检查、client-registration、HTTP transport（net8.0 + net10.0）、单文件安装器检查（含重命名/嵌套发现、卸载）。
- **ArcGISProMCP.esriAddinX**：468,175 字节，SHA256 `A728B7ED63B07391A83D49D14E88735D0926E7F2F0B1E4D3E755452D6593F06E`，未签名。DLL/PDB 内构建路径扫描为 0。

## 部署与验证（2026-11-19 13:05–13:12，真实会话）

1. 备份：`release/v1.1.10-artifacts/ArcGISProMCP.esriAddinX`（767D53D7，467,650 字节）。
2. 官方 `install_addin.ps1 -AddInPath <候选> -Reinstall` 安装；certutil 回读目标哈希 `A728B7ED…F06E` 一致。
3. `scripts/restart_pro.ps1 -Project ...MyProject_95.aprx`（1.1.10 修复的脚本，本次为发布前实战）重启并加载。
4. 握手：serverInfo `1.1.11.0`；ping `command_count=112`、工程路径正确。
5. 簿记：`update_installed.py 1.1.11 A728B7ED… "customer-v1.1.11 package build 2026-11-19"` 经 RunCode 执行，installed.json 更新（version=1.1.11、addin_size=468,175）。
6. 回归：`p0_regression_117.py` 对当前工程适用的断言全部 PASS（17/21；4 条失败为脚本引用已退役 MyProject_05 工程的图层名，属测试环境差异）；其中「重复字段失败→半成品删除」路径实测通过。`get_features` 分页（truncated/next_offset）、几何摘要、WKT 精度在 MyProject_95 图层（高程点_工作 / 等高线_工作）实测通过。

## 已知事项

- 仅验证 ArcGIS Pro 3.7；其他 Pro 版本未覆盖。
- add-in 未签名（首次加载需信任）。

## 回滚

- 恢复 `Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX` 为 `release/v1.1.10-artifacts/ArcGISProMCP.esriAddinX`（或 `.bak-*` 备份）；
- installed.json 回写 version=1.1.10、addin_sha256=`767D53D7…F1A7`、addin_size=467650；
- 重启 Pro。
