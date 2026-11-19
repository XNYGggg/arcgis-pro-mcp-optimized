# 发布说明 · Customer 1.1.13（1.14 增强版 + 模板蒸馏 + 模板删除，构建号 1.1.13，2026-11-19）

> **🚀 发布定位**：本包是 **Customer 1.1.13（正式版）——经测试稳定，推荐下载**。在 Customer 1.1.12 的基础上为模板库补齐**删除命令**（`template_delete` / `arcgis-distill delete`），验收清理模板不再需要手工改 index/文件；其余能力与 1.1.12 完全一致（含 1.1.12 模板蒸馏与 1.1.5–1.1.11 全部修复）。本版**为当前唯一推荐版本**；v1.1.11 已下线，其 GitHub Release 与 tag 均已删除。GitHub Release `v1.1.13` 提供离线包（与本仓 `install/` 内容一致）；`main` 分支内容即本版本；v1.1.4 为历史版本（仅留档）。

## 变更摘要（相对 1.1.12）

**1.1.13（模板删除）**：

- 新增 MCP 工具 `template_delete`（catalog 第 126 个工具，Template 组第 11 个）：C# 薄封装 `arcgis-distill delete <id> [--force]`，与 CLI 单一实现不漂移。
- `delete` 删除模板的 spec、index 条目、作业单，以及**不再被其他模板引用**的去重样本图（共享图片保留，返回 `removed.image_kept_for`）。
- 安全语义：删除不存在的 id 报 `not_found`（退出码 3），重复删除安全；`ready`（已批准）模板需 `--force`（否则退出码 5）；`inbox` 不受影响。
- 测试：`tests/test_distill.py` 16/16（8 个删除用例：无共享图片删除、共享图片保留、未知 id not_found、ready 需 force、路径型 id 拒绝、孤儿索引清理、超长 id 撞名后可删除、中断删除后 id 仍保留）；`tests/test_catalog_matches_bridge.py` 4/4（CSHARP_ONLY 增补 template_delete）。
- 文档与版本：`docs/distill.md`、`skills/template-distill/SKILL.md` 增加删除一节；四语 README 工具表/能力对照/版本同步；`docs/使用指南.md`、`VERSION.json`（126 工具 / 123 命令）、Config.daml / csproj / pyproject 统一 1.1.13；`source/docs/local-1.1.13.md` 记录本轮。

## 验证（2026-11-19）

- 构建：`scripts/build.ps1` 全过；add-in 432,750 字节 SHA256 `CDBAEE33…327B`；自安装包 600,551 字节 SHA256 `9646CEF6…3BD8`。
- 单元测试：`pytest tests/` 28 passed，其中 `tests/test_distill.py` 16/16、`tests/test_catalog_matches_bridge.py` 4/4；`tools.json` ↔ `catalog.py` 126 工具全等价。
- 打包校验：SHA256SUMS 重新生成并全量校验通过；`install/runtime/distill` 与 `source/src/arcgis_agentkit` 逐文件一致。
- 实机（2026-11-19，Pro 3.7，真实终端）：安装后握手 `1.1.13.0`、`ping` 命令数 **123**、`tools/list` **126** 工具（含 `template_delete`）；经已安装 `run-distill.ps1` 跑通 import → measure → delete（重复删除 → `not_found`/exit 3，用户模板库未受影响）；官方自检 **22 通过 / 2 警告（非失败）/ 0 失败**。
- **规模**：123 命令 / 126 工具（已实机验证）。仅验证 ArcGIS Pro 3.7；add-in 未签名（首次加载需信任）。

## 回滚

- 旧版本（v1.1.4，以及已下线的 v1.1.11）均为历史版本（仅留档），本版不提供回滚指引；历史资产仅作留档查证。
- 如需反馈问题，请附 `docs/排障手册.md` 所述日志与本版信息（`VERSION.json`）。

---

## 历史发布 · Customer 1.1.12（1.14 增强版 + 模板蒸馏，构建号 1.1.12，2026-11-19）

> **🚀 发布定位**：本包是 **Customer 1.1.12**，在 Customer 1.1.11 的基础上**整体并入 2.x（v2.0.2）的模板蒸馏模块**——1.1.x 客户线首次具备 10 个 `template_*` 工具与 `arcgis-distill` 命令行；其余能力与 1.1.11 完全一致（含 1.1.5–1.1.11 全部修复）。（历史记录：当时的上一稳定版 v1.1.4），本分支是其增强构建；不创建 GitHub Release / Gitee 发行版、不上传发行资产、不打 tag；安装包仅随仓库提供（`install/`）。当时的回滚口径（已过时）。

## 变更摘要（相对 1.1.11）

**1.1.12（模板蒸馏移植，模块来源 v2.0.2-opt @ 0aa42aa）**：

- 新增 `arcgis_agentkit.distill` Python 包（2.x 原样移植，仅依赖 Pillow；用 Pro 自带 `arcgispro-py3` 运行，零额外安装）。
- 新增 10 个 MCP 工具：`template_list` / `get` / `import` / `measure` / `patch` / `validate` / `review` / `approve` / `preview` / `apply`（C# 薄封装 `TemplateCommands.cs` → `arcgis-distill` CLI，单一实现不漂移）；工具数 115→**125**，命令 112→**122**。
- 新增运行时 `%LOCALAPPDATA%\ArcGIS Pro MCP\distill\`（`arcgis_agentkit` 包 + `run-distill.ps1` 启动器）与模板库种子 `library\`；模板库实际位置 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`，**升级安装保留已有模板**。
- 新增 `skills/template-distill/SKILL.md`（语义 patch 作业指导）与 `docs/distill.md`（状态机/命令/库结构）；`tests/test_distill.py` 覆盖 import→measure→patch→validate→preview→review→approve→apply 全链。
- 2.x 的 `guard_summon` / `guard_status` **不并入**本线，弹窗守护仍为独立脚本 `03-dialog-guard.cmd`。
- 版本与文档：Config.daml / csproj / pyproject / VERSION.json 统一 1.1.12；四语 README（模块状态、能力对照、工具目录 Template 组）、`docs/使用指南.md`、`docs/CHANGELOG.md`、`source/docs/local-1.1.12.md` 同步。

## 验证（2026-11-19）

- 构建：`scripts/build.ps1` 全过（installer 脚本检查、client-registration、HTTP transport net8.0+net10.0、单文件安装器检查）；add-in **476,195 字节** SHA256 `A20E263B041E78A1F5631B1DD4EFC1B7946C05A28038751DC20C38F0AB55C6C7`；自安装包 658,962 字节 SHA256 `F1ACCDCABA406617816B7318BEBE33732968D3065332BB5FA451791A34657BB9`。
- 单元测试：`tests/test_distill.py` 7/7；`tests/test_catalog_matches_bridge.py` 4/4（新增 tools.json 一致性与非图片拒绝、force 评审完整性测试）。
- 批量蒸馏测试：**50 张**专家级地图图件（10 类，已验证 49 幅全绿；1 幅图集封面页被工具确定性拒绝）的 import/measure/validate/preview 批量回归，及三方会谈评审记录，见 `release/distill-verify-20261119/`。
- 安装口径：`install/00-install.cmd` 是完整客户安装入口（add-in + 运行时 + 模板蒸馏）；`source/dist/Install-ArcGISProMCP.cmd` 是 add-in 单文件安装器，只安装 add-in，供源码审阅与校验。
- 实机（2026-11-19，真实终端）：握手 `1.1.12.0`、`ping` 命令数 122、`tools/list` 125 工具；MCP 端 `template_*` 全流程 11/11 通过（含 preview 1.5MB PNG 与 apply context），用户库验完清空；官方自检 21 通过 / 2 警告（均非失败）/ 0 失败。
- **规模**：122 命令 / 125 工具（已实机验证）。仅验证 ArcGIS Pro 3.7；add-in 未签名（首次加载需信任）。

## 回滚

- （历史记录）旧版本（v1.1.11、v1.1.4）均为历史记录（仅留档），本段原有回滚指引已作废。

---

## 历史发布 · Customer 1.1.11（1.14 增强版，构建号 1.1.11，2026-11-19）

> **🚀 发布定位**：本包是 **Customer 1.1.11（1.14 增强版）**，在 v1.1.4 之上累计合入 1.1.5–1.1.11 的修复与增强，重点是把 2026-11-18 实机全量测试确认的 P0–P2 缺陷全部修掉，并按发布前三方评审修正了失败清理与文档口径。（历史记录：当时的上一稳定版 v1.1.4），本分支是其增强构建；本分支不创建 GitHub Release / Gitee 发行版、不上传发行资产、不打 tag；安装包仅随仓库提供（`install/`）。当时的回滚口径（已过时）。

## 变更摘要（相对 v1.1.4）

**1.1.5（F1–F10）**：环境注入贯通（`set_environment` 的 snapRaster 等真正传给 GP，支持每调用覆盖）；`describe_geoprocessing_tool` 返回枚举与默认值；`create_map` 默认无底图（`project_default` 恢复旧行为）；栅格/TIN 的缩放与符号读取；新增 `add_table`；`export_map_view` 新增 `fit:"auto"`；`set_raster_symbology` 支持显式配色/标签；`set_map_properties` 的 epsg 生效；新增 `restart_pro` 自重启命令。

**1.1.6（V-1–V-7，工具链修复）**：toolbox 路径笔误给出 "Did you mean …" 提示、命名参数报错附带可用示例；`get_pump_status` 在 Python 桥未启动时**返回结构化状态**（`running:false, bridge_listening:false`）而不再以 Failed 呈现（执行类命令仍严格报错）；错误分级（`BRIDGE_NOT_LISTENING / BRIDGE_ERROR / BRIDGE_CLOSED / BRIDGE_TIMEOUT / UNSUPPORTED`）与 `last_error_code/at`，状态查询加 5 秒读超时；`diagnose` 主动探测桥端口（`bridge_listening/probe_ms`）；工具描述修正（原生端只收位置参数、桥依赖标注）；HTTP 传输测试断言改为"去重 + 必备子集"；桥 down 结果缓存 5 秒。

**1.1.7（P0，实机测试缺陷修复）**：`create_feature_class` / `create_table` 的 `fields` 参数真正生效——创建后逐个 `AddField` 并 `describe_dataset` 回读校验（名称/类型/长度），失败即删除半成品；`run_batch` 批量执行时图片结果默认剥离为 path/bytes/sha（不再把 base64 塞进上下文）；`get_features` 新增分页（`truncated` / `next_offset`）与 `geometry_summary` / `geometry_precision`；`preview_layout` 支持 `output_path` / `return_image`。

**1.1.8（P1，实机测试缺陷修复）**：`insert_features` / `update_features` / `delete_features` 支持独立表与数据集路径（表走 Table API，图层仍走编辑会话）；`sample_raster_values` 改为原生像素采样——支持 `points` / `point_layer`、内联返回、不再产生 `raster_samples` 副作用表（`write_table` 保留旧路径）；`create_map` 的 SCENE 类型修复（`SceneLocal`）；`add_join` 修复 GP 连接后图层视图失效（JoinRegistry + 客户端连接，含候选错误提示）；布局元素 `x/y` 统一为左下角原点并通过 `GetBounds` 回读校验、越界拒绝；`run_batch` 每步带耗时与串行标注；GP 结果入图改为等待 + 重试。

**1.1.9（P2，实机测试缺陷修复）**：`get_layer_info` / `set_raster_symbology` / `repair_layer_source` 等接受数据集路径（唯一匹配自动解析，多匹配列出图层名）；`repair_layer_source` 新增栅格分支（`RasterDataset.ReplaceDataSource`）；`search_data` 结果去重；`check_extension` 语义改为 `inspectable:false`（Pro 不向 add-in 暴露检出）；GP 输出过滤空串；`get_features` 对栅格图层名给出 `get_raster_info` 提示。

**1.1.10（重启自愈与工程化收尾）**：`restart_pro` 超时自愈——保存框探测改为子进程 + 15 秒硬超时（超时放弃并记日志）、按钮匹配支持本地化 `是(Y)/否(N)/取消(C)`、restart.log 改 UTF-8 并加阶段心跳、TCP 连接 5 秒超时，仍保持"不强杀 Pro"；重启调度标志带时间戳，10 分钟后可接管（助手挂掉不再永久拒绝新请求）；部署脚本归一（`scripts/update_installed.py` 位置参数版，哈希不符拒写）；9 个实机回归脚本支持 `MCP_TEST_SCRATCH` 等环境变量覆盖。

**1.1.11（发布前评审修正）**：`create_feature_class` / `create_table` 的失败清理改为**按对象身份**——删除数据集前先按数据源解析引用它的图层与独立表，删除后从**捕获时所属的地图**移除（切换视图不会漏清），同名但异源的图层不再被误删；数据源读取异常、删除失败、移除失败逐条收集并随工具错误报告（`Cleanup problems: …`），删除失败时错误文案不再声称「已删除」；`scripts/update_installed.py` 去除本机路径硬编码，且可在 RunCode 的 `__mcprun__` 入口下运行（`MCP_UPDATE_ARGS` 或 wrapper），BUILD.md 记录两种调用方式；清理本地一次性部署脚本与历史文档中的本机路径；构建加入 `PathMap`，DLL/PDB 不再含构建机源码路径；兼容口径统一为「仅实机验证 Pro 3.7」（VERSION.json、四语徽标、安装器提示）。**第 3 轮终审补充**：清理对象在字段处理开始前即捕获（字段新增期间切换视图也不会漏清），`AddField` 抛异常同样进入清理并上报；`install.ps1` 的「高于 3.7」提示改按主/次版本判断（`3.7.0.1901` 不再误触发）；`live_verify_115/116` 的本机路径参数化（`MCP_VERIFY_GDB` / `MCP_VERIFY_SNAP`），旧一次性部署脚本移出源码树；`update_installed.py` 优先读取 `MCP_UPDATE_ARGS`，备注不再被首个空格截断。**第 4 轮终审补充**：失败清理改为扫描工程内**全部地图**并把每个图层/独立表绑定到其所属地图——清理不再依赖活动视图（实测：命令激活图与 GP 入图不一致时，失败仍能准确移除并删除半成品）；未通过的字段请求在添加前校验的行为不变。

## 验证（本机实机，2026-11-18 / 11-19）

- 构建期：client-registration、installer、http-transport（net8.0 + net10.0）全部通过。
- 1.1.7：P0 回归 21/21 PASS；1.1.8：P1 连续两轮回归 0 失败；1.1.9：P2 回归 10/10、P1 重跑 0 失败。
- 1.1.10：构建全过，安装后握手 `1.1.10.0`、ping `command_count=112`、工程路径正确。
- 1.1.11：构建全过；安装后握手 `1.1.11.0`、ping `command_count=112`；D1 字段回归（p0_regression_117）对当前测试工程适用的断言全部 PASS（4 条历史断言引用已退役 MyProject_05 工程的图层名，属测试脚本环境差异），其中「重复字段失败→半成品删除」路径实测通过；`get_features` 分页/摘要/精度在现工程图层上实机复验通过。**仅验证 ArcGIS Pro 3.7，其他 Pro 版本未覆盖。**
- 规模：112 命令 / 115 工具；add-in SHA256 `0ABB872FBCD6FB65AA12F118501216F2BD3F305A9C6C68537870A5EDDF306DF9`（469,634 字节）。

## 发布前三方评审（2026-11-19）

Codex 结论「修正后可以发布」：完整性/版本一致性/哈希口径通过；四项修正意见已落实（行尾与清单、1.1.4 措辞、失败清理身份与异常报告、脚本与二进制路径）。DeepSeek 首轮 600 秒超时未回，第二轮复核见归档。评审材料与收敛稿见 `agent_bridge/docs/customer-v1.1.11客户包发布前评审-*`。

## 本包的定位与边界

- 面向客户端的完整离线包（安装脚本、运行时、客户端接入、文档、源码快照与构建产物），与 v1.1.4 客户版同构。
- 不创建 Release、不上传发行资产、不打 tag；默认分支保持 v1.1.4。
- 作为 1.14 增强版使用；遇问题请按 `docs/排障手册.md` 反馈（旧版本回滚指引已过时）。

---

## 历史：Customer 1.1.10（1.14 增强版，构建号 1.1.10）发布说明（2026-11-19，未单独发布）

> 1.1.10 的变更已包含在上面的 1.1.11 中（1.1.11 = 1.1.10 + 发布前评审修正），未单独成包发布。add-in 构建 SHA256 `767D53D75DA6D2BCDF440BBA6E7BDE731F96092602EA8CAFE578AB2B1698F1A7`（467,650 字节）。

---

## 历史：Customer 1.16（1.14 增强版，构建号 1.1.6）发布说明（2026-11-18）

> **🚀 发布定位**：本包是 **Customer 1.16（1.14 增强版）**，在 v1.1.4 之上合入 1.1.5 / 1.1.6 的修复与增强；**1.1.4 正式发布仍在准备中，敬请期待**。本分支不创建 GitHub Release / Gitee 发行版、不上传发行资产、不打 tag；安装包仅随仓库提供（`install/`）。当时的回滚口径（已过时）。

### 变更摘要（相对 v1.1.4）

**1.1.5（F1–F10）**：环境注入贯通（`set_environment` 的 snapRaster 等真正传给 GP，支持每调用覆盖）；`describe_geoprocessing_tool` 返回枚举与默认值；`create_map` 默认无底图（`project_default` 恢复旧行为）；栅格/TIN 的缩放与符号读取；新增 `add_table`；`export_map_view` 新增 `fit:"auto"`；`set_raster_symbology` 支持显式配色/标签；`set_map_properties` 的 epsg 生效；新增 `restart_pro` 自重启命令。

**1.1.6（V-1–V-7，工具链修复）**：toolbox 路径笔误给出 "Did you mean …" 提示、命名参数报错附带可用示例；`get_pump_status` 在 Python 桥未启动时**返回结构化状态**（`running:false, bridge_listening:false`）而不再以 Failed 呈现（执行类命令仍严格报错）；错误分级（`BRIDGE_NOT_LISTENING / BRIDGE_ERROR / BRIDGE_CLOSED / BRIDGE_TIMEOUT / UNSUPPORTED`）与 `last_error_code/at`，状态查询加 5 秒读超时；`diagnose` 主动探测桥端口（`bridge_listening/probe_ms`）；工具描述修正（原生端只收位置参数、桥依赖标注）；HTTP 传输测试断言改为"去重 + 必备子集"；桥 down 结果缓存 5 秒。

### 验证（本机实机，2026-11-18）

- 构建期：client-registration、installer、http-transport（net8.0 + net10.0）全部通过。
- 实机：全量回归 26/26；专项 10/10；桥状态桩 4/4；真实 Python 桥三态（桥停 / 桥起泵停 / 桥泵全起）全部通过。
- 规模：112 命令 / 115 工具；add-in SHA256 `96C7829E2A26E16C270722A84605F9945DA2348DFCF202C658A64EA9E012E625`（444,124 字节）。

### 本包的定位与边界

- 面向客户端的完整离线包（安装脚本、运行时、客户端接入、文档、源码快照与构建产物），与 v1.1.4 客户版同构。
- 不创建 Release、不上传发行资产、不打 tag；默认分支保持 v1.1.4。
- 作为 1.14 增强版使用；遇问题请按 `docs/排障手册.md` 反馈（旧版本回滚指引已过时）。

---

## 历史：客户版 v1.1.4 发布说明（2026-11-16）

> **📌 文档修订（2026-11-16 晚）**：系统要求更新为 **Windows 11 x64（仅 x64 / AMD64，不支持 ARM64）**；本包二进制与运行脚本零改动，仅文档（README / 本说明 / VERSION.json）与包内 `SHA256SUMS.txt` 更新。

## 三方会谈评审结论

发布前由 Claude 主持，Codex 与 DeepSeek 参与评审（2026-11-16，议题：客户版发布前审查）。
两位评审均给出“修正后可发布”的意见，共识问题与处理如下。

## 已采纳并修复（本轮）

| # | 评审问题 | 处理 |
|---|---|---|
| 1 | `dialog-guard.ps1` 把含空格的守护路径直接传入 `Start-Process -ArgumentList`，缺少命令行引号（Codex） | 已改为带引号的命令行字符串拼接 |
| 2 | 安装/升级先删除再复制，无备份无回滚（两位评审一致） | add-in 与运行时均改为“暂存 → 校验 → 原子替换 → 失败自动回滚”，旧版本保留 `.bak-<时间戳>` |
| 3 | 卸载直接删除 add-in 与运行时目录，未核验归属（两位一致） | 依据 `installed.json` 与 add-in 哈希确认归属；默认移入 `.uninstalled-<时间戳>` 备份，`-Purge` 才删除 |
| 4 | Claude 配置解析失败时会重建 JSON（Codex/DeepSeek） | 改为 fail-closed：解析失败立即停止并保留原文件，仅提示手工修复 |
| 5 | Claude Code 配置未检查 `$LASTEXITCODE`（Codex） | `remove`/`add` 均检查退出码并给出可操作的失败提示 |
| 6 | 缺少环境预检：.NET 10、端口占用、文档目录重定向（两位一致） | 安装与自检均新增：.NET Desktop Runtime 10 检测、6520/6510/6511 占用及占用进程、Documents/OneDrive 提示 |
| 7 | 包根缺少 LICENSE/NOTICE（DeepSeek，AGPL §5） | 已在包根加入 `LICENSE` 与 `NOTICE`（含上游署名与本包修改声明） |
| 8 | `source/dist/` 内上游 1.1.1 二进制可能被误用（DeepSeek） | 增加 `source/dist/README.md` 明确标注“非本客户版安装包” |
| 9 | `pyproject.toml=1.1.1` 与交付物 1.1.4 冲突（Codex） | 源码快照内版本号对齐为 1.1.4，描述同步为 113 工具 |
| 10 | README 系统要求不准确（Codex） | 明确 Windows 11 x64（仅 x64，不支持 ARM64）以 Pro 3.7 官方支持为准；补充 .NET Desktop Runtime 10 与 WebView2 由 Pro 受支持环境提供；补充 AGPL §13 网络条款说明 |

## 有意保留（不视为缺陷）

- **弹窗守护默认自动点击“是/允许”**：这是用户既有要求（2026-11-09）的核心行为，不改为默认仅提醒；`-AlertOnly` 可切换为仅提醒。更严格的模态框定位与负向测试套件列入下一版。
- **add-in 未签名**：签名需要商业证书，本版保持未签名并在 README/排障中说明首次信任流程。

## 未完成事项（下一版清单）

1. 干净 Windows 虚拟机按 README 全流程演练（安装 → 客户端握手 → 升级 → 卸载恢复）。
2. 含空格/中文路径的完整回归用例（本轮已修守护路径引号，其余场景待覆盖）。
3. 弹窗守护的负向测试套件（覆盖框、删除框必须只提醒不点击）。
4. add-in 代码签名（如取得证书）。

## 校验

- 全部文件 SHA256 见 `SHA256SUMS.txt`；add-in 哈希见 `VERSION.json` 与 `install/addin.sha256`。
- 本机验证：`install/00-install.cmd` 安装成功；`01-check.cmd` 11 通过 / 2 警告 / 0 失败（警告为 ArcGIS Pro 未启动时的端口状态，属预期）；PowerShell 语法解析全部通过。
