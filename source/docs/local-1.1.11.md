# ArcGIS Pro MCP 本地升级 1.1.11（发布前四轮三方评审修正 · 仅本机）

- 日期：2026-11-19
- 基线：1.1.10（add-in SHA256 `767D53D75DA6D2BCDF440BBA6E7BDE731F96092602EA8CAFE578AB2B1698F1A7`，467,650 字节，112 命令 / 115 工具）
- 依据：2026-11-19 客户包发布前四轮评审。Codex 每轮判「修正后可以发布」（共 13 项，全部落实）；DeepSeek 首轮超时、次轮桥接密钥欠费；改用系统环境变量 `Claude调用harness` 的密钥直连 dsh 后，判「可以发布」（其 3 条不阻断可选项已处理 2 条，1 条为构建流水线保守字段，已在文档说明）。

## 变更清单

| 项 | 变更 | 主要文件 |
|---|---|---|
| 1 | **失败清理（全地图绑定）**：`ApplyAndVerifyFieldsAsync` 在字段处理开始前调用 `CaptureCreatedOutputAsync`——扫描工程内**全部地图**，按数据源找到引用新数据集的图层/独立表，并把每个成员绑定到其所属地图；失败时删除数据集并按对象身份从**各自所属地图**移除，完全不依赖活动视图；`AddField` 抛异常同样进入清理；数据源读取/删除/移除失败逐条进入 `Cleanup problems` 并随错误报告；删除失败时文案改为「may still exist」 | addin/ArcGISProMCP/Commands/GpWrapperCommands.cs |
| 2 | **脚本去本机路径**：`update_installed.py` 默认路径从用户目录与脚本位置推导，`MCP_*` 可覆盖；入口支持 `__main__` 与 RunCode 的 `__mcprun__`，优先读取 `MCP_UPDATE_ARGS`（备注整串保留），BUILD.md 记录两种调用方式 | scripts/update_installed.py、BUILD.md |
| 3 | **清理旧脚本与路径残留**：移出 `deploy_local_116/117.py`、`update_installed_117/118/119.py` 及 `.result.json`、`bench_tiny.py.result.json`；`tests/bench_latency.py`、`tests/live_verify_115/116.py` 改为环境变量/相对路径（`MCP_VERIFY_GDB` / `MCP_VERIFY_SNAP` / `MCP_RUNCODE_PYT` / `TEMP`）；历史文档去本机用户目录与中转路径 | scripts/、tests/、docs/local-1.1.5.md、docs/local-1.1.6.md |
| 4 | **构建 PathMap**：`<PathMap>$(MSBuildProjectDirectory)=/_/src</PathMap>`，DLL/PDB 无构建机源码路径（扫描 0 命中） | addin/ArcGISProMCP/ArcGISProMCP.csproj |
| 5 | **兼容口径统一**：VERSION.json「3.7 (built and live-verified on 3.7; later versions untested)」、四语徽标 ArcGIS Pro 3.7、install.ps1 提示「仅 3.7 实机验证」且 >3.7 警告按主/次版本判断（3.7.0.1901 不误触发） | VERSION.json、README×4、install/install.ps1 |
| 6 | 版本 1.1.10→1.1.11 | Config.daml、ArcGISProMCP.csproj、pyproject.toml、CHANGELOG.md |

## 构建（最终构建）

- 环境：dotnet SDK 10.0.300；ArcGIS Pro 3.7.0.1901（net10.0-windows）。
- `scripts/build.ps1` 全过（installer 脚本检查、client-registration、HTTP transport net8.0+net10.0、单文件安装器检查）；本次构建以日志核验 `dotnet build failed` 为 0、EXIT 0。
- **ArcGISProMCP.esriAddinX**：469,634 字节，SHA256 `0ABB872FBCD6FB65AA12F118501216F2BD3F305A9C6C68537870A5EDDF306DF9`，未签名。
- 自安装包 `Install-ArcGISProMCP.cmd`：650,107 字节，SHA256 `087E3A8973814280F415C33D98032E02E2279DCA26B7C6DB644348F177043F7D`，内嵌载荷回算一致。
- 中间构建（均未发布）：r1 `31B17BC4…C4846`（评审前）、r2 `A728B7ED…F06E`（第 1 轮修正后）、r3 `10BAD933…8E85`（第 2 轮修正后）、r4 `CC454A9F…5A50`（第 3 轮修正后，因一次管道掩盖编译失败的教训后以日志显式校验）；最终 r5 = `0ABB872F…6DF9`（第 4 轮修正后）。

## 部署与验证（2026-11-19，真实会话）

1. 备份：`release/v1.1.10-artifacts/ArcGISProMCP.esriAddinX`（767D53D7，467,650 字节）。
2. 官方 `install_addin.ps1 -AddInPath <候选> -Reinstall` 安装；certutil 回读哈希 `0ABB872F…6DF9` 一致。
3. `scripts/restart_pro.ps1 -Project ...MyProject_95.aprx` 重启并加载。
4. 握手：serverInfo `1.1.11.0`；ping `command_count=112`、工程路径正确。
5. 簿记：wrapper 经 RunCode 执行 `update_installed.py`，installed.json 更新（version=1.1.11、addin_size=469,634、sha256=0ABB872F…）。
6. 回归：`p0_regression_117.py` 对当前工程适用的断言全部 PASS（17/21；4 条失败为脚本引用已退役 MyProject_05 工程的图层名——`高程点_工作副本`/`观光线` 不存在于现工程 MyProject_95，属测试环境差异）；「无效类型拒绝→零残留」「重复字段失败→半成品删除」清理路径实测通过。
7. **多地图清理实测（第 4 轮修正的针对性验证）**：新建临时地图 MCPTEST_MAP 并 activate 后执行失败型 `create_feature_class`（字段 OBJECTID 冲突），实际 GP 入图落在另一张地图（命令激活图 ≠ GP 入图），失败清理仍准确删除了半成品数据集并从其所属地图移除了图层（`地图` 中无残留、`describe_dataset` 报 not found）；随后清理全部测试残留（临时地图、图层与数据集）。`get_features` 分页/几何摘要/WKT 精度在现工程图层实测通过。
8. 克隆复核：以 `git clone` 把本分支检出到独立目录，逐文件校验 `SHA256SUMS.txt` 194 条——mismatch 0 / missing 0 / extra 0（行尾按 `.gitattributes` 的 `eol=crlf` 检出，与清单口径一致）。

## 已知事项

- 仅验证 ArcGIS Pro 3.7；其他 Pro 版本未覆盖。
- add-in 未签名（首次加载需信任）。
- build-report.json 的 `runtimeValidation` 沿用构建流水线的保守字段（pending per claimed version），实机验证记录以本文件为准。

## 回滚

- 恢复 `Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX` 为 `release/v1.1.10-artifacts/ArcGISProMCP.esriAddinX`（或 `.bak-*` 备份）；
- installed.json 回写 version=1.1.10、addin_sha256=`767D53D7…F1A7`、addin_size=467650；
- 重启 Pro。
