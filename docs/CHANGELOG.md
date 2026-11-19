# 更新日志

## Customer 1.1.13（1.14 增强版 + 模板蒸馏 + 模板删除，构建号 1.1.13，2026-11-19）

- 相对 1.1.12：模板库补齐**删除命令** `template_delete`（`arcgis-distill delete <id> [--force]`）。
  - 删除内容：spec、index 条目、作业单，以及不再被其他模板引用的去重样本图（共享图片保留，返回 `removed.image_kept_for`）。
  - 安全语义：删除不存在的 id 返回 `not_found`（退出码 3），重复删除安全；`ready`（已批准）模板需 `--force`（否则退出码 5）；`inbox` 不受影响。
  - C# 薄封装 `template_delete`（TemplateCommands.cs）+ catalog Template 组条目；`tests/test_distill.py` 11/11（新增 4 个删除用例）、`tests/test_catalog_matches_bridge.py` 4/4；工具数 125→126、命令 122→123。
  - 文档：`docs/distill.md`、`skills/template-distill/SKILL.md` 增加删除一节；四语 README、`docs/使用指南.md`、`VERSION.json` 同步。
  - 构建：全过；add-in 476,408 字节 SHA256 `4B90BB43…3D7419`；自安装包 659,248 字节 SHA256 `B1E4922F…3A07F`。
  - 验证：pytest 全绿；`tools.json` 与 `catalog.py` 全等价（126 工具）；SHA256SUMS 重新生成并全量校验。

## Customer 1.1.12（1.14 增强版 + 模板蒸馏，构建号 1.1.12，2026-11-19）

- 相对 1.1.11：**整体并入 2.x（v2.0.2）模板蒸馏模块**（1.1.x 客户线首次具备）。
  - 新增 `arcgis_agentkit.distill` Python 包（仅依赖 Pillow，用 Pro 自带 Python 运行）+ 运行时 `distill\` 目录与 `run-distill.ps1` 启动器；模板库 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`（升级保留）。
  - 新增 10 个 MCP 工具：`template_list / get / import / measure / patch / validate / review / approve / preview / apply`（C# 薄封装 arcgis-distill CLI）；工具数 115→125、命令 112→122。
  - 新增 `skills/template-distill/SKILL.md`、`docs/distill.md`、`tests/test_distill.py`；`test_catalog_matches_bridge.py` 将 template_* 记为 C# 原生实现。
  - 2.x 的 `guard_summon / guard_status` 不并入；弹窗守护仍为独立脚本 `03-dialog-guard.cmd`。
  - 构建：全过；add-in 476,195 字节 SHA256 `A20E263B…C6C7`；自安装包 658,962 字节 SHA256 `F1ACCDCA…7BB9`。
  - 验证：`test_distill.py` 7/7、`test_catalog_matches_bridge.py` 4/4；50 张专家级地图图件的批量蒸馏回归（49 幅全绿，1 幅封面页被正确拒绝）与三方评审见 `release/distill-verify-20261119/`。
  - 实机：握手 `1.1.12.0`、`ping` 122、`tools/list` 125；MCP `template_*` 全流程 11/11 通过；官方自检 21 通过 / 0 失败。

## Customer 1.1.11（1.14 增强版，构建号 1.1.11，2026-11-19）

- 相对 v1.1.4 累计合入 1.1.5 的 F1–F10、1.1.6 的 V-1–V-7，以及 2026-11-18 实机全量测试确认的 P0–P2 缺陷修复：
  - 1.1.7（P0）：`create_feature_class` / `create_table` 的 `fields` 真正生效（AddField + describe 回读校验、失败清理）；`run_batch` 图片默认剥离为 path/bytes/sha；`get_features` 分页与几何摘要/精度；`preview_layout` 支持 `output_path`。
  - 1.1.8（P1）：独立表/数据集路径的增改删；`sample_raster_values` 原生像素采样（无副作用表）；`create_map` SCENE 修复；`add_join` 连接注册表修复；布局元素左下角坐标与回读校验；`run_batch` 每步耗时；GP 入图等待重试。
  - 1.1.9（P2）：图层命令接受数据集路径；`repair_layer_source` 支持栅格；`search_data` 去重；`check_extension` 语义修正；GP 输出过滤；栅格图层名提示。
  - 1.1.10：`restart_pro` 超时自愈（15 秒硬超时、本地化按钮、UTF-8 心跳日志、5 秒 TCP 超时）；重启调度标志 10 分钟过期；部署脚本归一。
  - 1.1.11（发布前评审修正）：失败清理改为按对象身份、从捕获时所属地图移除并逐条上报；`update_installed.py` 去本机路径并支持 RunCode `__mcprun__` 入口；清理旧部署脚本与历史文档本机路径；构建 PathMap 剔除 DLL/PDB 源码路径；兼容口径统一为仅实机验证 Pro 3.7；第 3 轮终审后清理在字段处理前捕获、AddField 异常入清理、版本提示按主/次版本判断、live_verify 路径参数化、登记脚本优先读 MCP_UPDATE_ARGS；失败清理扫描全部地图并按所属地图移除（不依赖活动视图）。
- 规模：112 命令 / 115 工具；add-in SHA256 `0ABB872F…6DF9`（469,634 字节）。
- 未创建 Release：不上传发行资产、不打 tag；默认分支保持 v1.1.4。

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
