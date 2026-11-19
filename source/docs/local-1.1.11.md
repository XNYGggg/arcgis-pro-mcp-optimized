# ArcGIS Pro MCP 本地升级 1.1.11（发布前三轮三方评审修正 · 仅本机）

- 日期：2026-11-19
- 基线：1.1.10（add-in SHA256 `767D53D75DA6D2BCDF440BBA6E7BDE731F96092602EA8CAFE578AB2B1698F1A7`，467,650 字节，112 命令 / 115 工具）
- 依据：2026-11-19 客户包发布前三轮评审。Codex 三轮均判「修正后可以发布」（三轮共 12 项，全部落实）；DeepSeek 首轮超时、次轮桥接密钥欠费，第三轮改用系统环境变量 `Claude调用harness` 的密钥直连 dsh，判「可以发布」（附 3 条不阻断的可选项，其中路径项已在终审修正中处理）。

## 变更清单

| 项 | 变更 | 主要文件 |
|---|---|---|
| 1 | **失败清理按对象身份（捕获先行）**：`ApplyAndVerifyFieldsAsync` 在字段处理开始前捕获接收输出的地图与引用的图层/独立表（`CaptureCreatedOutputAsync`）；删除数据集后按对象身份、从捕获地图移除；`AddField` 抛异常同样进入清理；数据源读取/删除/移除失败逐条进入 `Cleanup problems` 并随错误报告；删除失败时文案改为「may still exist」 | addin/ArcGISProMCP/Commands/GpWrapperCommands.cs |
| 2 | **脚本去本机路径**：`update_installed.py` 默认路径从用户目录与脚本位置推导，`MCP_*` 可覆盖；入口支持 `__main__` 与 RunCode 的 `__mcprun__`，优先读取 `MCP_UPDATE_ARGS`（备注保留整串），BUILD.md 记录两种调用方式 | scripts/update_installed.py、BUILD.md |
| 3 | **清理旧脚本与路径残留**：移出 `deploy_local_116/117.py`、`update_installed_117/118/119.py` 及 `.result.json`、`bench_tiny.py.result.json`；`tests/bench_latency.py`、`tests/live_verify_115/116.py` 改为环境变量/相对路径（`MCP_VERIFY_GDB` / `MCP_VERIFY_SNAP` / `MCP_RUNCODE_PYT` / `TEMP`）；历史文档去本机用户目录与中转路径 | scripts/、tests/、docs/local-1.1.5.md、docs/local-1.1.6.md |
| 4 | **构建 PathMap**：`<PathMap>$(MSBuildProjectDirectory)=/_/src</PathMap>`，DLL/PDB 无构建机源码路径（扫描 0 命中） | addin/ArcGISProMCP/ArcGISProMCP.csproj |
| 5 | **兼容口径统一**：VERSION.json「3.7 (built and live-verified on 3.7; later versions untested)」、四语徽标 ArcGIS Pro 3.7、install.ps1 提示「仅 3.7 实机验证」且 >3.7 警告按主/次版本判断（3.7.0.1901 不误触发） | VERSION.json、README×4、install/install.ps1 |
| 6 | 版本 1.1.10→1.1.11 | Config.daml、ArcGISProMCP.csproj、pyproject.toml、CHANGELOG.md |

## 构建（最终构建）

- 环境：dotnet SDK 10.0.300；ArcGIS Pro 3.7.0.1901（net10.0-windows）。
- `scripts/build.ps1` 全过（installer 脚本检查、client-registration、HTTP transport net8.0+net10.0、单文件安装器检查）。
- **ArcGISProMCP.esriAddinX**：469,270 字节，SHA256 `CC454A9FC20D4C0136673F96147BF55B7CA8D621A6B88ADFD7E5E83F519F5A50`，未签名。
- 自安装包 `Install-ArcGISProMCP.cmd`：649,619 字节，SHA256 `0D870BE4A02CD0C4405DE97F5B8611615AB9A86436BF51864AC15CCE4AB4776F`，内嵌载荷回算一致。
- 中间构建（均未发布）：r1 `31B17BC4…C4846`（评审前）、r2 `A728B7ED…F06E`（第 1 轮修正后）、r3 `10BAD933…8E85`（第 2 轮修正后）；最终 r4 = `CC454A9F…5A50`（第 3 轮修正后）。

## 部署与验证（2026-11-19，真实会话）

1. 备份：`release/v1.1.10-artifacts/ArcGISProMCP.esriAddinX`（767D53D7，467,650 字节）。
2. 官方 `install_addin.ps1 -AddInPath <候选> -Reinstall` 安装；certutil 回读哈希 `CC454A9F…5A50` 一致。
3. `scripts/restart_pro.ps1 -Project ...MyProject_95.aprx` 重启并加载。
4. 握手：serverInfo `1.1.11.0`；ping `command_count=112`、工程路径正确。
5. 簿记：wrapper 经 RunCode 执行 `update_installed.py`，installed.json 更新（version=1.1.11、addin_size=469,270、sha256=CC454A9F…）。
6. 回归：`p0_regression_117.py` 对当前工程适用的断言全部 PASS（17/21；4 条失败为脚本引用已退役 MyProject_05 工程的图层名——`高程点_工作副本`/`观光线` 不存在于现工程 MyProject_95，属测试环境差异）；「无效类型拒绝→零残留」「重复字段失败→半成品删除」清理路径实测通过。`get_features` 分页/几何摘要/WKT 精度在现工程图层实测通过。
7. 克隆复核：以 `git clone` 把本分支检出到独立目录，逐文件校验 `SHA256SUMS.txt` 194 条——mismatch 0 / missing 0 / extra 0（行尾按 `.gitattributes` 的 `eol=crlf` 检出，与清单口径一致）。

## 已知事项

- 仅验证 ArcGIS Pro 3.7；其他 Pro 版本未覆盖。
- add-in 未签名（首次加载需信任）。
- build-report.json 的 `runtimeValidation` 沿用构建流水线的保守字段（pending per claimed version），实机验证记录以本文件为准。

## 回滚

- 恢复 `Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX` 为 `release/v1.1.10-artifacts/ArcGISProMCP.esriAddinX`（或 `.bak-*` 备份）；
- installed.json 回写 version=1.1.10、addin_sha256=`767D53D7…F1A7`、addin_size=467650；
- 重启 Pro。
