# ArcGIS Pro MCP 本地升级 1.1.11（发布前两轮三方评审修正 · 仅本机）

- 日期：2026-11-19
- 基线：1.1.10（add-in SHA256 `767D53D75DA6D2BCDF440BBA6E7BDE731F96092602EA8CAFE578AB2B1698F1A7`，467,650 字节，112 命令 / 115 工具）
- 依据：2026-11-19 客户包发布前两轮三方评审（Codex 两轮均为「修正后可以发布」；DeepSeek 首轮超时、次轮账户欠费离线）

## 变更清单

| 项 | 变更 | 主要文件 |
|---|---|---|
| 1 | **失败清理按对象身份**：删除数据集前按数据源解析引用它的图层/独立表（并捕获所属地图），删除后按对象身份、从该地图移除；不再按名称匹配（同名异源不再误删）；数据源读取异常、删除失败、移除失败逐条收集为 `Cleanup problems` 随工具错误报告；删除失败时错误文案不再声称「已删除」 | addin/ArcGISProMCP/Commands/GpWrapperCommands.cs |
| 2 | **脚本去本机路径**：`update_installed.py` 默认从用户目录与脚本位置推导，`MCP_*` 环境变量可覆盖；入口支持 `__main__` 与 RunCode 的 `__mcprun__`（无 argv 时读 `MCP_UPDATE_ARGS`）；BUILD.md 记录两种调用方式 | scripts/update_installed.py、BUILD.md |
| 3 | **清理旧部署脚本与本机路径**：移除 `scripts/deploy_local_116.py`；`tests/bench_latency.py`、`tests/live_verify_116_extra.py` 改为环境变量/相对路径；历史文档中本机用户目录路径替换为 `%USERPROFILE%` | scripts/、tests/、docs/local-1.1.5.md、docs/local-1.1.6.md |
| 4 | **构建 PathMap**：`<PathMap>$(MSBuildProjectDirectory)=/_/src</PathMap>`，DLL/PDB 中不再包含构建机源码路径（扫描 0 命中） | addin/ArcGISProMCP/ArcGISProMCP.csproj |
| 5 | **兼容口径统一**：VERSION.json、四语 README 徽标、install.ps1 提示均为「仅实机验证 Pro 3.7；更高版本未测试」 | VERSION.json、README×4、install/install.ps1 |
| 6 | 版本 1.1.10→1.1.11 | Config.daml、ArcGISProMCP.csproj、pyproject.toml、CHANGELOG.md |

## 构建（最终构建）

- 环境：dotnet SDK 10.0.300；ArcGIS Pro 3.7.0.1901（net10.0-windows）。
- `scripts/build.ps1` 全过（installer 脚本检查、client-registration、HTTP transport net8.0+net10.0、单文件安装器检查）。
- **ArcGISProMCP.esriAddinX**：468,529 字节，SHA256 `10BAD933D0F4FC102DA3E36EED588D24ACE15AEC433A9E3BA61940FCD9F78E85`，未签名。
- 自安装包 `Install-ArcGISProMCP.cmd`：648,622 字节，SHA256 `C4DEEF256A7D8E653058D1DF3BB35B66EC752793FB7037A48C65A01C1014FF14`，内嵌载荷回算一致。
- 中间构建：1.1.11 r1 `31B17BC4…C4846`（评审前）、r2 `A728B7ED…F06E`（第 1 轮修正后）；最终 r3 为上述 `10BAD933…8E85`（第 2 轮修正后），r1/r2 均未发布。

## 部署与验证（2026-11-19，真实会话）

1. 备份：`release/v1.1.10-artifacts/ArcGISProMCP.esriAddinX`（767D53D7，467,650 字节）。
2. 官方 `install_addin.ps1 -AddInPath <候选> -Reinstall` 安装；certutil 回读哈希 `10BAD933…8E85` 一致。
3. `scripts/restart_pro.ps1 -Project ...MyProject_95.aprx` 重启并加载。
4. 握手：serverInfo `1.1.11.0`；ping `command_count=112`、工程路径正确。
5. 簿记：wrapper 经 RunCode 执行 `update_installed.py`，installed.json 更新（version=1.1.11、addin_size=468,529、sha256=10BAD933…）。
6. 回归：`p0_regression_117.py` 对当前工程适用的断言全部 PASS（17/21；4 条失败为脚本引用已退役 MyProject_05 工程的图层名——`高程点_工作副本`/`观光线` 不存在于现工程 MyProject_95，属测试环境差异）；其中「无效类型拒绝→零残留」「重复字段失败→半成品删除」清理路径实测通过。`get_features` 分页/几何摘要/WKT 精度在现工程图层实测通过。

## 已知事项

- 仅验证 ArcGIS Pro 3.7；其他 Pro 版本未覆盖。
- add-in 未签名（首次加载需信任）。

## 回滚

- 恢复 `Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX` 为 `release/v1.1.10-artifacts/ArcGISProMCP.esriAddinX`（或 `.bak-*` 备份）；
- installed.json 回写 version=1.1.10、addin_sha256=`767D53D7…F1A7`、addin_size=467650；
- 重启 Pro。
