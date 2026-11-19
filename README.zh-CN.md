[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![日本語](https://img.shields.io/badge/Language-%E6%97%A5%E6%9C%AC%E8%AA%9E-bc002d?style=flat-square)](README.ja.md) [![한국어](https://img.shields.io/badge/Language-%ED%95%9C%EA%B5%AD%EC%96%B4-0047a0?style=flat-square)](README.ko.md) [![Version](https://img.shields.io/badge/Version-Customer%201.1.13-2ea44f?style=flat-square)](VERSION.json) [![Changelog](https://img.shields.io/badge/Docs-Changelog-6f42c1?style=flat-square)](docs/CHANGELOG.md) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011%20x64-lightgrey?style=flat-square)]()

# 🗺️ ArcGIS Pro MCP 优化版 · Customer 1.1.13（1.14 增强版 + 模板蒸馏）🚀

用一句自然语言，让 AI 助手直接操作你正开着的 ArcGIS Pro 工程：加图层、改符号、查属性、跑地理处理、出图导出。数据不出本机，所有通信都走 `127.0.0.1` 🔒。

> **🚀 本版为 Customer 1.1.13（1.14 增强版 + 模板蒸馏 + 模板删除，构建号 1.1.13）——正式版：经测试稳定，推荐下载**。在 **v1.1.4** 之上累计合入 1.14 之后的全部修复：1.1.5（F1–F10）、1.1.6（V-1–V-7）、2026-11-18 实机全量测试确认的 P0–P2 缺陷修复（1.1.7–1.1.9）、1.1.10 的重启自愈、1.1.11 的清理修复、1.1.12 从 2.x 并入的模板蒸馏模块，以及 1.1.13 的模板删除（template_delete）。**GitHub Release `v1.1.13` 提供离线包**（与本仓 `install/` 内容一致）；**v1.1.11 已下线（发行版与 tag 均已删除），由本版接替成为推荐下载**；v1.1.4 保留为上一稳定版供回滚。

> **📌 版本说明（2026-11-19 更新）**：**Customer 1.1.13 是 1.x 稳定线的当前正式版——经测试稳定，推荐日常使用**；v1.1.11 已下线（发行版与 tag 均已删除），由 1.1.13 完全接替；v1.1.4 保留为上一稳定版供回滚。**2.0 线为测试版**——优化尚未完成，其 GitHub Release 资产与 tag 已移除，暂不可下载；2.0 代码仅保留在 `customer-v2.0.1` / `customer-v2.0.0` 分支中。

> **🙏 上游项目与原作者**：本客户版基于 **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**（原作者 **Pisut Nakmuenwai**，AGPL-3.0-or-later），在其基础上累计本地补丁至 **v1.1.13**。完整署名与修改声明见包根 `NOTICE` 与 `source/SOURCE-NOTICE.md`。

本客户版把运行所需的**全部组件**打包在一起，离线可用，**不需要安装 Python、不需要 pip、不需要联网**，除 ArcGIS Pro 本身外没有任何额外依赖。电池都给你装好了 🔋

---

## 🆚 相对 1.14（v1.1.4）升级了什么

以下全部是 v1.1.4 客户包之后合入的变更。工具数 113 → **126**（1.1.5 新增 `add_table` 与 `restart_pro`；1.1.12 并入 2.x 模板蒸馏，新增 10 个 `template_*` 工具；1.1.13 新增 `template_delete`）；add-in 二进制哈希 ``669E1B74…8E99``（432,751 字节）。

| 构建 | 类型 | 升级要点 |
|---|---|---|
| **1.1.5** | F1–F10 | `set_environment` 真正传给 GP（支持每调用覆盖）；`describe_geoprocessing_tool` 返回枚举与默认值；`create_map` 默认不加底图（`project_default` 恢复旧行为）；栅格/TIN 缩放与符号读取；**新增 `add_table`**；`export_map_view` 支持 `fit:"auto"`；`set_raster_symbology` 支持显式配色/标签；`set_map_properties` 的 epsg 生效；**新增 `restart_pro`** |
| **1.1.6** | V-1–V-7 | toolbox 路径笔误给出 "Did you mean …" 提示；命名参数报错附带可用示例；`get_pump_status` 在 Python 桥未启动时**返回结构化状态**（`running:false, bridge_listening:false`）而不再报失败；错误分级（`BRIDGE_NOT_LISTENING / BRIDGE_ERROR / BRIDGE_CLOSED / BRIDGE_TIMEOUT / UNSUPPORTED`）与 `last_error_code/at`；状态查询 5 秒读超时；`diagnose` 主动探测 6511 端口（`bridge_listening` / `probe_ms`）；工具描述修正（原生端只收位置参数、桥依赖标注）；HTTP 传输断言加固；桥 down 结果缓存 5 秒 |
| **1.1.7** | P0 | `create_feature_class` / `create_table` 的 `fields` **真正生效**——创建后逐个 `AddField` 并 `describe_dataset` 回读校验（名称/类型/长度），失败即删除半成品；`run_batch` 图片默认剥离为 path/bytes/sha（不再把 base64 塞进上下文）；`get_features` 分页（`truncated` / `next_offset`）与 `geometry_summary` / `geometry_precision`；`preview_layout` 支持 `output_path` / `return_image` |
| **1.1.8** | P1 | `insert_features` / `update_features` / `delete_features` 支持独立表与数据集路径（表走 Table API，图层仍走编辑会话）；`sample_raster_values` 原生像素采样——支持 `points` / `point_layer`、内联返回、不再产生 `raster_samples` 副作用表（`write_table` 保留旧路径）；`create_map` SCENE 修复（`SceneLocal`）；`add_join` 修复 GP 连接后图层视图失效（JoinRegistry + 客户端连接，含候选错误提示）；布局元素 `x/y` 统一左下角原点、`GetBounds` 回读校验、越界拒绝；`run_batch` 每步耗时与串行标注；GP 结果入图等待重试 |
| **1.1.9** | P2 | `get_layer_info` / `set_raster_symbology` / `repair_layer_source` 接受数据集路径（唯一匹配自动解析，多匹配列出图层名）；`repair_layer_source` 新增栅格分支（`RasterDataset.ReplaceDataSource`）；`search_data` 去重；`check_extension` 语义修正（`inspectable:false`，Pro 不向 add-in 暴露检出）；GP 输出过滤空串；`get_features` 对栅格图层名给出 `get_raster_info` 提示 |
| **1.1.10** | 重启自愈 | `restart_pro` 超时自愈——保存框探测改为子进程 + 15 秒硬超时（超时放弃并记日志）；按钮匹配支持本地化 `是(Y) / 否(N) / 取消(C)`；`restart.log` 改 UTF-8 并加阶段心跳；TCP 连接 5 秒超时；仍保持"不强杀 Pro"。重启调度标志 10 分钟后可接管（助手挂掉不再永久拒绝新请求）。部署归一为 `scripts/update_installed.py`（位置参数、哈希不符拒写） |
| **1.1.11** | 清理修复 | `create_feature_class` / `create_table` 失败清理改为：删除数据集前先按数据源解析引用它的图层与独立表，删除后按对象身份移除——同名但异源的图层不再被误删；清理失败随工具错误一并报告（不再吞异常）。`scripts/update_installed.py` 去除本机路径硬编码；构建把源码路径从 DLL/PDB 中映射剔除 |
| **1.1.12** | 模板蒸馏 | 并入 2.x 模板蒸馏模块：10 个 `template_*` 工具 + `arcgis-distill` CLI（Pro 自带 Python，零额外依赖）+ 本地模板库（`%LOCALAPPDATA%\ArcGIS Pro MCP\library`，升级保留）；新增 `skills/template-distill` 与 `docs/distill.md`；工具数 115→125、命令 112→122 |
| **1.1.13** | 模板删除 | 模板库新增 `template_delete`——删除模板的 spec、索引条目、作业单与去重样本图（仍被其他模板引用时保留）；已批准（ready）模板需 `force`；工具数 125→126、命令 122→123 |

本机实机验证（**仅 ArcGIS Pro 3.7**）：1.1.7 回归 21/21 PASS；1.1.8 连续两轮回归 0 失败；1.1.9 P2 回归 10/10、P1 重跑 0 失败；1.1.10 握手 `1.1.10.0`；1.1.11 握手 `1.1.11.0`、ping `command_count=112`，D1 字段回归对当前测试工程适用的断言全部通过（4 条历史断言引用已退役工程的图层名），`get_features` 分页/摘要/精度已实机复验。其他 Pro 版本未覆盖。

1.1.13（模板删除）：构建全过（client-registration / installer / HTTP net8.0+net10.0），`tests/test_distill.py` 13/13（6 个删除用例：无共享图片删除、共享图片保留、未知 id not_found、ready 需 force、路径型 id 拒绝、孤儿索引清理）、`tests/test_catalog_matches_bridge.py` 4/4 通过；`tools.json` 与 `catalog.py` 在 126 工具 / 123 命令上全等价。1.1.12 的实机验收记录（握手、125 工具、`template_*` 全流程 11/11、官方自检 21 通过 / 0 失败）见 `docs/CHANGELOG.md`。细节见 `docs/CHANGELOG.md` 与 `RELEASE-NOTES.md`。 1.1.13（模板删除）实机验收（2026-11-19，Pro 3.7，真实终端）：安装后的构建握手 `1.1.13.0`、`ping` 命令数 **123**、`tools/list` 共 **126** 工具（含 `template_delete`）；经已安装的 `run-distill.ps1` 跑通 import → measure → delete 全链路（重复删除 → `not_found`/exit 3），官方自检 **22 通过 / 0 失败**。 1.1.13（模板删除）实机验收（2026-11-19，Pro 3.7，真实终端）：安装后的构建握手 `1.1.13.0`、`ping` 命令数 **123**、`tools/list` 共 **126** 工具（含 `template_delete`）；经已安装的 `run-distill.ps1` 跑通 import → measure → delete 全链路（重复删除 → `not_found`/exit 3），官方自检 **22 通过 / 0 失败**。

---

## 🖥️ 系统要求

| 项目 | 要求 |
|---|---|
| 🪟 操作系统 | **Windows 11 x64**（64 位 Home / Pro / Enterprise，ArcGIS Pro 3.7 官方支持）。**架构：仅 x64（AMD64），不支持 ARM64** |
| 🗺️ ArcGIS Pro | **3.7**（基于 Pro 3.7 / .NET 10 构建；仅实机验证 3.7，更高版本未测试） |
| 🔑 权限 | 普通用户即可，安装**不需要管理员** |
| 📦 其他依赖 | 无（PowerShell 与 .NET 由 Windows / Pro 自带） |

---

## 🚀 5 分钟上手

1. 📂 **解压**本包到任意目录（例如 `D:\ArcGIS-Pro-MCP-客户版`）。
2. 🛠️ **关闭 ArcGIS Pro**，双击 `install\00-install.cmd`。脚本会自动安装 add-in、部署运行时、并尝试配置 Claude Desktop / Claude Code。
3. 🖥️ **打开 ArcGIS Pro**。首次加载时如提示“加载项不受信任”，点“是 / 信任”（放心点 😄）。在 Pro 顶部找到 **MCP 选项卡**，确认状态为 **running**（端口 `127.0.0.1:6520`）。
4. 🔌 **接入你的 AI 客户端**（见下方“配置方法”），然后双击 `install\01-check.cmd` 自检，全部 `[OK]` 即可使用 (๑•̀ㅂ•́)و✧

> 自检时若 ArcGIS Pro 没开，端口检查会显示警告——先打开 Pro 再跑一次即可 👌

---

## 🎛️ 配置方法 —— 接入你的 AI 客户端

**前提**：ArcGIS Pro 已打开，**MCP 选项卡**显示 **running**。所有端口只绑定本机：

| 端口 | 用途 |
|---|---|
| `6520` | MCP over HTTP —— 所有 AI 客户端都连这里 |
| `6510` | 旧版 TCP 桥（现代客户端不用） |
| `6511` | 可选 Pro 内 Python 桥（`execute_arcpy_code`） |

### 🟣 Claude Code

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
claude mcp list
```

手工等价配置（`~/.claude.json`）：

```json
{ "mcpServers": { "arcgis": { "type": "http", "url": "http://127.0.0.1:6520/mcp" } } }
```

移除用 `claude mcp remove arcgis`，再加回用上面的命令。

### 🖥️ Claude Desktop（使用自带的 stdio 桥）

Claude Desktop 不能直连 HTTP，只能启动本地进程——用包内纯 PowerShell 的桥（无需 Python）：

1. **完全退出 Claude Desktop**（含托盘图标）。
2. 运行 `install\02-configure-claude.cmd`，或手工编辑配置（请先备份）。
3. 配置文件位于以下目录中“已存在”的第一个：
   `%APPDATA%\Claude`、`%APPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude` —— 文件名 `claude_desktop_config.json`。

```json
{
  "mcpServers": {
    "arcgis": {
      "command": "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
      "args": ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File",
               "C:\\Users\\<你的用户名>\\AppData\\Local\\ArcGIS Pro MCP\\stdio-bridge.ps1"]
    }
  }
}
```

4. 重新打开 Claude Desktop，工具列表里出现 `arcgis` 即成功。

### 🟢 Codex

在 `~/.codex/config.toml` 中加入（如已有旧 `arcgis` 段，整段替换，包括旧的 `env` 子表）：

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

保存后重启 Codex。

### 🔧 其他 MCP 客户端

以 Streamable HTTP 连接 `http://127.0.0.1:6520/mcp`，或用上面的通用 JSON。（Antigravity 用 `serverUrl` 且不要 `type`。）

### ✅ 验证连接

- `install\01-check.cmd` —— 检查 Pro 版本、add-in 哈希、运行时、端口与客户端配置。
- `netstat -ano | findstr 6520` —— 确认端口在监听。
- 日志：`%LOCALAPPDATA%\ArcGIS-MCP\addin.log`。

---

## 🎯 使用方法

**基本循环**：打开有数据的工程 → 说出你的需求 → 助手自动选择工具 → 查看结果（图片会直接内联返回）🎨

| 你可以说 | 实际发生什么 |
|---|---|
| “按面积给区县做分级色彩，浅红到深绿” | `set_layer_renderer`（分级色彩） |
| “选出人口大于 50 万的县并导出成新要素类” | `select_features` → `export_features` |
| “给道路做 500 米缓冲区” | `run_geoprocessing_tool`（Buffer） |
| “统计每个乡镇的耕地面积” | `summarize_features` / `zonal_statistics` |
| “把当前地图视图截给我看” | `export_map_view`（返回 PNG） |
| “出一张 A3 竖版 PDF，带标题、图例、比例尺、指北针” | `create_layout` → `add_map_frame` → `add_layout_*` → `export_layout` |
| “把 12 个图层按同一套符号批量导出” | `run_batch` 或 `execute_arcpy_code` |
| “新建一个含 NAME、AREA_HA、CODE 字段的空要素类” | `create_feature_class`（字段真正创建并回读校验） |

**按工具名调用**：下表中的每个工具都可以被客户端按名字直接调用；`run_batch` 可在一次往返里串起多条命令；没有专用工具时用 `execute_arcpy_code` 兜底。

**编辑规则**：编辑在 `save_edits` 之前一直挂起，挂起期间数据被锁定（删除/覆盖会失败）；`discard_edits` 丢弃挂起编辑。`delete_features` / `delete_dataset` 不可撤销——先让助手说明影响范围再执行。

**批量与提速**：重复度高、数据量大时，让助手用 RunCode 写一个幂等 arcpy 脚本一次执行；大工程按范围/OID 分块并留检查点。`run_batch` 里图片结果会自动降为 path/bytes/sha，不再刷屏。

---

## 🧩 三个模块：作用与使用方法（附提示词写法）

本项目按"一仓三模块"设计，三个模块各司其职、可以独立使用，实际使用中通常配合出场：

| 模块 | 作用（一句话） | 什么时候用 | 本版状态 |
|---|---|---|---|
| 🧩 **MCP 插件** | AI 操作 Pro 的唯一干道：126 个 MCP 工具 + RunCode / Python 桥两条 arcpy 通道 | 所有日常操作；批量脚本；需要 `CURRENT` 的交互 | ✅ 装好即用 |
| 🛡️ **弹窗守护** | 自动点击"第三方代码库"信任框，点不到就置顶响铃提醒 | 第一次跑 RunCode.pyt、无人值守批处理 | ✅ 独立脚本 `03-dialog-guard.cmd` |
| 🧪 **模板蒸馏** | 把一张样图蒸馏成可复用版式模板，批量出图版式统一 | 多张图套同一版式、复现论文/汇报图 | ✅ 已包含（自 1.1.12，移植自 2.x） |

### 🧩 模块一：MCP 插件（核心）

- **作用**：AI 与 Pro 之间的唯一通道——加图层、改符号、跑地理处理、出图导出都走它。arcpy 执行有两条通道：**RunCode 工具箱**适合一次性 / 批量脚本；**Python 桥**适合跨调用保持变量、需要 `CURRENT` 的交互式会话。
- **使用方法**：
  1. 安装后打开 Pro，确认顶部 **MCP 选项卡** 为 **running**（`127.0.0.1:6520`）；
  2. RunCode：让助手调用 `run_python_toolbox_tool` 时传入路径 `%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`；
  3. Python 桥（可选）：Pro 菜单 **分析 → Python**，粘贴桥的启动代码（端口 6511；需要 `CURRENT` 时再加一行 `start_pump()`，详见下文《RunCode 与 Python 桥怎么用》）。
- **提示词写法**：说清 **对象 + 操作 + 输出**；批量任务加一句"先做一条给我确认"。
  - ✅ "把 `区县边界` 按 `AREA` 自然断点分 5 级，浅红到深绿，图例放右下角，导出 A3 横向 PDF 到 `D:\out\`，300 dpi。"
  - ✅ "用 RunCode 写一个幂等脚本：打开所有图层的标注，字段 `NAME`、字号 10；已经打开的跳过。"
  - ✅ "这段 arcpy 需要 `CURRENT`：把桥的启动代码按我的用户名填好发我，我自己粘贴到 Pro。"
  - ❌ "帮我处理一下数据。"——没有对象、操作和输出，助手只能反复追问。

### 🛡️ 模块二：弹窗守护（跑 RunCode 才需要）

- **作用**：Pro 第一次运行第三方 `.pyt` 会弹"第三方代码库"信任框，无人值守时任务会卡住。守护脚本只点这一类"是(Y)"；点不到就置顶提醒（至少 30 秒、每 10 秒响铃）；覆盖 / 删除等其他确认框**绝不触碰**。
- **使用方法**（详见下文《弹窗守护怎么用》）：
  ```text
  install\03-dialog-guard.cmd                    # 默认 600 秒
  install\03-dialog-guard.cmd -DurationSeconds 120
  install\03-dialog-guard.cmd -AlertOnly         # 只提醒，不自动点
  ```
  每个 Pro 进程首次信任后不再弹；重启 Pro 后需要再守护一次。
- **提示词写法**：把"启动守护"和"要跑什么"说在同一句里。
  - ✅ "我要跑 RunCode.pyt：先在后台启动弹窗守护 600 秒，信任框自动点『是』，点不到就置顶响铃提醒我。"
  - ✅ "这次只提醒、不自动点。"
  - ❌ "帮我点掉所有弹窗。"——守护只处理信任框，其他确认框必须由你拍板。

### 🧪 模块三：模板蒸馏（自 1.1.12 起已包含，移植自 2.x）

- **作用**：把一张样本成图蒸馏成可复用的版式模板；之后批量出图直接套模板，多张图版式统一、可复现。
- **使用方法**：本版**已包含**（1.1.12 起，模块移植自 2.x）：直接对助手说“把这张图蒸馏成模板”即可；助手按 `import → measure → patch → validate → preview → review → approve → apply` 流程调用 11 个 `template_*` 工具，确定性步骤由 `arcgis-distill` 完成，语义字段由助手按 `skills/template-distill/SKILL.md` 撰写；模板库在 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`，升级安装会保留。
- **提示词写法**：给模板起名，并把流程和确认点写进去。
- **清理模板**：`template_delete`（或 `arcgis-distill delete <id> [--force]`）删除模板——spec、索引条目、作业单，以及不再被其他模板引用的样本图；已批准（ready）模板需 `force`。
  - ✅ "把 `D:\samples\fig3.png` 蒸馏成模板，命名『研究区总览图』：import → measure → patch → validate → preview，预览先发我确认，确认后再 review + approve。"
  - ✅ "用『研究区总览图』模板给 `maps` 里每张地图出 A3 PDF，标题用地图名，输出到 `D:\out\maps\`。"
  - ❌ "照着这张图做。"——没说模板名、流程和确认点，版式无法复现。

### 🧭 提示词通用写法（三个模块都适用）

1. **点名模块**：普通操作不用点名（默认走 MCP）；要跑脚本说"用 **RunCode**"；要持久会话说"用 **Python 桥**"；无人值守先"启动**弹窗守护**"；要统一版式说"用**模板**"。
2. **对象写具体**：图层名、字段名、路径用反引号原样写出，不要用"那个图层""刚才的数据"。
3. **输出定死**：格式（PNG / PDF / 新要素类）、尺寸或分辨率、保存路径。
4. **留确认点**：批量、覆盖、删除类操作先"做一条给我看"或"先说明影响范围"。

> 出图类提示词的完整四要素与模板，见下一节《出图制图怎么用（提示词指南）》。

---

## 🎨 出图制图怎么用（提示词指南）

这一节把"怎么跟 AI 说"讲透：不需要记工具名，按下面四要素交代，助手会自动拆成 MCP 工具调用。

### 说清四件事，一次做对

| 要素 | 要交代什么 | 示例 |
|---|---|---|
| 数据 | 图层名、数据路径、当前选择集 | "当前地图里的 `区县边界` 图层" |
| 操作 | 分级字段/方法、缓冲距离、统计口径 | "按 `AREA` 分 5 级，自然断点" |
| 风格 | 配色、字号、图例/比例尺/指北针位置、底图 | "浅红到深绿，图例放右下角" |
| 输出 | 图片/PDF/新要素类，分辨率与保存路径 | "A3 横向 PDF，300 dpi，存到 `D:\out\`" |

**好提示词 vs 坏提示词**
- ❌ "帮我美化一下地图" —— 没有目标，助手只能猜。
- ✅ "把 `土地利用` 按 `DLMC` 做唯一值符号化，图例放右下角，然后导出一张 A3 横向 PDF 到 `D:\out\landuse.pdf`，300 dpi。"
- ❌ "出个图" —— 没说范围、版式、输出。
- ✅ "以 `研究区` 图层为范围，比例尺 1:10000，出一张 A4 纵向 PNG，标题‘研究区土地利用现状’，带图例、比例尺、指北针，先发预览图给我确认。"

### 各工具组提示词速查

| 模块（工具组） | 你可以这样说 | 助手会用 |
|---|---|---|
| 会话与工程 | "先 `diagnose` 看看连接和当前工程；然后把工程另存到 `D:\proj\成果.aprx`" | diagnose / save_project |
| 图层 | "把 `roads` 移进 `底图` 组，透明度 40%，只显示 1:50000 以近；`POI` 图层关掉" | move_layer / set_layer_transparency / set_layer_scale_range / set_layer_visibility |
| 数据与属性 | "读一下 `parcels` 的前 20 行，按面积降序；统计 `DLMC` 各类面积" | get_features / summarize_features |
| 选择 | "选出 `POP` 大于 50 万的县，导出成 `D:\out\counties_big.shp`；再按位置选出落在 `研究区` 内的地块" | select_features / select_by_location / export_features |
| 字段与数据集 | "给 `parcels` 加 DOUBLE 字段 `AREA_HA`，用 `!Shape_Area!/10000` 计算；新建 EPSG:4547 的面要素类 `成果_2026`" | add_field / calculate_field / create_feature_class |
| 地理处理 | "道路做 500 米缓冲并融合；用 `研究区` 掩膜裁剪土地利用栅格" | run_geoprocessing_tool / run_batch |
| 符号化 | "按面积分 5 级、自然断点、浅红到深绿；或按 `TYPE` 唯一值配色" | set_layer_renderer |
| 地图视图 | "缩放到 `研究区`，比例尺 1:10000，截一张 1600×900 的 PNG 给我" | set_map_view / zoom_to_layer / export_map_view |
| 布局与出图 | "建 A3 横向布局，标题…，图例只保留 3 个图层，加比例尺和指北针，导出 300 dpi PDF" | create_layout / add_layout_* / export_layout |
| 栅格 | "读 DEM 的波段、像元大小和统计值；用 NDVI 公式算植被指数" | get_raster_info / raster_calculator / zonal_statistics |
| 目录与数据发现 | "在工程和文件夹连接里搜名字含 `2025` 的 shapefile" | search_data / describe_dataset / list_folder |
| 实用工具 | "一次完成：隐藏底图→缩放到区县→导出 PNG；用 arcpy 给所有图层名加前缀" | run_batch / execute_arcpy_code |

### 标准出图提示词模板（复制即用）

```text
用当前工程的 <地图名> 出图：
- 范围：<研究区图层/范围>，比例尺 1:<...>
- 数据：<图层1>、<图层2>（需要时说明字段和筛选条件）
- 符号：<按字段分级/唯一值/单色>，配色 <色带或十六进制>
- 版式：<A3 横向 / A4 纵向>，标题 "<标题>"，图例 <位置>，比例尺 <位置>，指北针 <位置>
- 输出：<PDF / PNG>，<300> dpi，保存到 <绝对路径>
- 流程：先自检版式，把预览图发给我确认，确认后再导出正式文件。
```

### 批量出图提示词

```text
对 `maps` 里的每个地图，套用同一套 A3 横向布局模板导出 PDF：
标题用地图名，图例保留可见图层，比例尺和指北针放右下角，
输出到 `D:\out\maps\`，300 dpi。先导出第一张给我看，确认后再批量。
```

### 常见翻车怎么用一句话补救

- "图例压住主图了" → 助手调整 `legend` 坐标或版式。
- "页面比例不对/留白太多" → 调整 map frame 与页面比例。
- "配色太跳/不符合论文风格" → 给出色带、十六进制或参考图。
- "先别导出，给我预览" → 助手先 `export_map_view` 或布局预览。

**效果**：把"数据、操作、风格、输出"一次讲全，助手通常一轮就能做出接近终稿的图；后续修改只需指出差异（"图例右移 1 cm"），不用重述全部。

---

## 🛡️ 弹窗守护怎么用（1.x 版）

**它解决什么**：ArcGIS Pro 第一次运行第三方 `.pyt` / Python 工具（如 RunCode.pyt）会弹"第三方代码库"信任框；无人值守时任务会卡住。本版没有 MCP 守卫工具，而是包内自带一个**独立守护脚本**：启动后轮询 Pro 窗口，只点这一类"是(Y)"，按设定时长自动退出。

**启动方式**

```text
install\03-dialog-guard.cmd                    # 默认 600 秒
install\03-dialog-guard.cmd -DurationSeconds 120
install\03-dialog-guard.cmd -AlertOnly         # 只置顶提醒，不自动点击
install\03-dialog-guard.cmd -NotifyTest        # 只测提醒窗（30 秒）
```

也可以直接双击 `03-dialog-guard.cmd`，或让支持终端操作的 AI 助手在后台启动它。

**提示词示例**

| 你说 | 助手会做什么 |
|---|---|
| "我要在 Pro 里手动跑 RunCode.pyt。先帮我启动弹窗守护 600 秒；出现第三方代码信任框自动点‘是’，点不到就置顶响铃提醒我" | 在后台运行 `install\03-dialog-guard.cmd -DurationSeconds 600` |
| "这次只提醒、不自动点" | 启动 `-AlertOnly` |
| "测试一下提醒窗能不能弹出来" | 启动 `-NotifyTest`（30 秒） |
| "守护的日志在哪？" | `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log` |

**边界与效果**
- 只处理带"第三方 / 代码库 / third-party / code library"上下文的信任框；覆盖、删除等其他确认框一律不碰。
- 点不到按钮时，置顶提醒窗至少 30 秒并每 10 秒响铃。
- 每个 Pro 进程首次信任后不再弹；重启 Pro 后恢复需要。
- 2.x 把同一能力升级成了 `guard_summon` / `guard_status`，并在 RunCode 调用前自动 arm/settle；本版需要手动启动脚本。

---

## 🐍 RunCode 与 Python 桥怎么用（提示词指南）

arcpy 通道分两级：**RunCode 工具箱**适合一次性/批量脚本；**Python 桥**适合需要跨调用保持变量的交互式会话。

### RunCode 工具箱

路径：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`。让助手调用 `run_python_toolbox_tool` 时传入该路径即可；也可以自己在 Pro 里"插入 → 工具箱 → 添加工具箱"。

**提示词示例**
- "用 RunCode.pyt 批量把当前工程所有图层的标注打开，标注字段用 `NAME`，字号 10。"
- "写一个幂等 arcpy 脚本，用 RunCode 给每个图层名加前缀 `LYR_`，已经加过就跳过。"
- "这段脚本要在 RunCode.pyt 里跑，先帮我检查参数和路径，再执行。"

**首次运行**会弹"第三方代码库"信任框：先启动上面的弹窗守护，或手动点一次"是"。同一 Pro 进程只需信任一次。

### Python 桥（需要 `CURRENT` 或持久变量时）

1. 在 Pro 菜单 **分析 → Python** 打开 Python 窗口；
2. 粘贴执行（路径按默认安装位置）：

```python
import sys; sys.path.insert(0, r"C:\Users\<你的用户名>\AppData\Local\ArcGIS Pro MCP\plugin")
import mcp_bridge; print(mcp_bridge.start_server(port=6511))
```

3. 需要 `CURRENT`（当前工程）时再加一行（必须在主线程执行）：

```python
print(mcp_bridge.start_pump())
```

**提示词示例**
- "接下来的 arcpy 需要 `CURRENT`。请提醒我在 Pro 的 Python 窗口粘贴桥的启动代码，然后我用 `execute_arcpy_code` 执行。"
- "帮我把桥启动代码按我的用户名填好，复制给我；我自己粘贴到 Pro。"
- "先 `get_pump_status` 看看 pump 是否就绪；没就绪就提醒我粘贴。"
- "用持久会话分三步：先读图层，再建字段，最后计算——变量要跨调用保留。"

**效果**：RunCode 解决"批量脚本一次跑完"；Python 桥解决"多步交互、需要当前工程"；两者都不需要另装 Python。

---

## 🧪 关于蒸馏模块（1.1.12 起已包含）

本分支自 **1.1.12** 起**包含**模板蒸馏（从 2.x 整体移植，1.1.13 增加 `template_delete`）：11 个 `template_*` 工具 + `arcgis-distill` CLI + 本地模板库。2.x 独有的 `guard_summon` / `guard_status` 仍不在本线，弹窗守护继续用独立脚本 `03-dialog-guard.cmd`。

- 11 个 `template_*` 工具：import / measure / patch / validate / preview / review / approve / apply / list / get / delete；
- 模板库在 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`，升级安装会保留已有模板；
- 确定性步骤（测量/校验/预览/入库）由 `arcgis-distill` 完成（Pro 自带 Python，零额外依赖），语义字段由助手按 `skills/template-distill/SKILL.md` 撰写，工具本身零 API key；
- 典型用法："把 `D:\samples\fig3.png` 蒸馏成模板，名称研究区总览图；按 import → measure → patch → validate → preview 流程，预览给我确认后再 review + approve。"
- 清理：`arcgis-distill delete <id> [--force]` 删除模板及无引用样本图；ready 模板需 `force`。

1.1.13 与 2.x 的能力对照：

| 能力 | 1.1.13（本分支） | 2.x |
|---|---|---|
| 原生 MCP 工具 | 126 个（13 组） | 125 个（14 组，含 guard 2 个） |
| RunCode / Python 桥 | ✅ | ✅ |
| 弹窗守护 | 独立脚本 `03-dialog-guard.cmd` | `guard_summon` / `guard_status` + RunCode 自动 arm/settle |
| 模板蒸馏 | ✅ 11 个 `template_*` 工具 + schema v1.1 | ✅ 同左 |

---

## 🧰 全部 126 个 MCP 工具（13 组）

助手能做的全部事情，按名字列在这里。点开看看 👇

<details>
<summary><b>📜 完整工具清单 —— 会话 16 · 图层 20 · 数据 11 · 选择 6 · 字段/数据集 11 · 地理处理 7 · 符号化 6 · 视图 7 · 布局 16 · 栅格 5 · 目录 6 · 实用工具 4 · 模板蒸馏 11</b></summary>

### 会话与工程（16）

| 工具 | 用途 |
|---|---|
| `ping` | 检查桥接连通性，并返回所连接的工程。 |
| `get_capabilities` | 列出桥接支持的全部命令（按领域分组），用于发现具名工具之外的能力。 |
| `get_queue_status` | 查看三车道队列状态：各车道深度、当前命令与最近完成记录；调用变慢或被排队时先看它。 |
| `diagnose` | 连接自检：Pro 版本、许可、打开的工程、活动地图、地图视图与写权限；并主动探测 Python 桥端口（`bridge_listening` / `probe_ms`）。出问题先跑它。 |
| `get_arcgis_info` | ArcGIS Pro 版本、许可级别、可用扩展与当前工程路径。 |
| `get_project_info` | 工程路径、默认地理数据库与工具箱、地图、布局、文件夹与数据库连接。 |
| `save_project` | 保存工程（.aprx），或另存副本到指定位置。 |
| `list_maps` | 列出所有地图与场景，含坐标系与图层数量。 |
| `create_map` | 在工程中新建地图或场景（默认不加底图；`project_default` 恢复旧行为）。 |
| `remove_map` | 从工程中删除地图。 |
| `activate_map` | 在 Pro 界面中打开/激活地图视图；相机类命令与 export_map_view 之前必需。 |
| `set_map_properties` | 重命名地图或修改其坐标系（`epsg` 生效）。 |
| `get_map_extent` | 地图中全部数据图层的合并范围。 |
| `get_environment` | 读取当前 arcpy 地理处理环境设置。 |
| `set_environment` | 设置 arcpy 环境（工作空间、输出坐标系、范围、掩膜、像元大小、覆盖输出、并行因子、捕捉栅格等）；设置真正传入 GP，且支持每调用覆盖。 |
| `restart_pro` | 重启 ArcGIS Pro（先保存并重开工程）——让重编译的 add-in 免手动加载；回复在 Pro 关闭前返回，桥恢复后客户端自动重连。 |

### 图层（20）

| 工具 | 用途 |
|---|---|
| `get_layers` | 列出地图中的图层（含绘制顺序与组嵌套）与独立表；了解工程内容从这里开始。 |
| `get_layer_info` | 单图层完整详情：数据源、坐标系、范围、字段、要素数、渲染器与标注状态；也接受数据集路径（1.1.9+）。 |
| `add_layer` | 按路径或服务 URL 向地图添加数据：要素类、shapefile、栅格、表、.lyrx 图层文件或 Web 服务。 |
| `add_table` | 向地图添加独立表（地理数据库表或文件夹中的表）。 |
| `add_web_layer` | 按 URL 添加 Web 图层（Feature/Map/Image 服务、WMS、WMTS、矢量瓦片）。 |
| `remove_layer` | 从地图中移除图层或独立表。 |
| `rename_layer` | 重命名内容列表中的图层。 |
| `duplicate_layer` | 在地图内复制图层，便于用不同符号化展示。 |
| `set_layer_visibility` | 显示或隐藏单个、多个或地图中的全部图层。 |
| `set_layer_transparency` | 设置图层透明度（0 不透明，100 全透明）。 |
| `set_layer_scale_range` | 限制图层的可见比例范围。 |
| `set_definition_query` | 设置图层定义查询（SQL where），空字符串表示清除。 |
| `move_layer` | 调整图层相对顺序，或移入组图层。 |
| `create_group_layer` | 创建组图层，可把已有图层移入其中。 |
| `zoom_to_layer` | 将地图视图缩放到图层范围。 |
| `set_basemap` | 设置底图：地形、影像、影像混合、街道、导航、浅灰画布、深灰画布、地形晕渲、海洋、OpenStreetMap、国家地理风格等。 |
| `get_broken_layers` | 列出所有地图中数据源缺失的图层。 |
| `repair_layer_source` | 将图层重新指向新的工作空间或数据集，修复失效数据源（含栅格图层，1.1.9+）。 |
| `add_join` | 按公共字段把表连接到图层（客户端连接注册表；GP 重注册后连接仍有效）。 |
| `remove_join` | 移除图层的连接。 |

### 数据与属性（11）

| 工具 | 用途 |
|---|---|
| `get_features` | 读取图层、表或数据集路径的属性行，支持 where、字段子集、排序与几何；分页返回 `truncated` / `next_offset`，几何可给摘要或带精度的 WKT。 |
| `count_features` | 统计要素数量，可带 where 条件。 |
| `get_unique_values` | 返回字段的唯一值及各自行数。 |
| `get_field_statistics` | 数值字段的 min/max/mean/median/sum/标准差。 |
| `summarize_features` | 按一个或多个字段分组聚合，是回答“每类多少/总量多少”的快捷方式（无需 GP）。 |
| `insert_features` | 向图层、独立表或数据集路径插入新行。 |
| `update_features` | 按 where 条件更新行的属性与/或几何（图层与独立表都支持）。 |
| `delete_features` | 按 where 条件删除行（图层与独立表都支持）。 |
| `save_edits` | 提交挂起的编辑；未保存的编辑会锁定数据，导致删除或覆盖失败。 |
| `discard_edits` | 丢弃挂起的编辑。 |
| `calculate_field` | 字段计算，例如表达式 "!Shape_Area! / 10000"。 |

### 选择（6）

| 工具 | 用途 |
|---|---|
| `select_features` | 按 SQL where 条件选择要素。 |
| `select_by_location` | 按与另一图层的空间关系选择要素。 |
| `get_selection` | 报告当前选择情况（按图层，含 ObjectID 与可选属性行）。 |
| `set_selection` | 按 ObjectID 选择指定要素。 |
| `clear_selection` | 清除单个图层或地图中全部选择。 |
| `zoom_to_selection` | 将地图视图缩放到当前选中的要素。 |

### 字段与数据集（11）

| 工具 | 用途 |
|---|---|
| `list_fields` | 列出图层字段及类型、别名、长度与域。 |
| `add_field` | 向图层或表添加字段。 |
| `add_fields` | 一次调用添加多个字段。 |
| `delete_field` | 删除一个或多个字段。 |
| `alter_field` | 重命名字段，或修改别名/长度。 |
| `create_feature_class` | 创建空要素类（默认放在工程默认地理数据库）并加入地图；请求的字段在创建后逐个添加并回读校验——不符即删除新数据集并报错。 |
| `create_table` | 创建空独立表；请求的字段在创建后逐个添加并回读校验——不符即删除新表并报错。 |
| `create_file_geodatabase` | 新建文件地理数据库。 |
| `truncate_table` | 清空表或要素类的全部行，保留结构。 |
| `delete_dataset` | 从磁盘或地理数据库删除数据集；不可撤销。 |
| `export_features` | 按当前选择与定义查询导出图层为新数据集。 |

### 地理处理（7）

| 工具 | 用途 |
|---|---|
| `run_geoprocessing_tool` | 运行任意 arcpy 地理处理工具——分析的通用通道；图层名会解析为实时图层，选择与定义查询生效。参数名未知时先调 describe_geoprocessing_tool。 |
| `list_geoprocessing_tools` | 按名称搜索可用的地理处理工具。 |
| `list_toolboxes` | 列出 arcpy 工具箱模块与工程中的工具箱。 |
| `describe_geoprocessing_tool` | 运行前查询工具的参数、数据类型、默认值、枚举与用法说明。 |
| `run_python_toolbox_tool` | 运行磁盘上自定义 .pyt / .atbx / .tbx 工具箱中的工具（原生端只收位置参数）；路径笔误时给出 "Did you mean …" 提示。 |
| `check_extension` | 报告 add-in 能否检查扩展许可（Pro 上为 `inspectable:false`——是否缺许可需实际运行工具判断）。 |
| `get_messages` | 最近一次地理处理操作的消息。 |

### 符号化（6）

| 工具 | 用途 |
|---|---|
| `set_layer_renderer` | 修改图层符号化：单一符号、按类别的唯一值、按数值字段的分级/连续色带。 |
| `get_layer_symbology` | 查看图层当前渲染器、分级断点、唯一值与标注设置。 |
| `list_color_ramps` | 列出工程中可用的色带。 |
| `set_layer_labeling` | 开关标注，并设置表达式、字体与晕圈。 |
| `apply_symbology_from_layer` | 从 .lyrx 文件或另一图层复制符号化。 |
| `save_layer_file` | 将图层及其符号化保存为 .lyrx 文件复用。 |

### 地图视图（7）

| 工具 | 用途 |
|---|---|
| `get_map_view` | 当前相机位置：中心、比例、旋转与可见范围；需要地图选项卡在 Pro 中打开。 |
| `set_map_view` | 移动地图视图：设置范围、中心点、比例和/或旋转。 |
| `export_map_view` | 把地图视图渲染为 PNG 并返回图片，用于目视检查；需要地图选项卡打开；支持 `fit:"auto"` 按视图比例出图。 |
| `list_bookmarks` | 列出地图上定义的空间书签。 |
| `create_bookmark` | 把当前视图或指定范围保存为命名书签。 |
| `apply_bookmark` | 将地图视图缩放到书签。 |
| `delete_bookmark` | 删除地图书签。 |

### 布局与出图（16）

| 工具 | 用途 |
|---|---|
| `list_layouts` | 列出打印布局及页面尺寸与元素数量。 |
| `get_layout_info` | 查看布局：页面设置与每个元素的位置尺寸。 |
| `create_layout` | 新建布局页（默认含铺满页面的地图框）。 |
| `delete_layout` | 从工程删除布局。 |
| `add_map_frame` | 在布局页面指定位置添加地图框。 |
| `set_map_frame_extent` | 将布局地图框指向某个图层、范围或比例。 |
| `add_layout_text` | 向布局添加标题、署名等文本元素。 |
| `add_layout_legend` | 添加与布局地图框关联的图例。 |
| `add_layout_scale_bar` | 添加与地图框关联的比例尺。 |
| `add_layout_north_arrow` | 添加与地图框关联的指北针。 |
| `add_layout_picture` | 在布局上放置图片（如 Logo）。 |
| `set_layout_element` | 移动、缩放、重命名、隐藏或修改任意布局元素的文本（页面坐标以左下角为原点；越界位置会被拒绝）。 |
| `delete_layout_element` | 从布局中删除元素。 |
| `export_layout` | 导出布局为 PDF / PNG / JPEG / SVG / TIFF；PNG/JPEG 以图片内联返回便于检查。 |
| `preview_layout` | 把布局渲染为临时图片（给定 `output_path` 才写文件），便于边建边看。 |
| `export_map_series` | 把布局的地图系列（地图册）导出为多页 PDF。 |

### 栅格（5）

| 工具 | 用途 |
|---|---|
| `get_raster_info` | 栅格详情：波段、大小、像元大小、像素类型、统计与坐标系。 |
| `set_raster_symbology` | 设置栅格图层的配色器与色带；支持显式颜色与标签。 |
| `raster_calculator` | 栅格计算（地图代数）：为栅格绑定短名后写 Python 表达式；需要 Spatial Analyst 扩展。 |
| `sample_raster_values` | 按地图坐标（`points`）或点图层要素读取栅格像元值——内联返回，不产生副作用表（`write_table=true` 时例外）。 |
| `zonal_statistics` | 分区统计：汇总分区多边形内的栅格值并返回表格；需要 Spatial Analyst 扩展。 |

### 目录与数据发现（6）

| 工具 | 用途 |
|---|---|
| `list_workspace_contents` | 列出地理数据库或文件夹内的数据集；默认工程默认地理数据库。 |
| `list_folder` | 列出磁盘上的 GIS 文件与子文件夹；默认工程主目录。 |
| `describe_dataset` | 按路径描述任意数据集：类型、几何、坐标系、范围、字段与行数（无需加入地图）。 |
| `search_data` | 在工程地理数据库、主目录与文件夹连接中按名称查找数据，是加数据前最快的定位方式；结果已去重。 |
| `get_project_items` | 工程中注册的文件夹连接、数据库连接与工具箱。 |
| `add_folder_connection` | 把文件夹注册到工程，便于浏览其数据。 |

### 实用工具（4）

| 工具 | 用途 |
|---|---|
| `execute_arcpy_code` | 在 ArcGIS Pro 内运行 Python：arcpy 已导入，arcpy.mp.ArcGISProject('CURRENT') 为打开工程，变量跨调用保持，print 输出被捕获，末尾表达式值会返回。没有专用工具时用它；由 Python 桥（6511）应答。 |
| `get_pump_status` | 主线程调度器是否已安装；Python 桥未启动时返回结构化 down 状态（`running:false, bridge_listening:false`，带 code 与 hint），而不是报错。 |
| `stop_pump` | 移除主线程调度器；实时工程类命令将停止工作，直到用户重新安装。 |
| `run_batch` | 一次往返运行多条命令，多步工作流会快很多；图片结果默认降为 path/bytes/sha，每步带耗时。 |

### 模板蒸馏（11）

| 工具 | 用途 |
|---|---|
| `template_list` | 列出本地模板库中的版式模板，可按状态或标签过滤。 |
| `template_get` | 读取单个模板的完整 spec（版式几何、配色、字体、分区、坑点与评审记录）。 |
| `template_import` | 把一张样本成图导入模板库为草稿（按图片 sha256 去重），下一步是 template_measure。 |
| `template_measure` | 测量样本图：图幅包围盒、纵横比、强框线、主色与 DPI，写入不可手改的 measured 块。 |
| `template_patch` | 提交语义字段（分区/图例/字体/配色/坑点）并做校验合并；禁止修改 measured 等保护字段。 |
| `template_validate` | 运行 schema 与几何一致性校验（图框/分区在画布内、测量容差 ≤3%、字体层级、颜色、坑点）。 |
| `template_review` | 记录模板的人工评审结论（评审人 + 备注）。 |
| `template_approve` | 批准已评审且校验通过的模板为 ready；缺评审或有校验错误时拒绝（force 可覆盖并记录）。 |
| `template_preview` | 渲染模板线框图（与样本图并排）并返回 PNG，供视觉核对。 |
| `template_apply` | 把 ready 模板写成制图上下文 JSON（版式/配色/字体），助手据此在 Pro 中出图。 |
| `template_delete` | 从模板库删除模板——spec、索引条目、作业单，以及不再被其他模板引用的样本图；已批准（ready）模板需 force。 |
</details>

---

## 📦 包里有什么

| 路径 | 内容 | 说明 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.13 | 126 个 MCP 工具 + 全部 GP 工具；HTTP MCP 6520 / 旧 TCP 6510 |
| `install\runtime\distill\` | 🧪 模板蒸馏 | `arcgis-distill` 命令行 + `arcgis_agentkit` 包（Pro 自带 Python 运行）；模板库在 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`，升级保留 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio 桥 | 给只能启动本地进程的客户端（如 Claude Desktop）转接 6520；纯 PowerShell，无需 Python |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode 工具箱 | 需要高频执行任意 arcpy 时使用；首次运行会弹一次信任框 |
| `install\runtime\guard\` | 🛡️ 弹窗守护 | 自动点击“第三方代码库执行”信任框；失败时置顶提醒（≥30 秒、响铃） |
| `install\runtime\plugin\` | 🔌 Python 桥（可选） | 在 Pro 的 Python 窗口里启动，提供 `execute_arcpy_code` 持久会话（端口 6511） |
| `install\*.cmd / *.ps1` | 🛠️ 安装与自检 | 安装 / 自检 / 配置 Claude / 弹窗守护 / 卸载 |
| `clients\` | 📁 客户端接入说明 | Claude Code、Claude Desktop、Codex、通用 JSON |
| `docs\` | 📖 使用指南、排障、工作规则、更新日志 | 先看使用指南 |
| `source\` | 📜 完整源码快照 | 与本包二进制对应的源码（AGPL-3.0 合规要求），含上游 LICENSE / NOTICE 与构建产物 `source\dist\` |

### 🔍 依赖审计（为什么说“全打包”）

- 🧩 add-in：自包含 C# 程序集，运行在 **ArcGIS Pro 3.7 系统要求所列的** .NET Desktop Runtime 10 x64 上——**本包**无需另装 .NET。
- 🌉 stdio 桥、🛡️ 弹窗守护：Windows 自带的 PowerShell 5.1 + WinForms——无需另装。
- 🐍 RunCode 工具箱、Python 桥：运行在 ArcGIS Pro 自带的 Python（arcpy）里——无需另装 Python。
- ✅ **因此本包没有任何需要 pip / 联网安装的外部依赖。**“离线可用”指本包自身的安装与运行链路。ArcGIS Pro 3.7 的系统要求（Microsoft .NET Desktop Runtime 10 x64 与 WebView2）属于 Esri 的前置条件——若本机缺失，请按 Esri 系统要求安装对应组件（Pro 安装器通常会提示或代为安装）。

---

## 🧪 可选组件怎么用

**🐍 RunCode 工具箱**（高频 arcpy 场景）
- 路径：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- 让 AI 调用 `run_python_toolbox_tool` 时传入该路径即可；或在 Pro 里“插入 → 工具箱 → 添加工具箱”选择它。
- 每个 Pro 会话首次运行会弹“第三方代码库”信任框：可先运行 `03-dialog-guard.cmd`，守护会自动点“是” ✨。

**🔌 Python 桥**（需要跨调用保持 arcpy 会话时）
1. 在 ArcGIS Pro 菜单 **分析 → Python** 打开 Python 窗口；
2. 粘贴执行（路径已按默认安装位置写好）：
   ```python
   import sys; sys.path.insert(0, r"C:\Users\<你的用户名>\AppData\Local\ArcGIS Pro MCP\plugin")
   import mcp_bridge; print(mcp_bridge.start_server(port=6511))
   ```
3. 需要 `CURRENT`（当前工程）时再加一行（必须在主线程执行）：
   ```python
   print(mcp_bridge.start_pump())
   ```

**🛡️ 弹窗守护**（推荐在跑 RunCode / ENVI→Pro 联合流程前启动）
```
03-dialog-guard.cmd                 # 600 秒守护
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 只测提醒窗（30 秒）
03-dialog-guard.cmd -AlertOnly      # 只提醒，不自动点击
```
安全边界：只处理带“第三方 / 代码库 / third-party / code library”上下文的信任框；**绝不触碰覆盖、删除等其他确认框** 🙅。日志在 `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`。

---

## 🔒 安全说明

- 🏠 add-in 与所有桥接服务只监听 `127.0.0.1`，局域网其他机器无法连接。
- ⚖️ 本软件仅在 `127.0.0.1` 提供本机服务，不向第三方提供网络服务；如你在修改后通过网络向他人提供服务，AGPL-3.0 §13 的源码提供义务适用。
- 🔏 add-in 为未签名构建：首次加载时 ArcGIS Pro 会要求你确认信任，这是正常流程；请核对本包 `SHA256SUMS.txt`。
- 🕵️ 本包不包含任何遥测或联网上报。
- 🧹 卸载：`99-uninstall.cmd`（可选择保留客户端配置）。

---

## 📜 版本与许可

- 🏷️ 版本：**1.1.13**（Customer 1.1.13 · 1.14 增强版 + 模板蒸馏；构建日期见 `VERSION.json`；工具数 126）
- 👤 维护者：**Law**（[@XNYGggg](https://github.com/XNYGggg)）
- 🙏 上游项目与原作者：**[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**，作者 **Pisut Nakmuenwai**（AGPL-3.0-or-later）。本包在其基础上累计补丁至 1.1.12（修改点见 `docs\CHANGELOG.md` 与 `RELEASE-NOTES.md`）。
- ⚖️ 许可证：**AGPL-3.0-or-later**（本包附带完整源码快照与上游 NOTICE）
- 🏷️ 商标：“ArcGIS”为 Esri 商标；本项目为第三方工具，与 Esri 无隶属关系。

🆘 常见问题见 `docs\排障手册.md`；进阶用法见 `docs\使用指南.md`。祝你出图愉快 🎨 (｀・ω・´)ゞ

---

<!-- LDU-THANKS -->

<h2 align="center">🙏 致谢</h2>

<p align="center">
  本项目的研究与开发得到了 <strong>鲁东大学</strong> 的支持，谨此致以诚挚的感谢。
</p>

<p align="center">
  <a href="https://www.ldu.edu.cn/"><img src="assets/ldu-emblem.png" width="104" alt="鲁东大学校徽"></a>
</p>

<h3 align="center">鲁东大学 · Ludong University</h3>

> 鲁东大学始建于 1930 年，坐落于山东烟台，是一所以应用型人才培养为主导、工农理文多学科协调发展的省属综合性大学。学校现设 26 个学院，工程学、化学、材料科学等 6 个学科进入 ESI 全球排名前 1%，秉承「厚德、博学、日新、笃行」的校训。

<p align="center">
  📍 山东省烟台市芝罘区红旗中路 186 号（邮编 264025）<br>
  🌐 <a href="https://www.ldu.edu.cn/">学校官网</a> ·
  <a href="https://jwc.ldu.edu.cn/">教务处</a> ·
  <a href="https://lib.ldu.edu.cn/">图书馆</a> ·
  <a href="https://ehall.ldu.edu.cn/">网上办事大厅</a>
</p>
