[![English](https://img.shields.io/badge/Language-English-1f6feb?style=flat-square)](README.md) [![简体中文](https://img.shields.io/badge/Language-%E7%AE%80%E4%BD%93%E4%B8%AD%E6%96%87-d1242f?style=flat-square)](README.zh-CN.md) [![Version](https://img.shields.io/badge/Version-1.1.4%20(stable)-2ea44f?style=flat-square)](../../releases/tag/v1.1.4) [![License: AGPL v3+](https://img.shields.io/badge/License-AGPL--3.0--or--later-blue?style=flat-square)](LICENSE) [![ArcGIS Pro](https://img.shields.io/badge/ArcGIS%20Pro-3.7%2B-00a1e0?style=flat-square)](https://www.esri.com/en-us/arcgis/products/arcgis-pro/overview) [![Platform](https://img.shields.io/badge/Platform-Windows%2011-lightgrey?style=flat-square)]() [![Release](https://img.shields.io/github/v/release/XNYGggg/arcgis-pro-mcp-optimized?style=flat-square&label=release)](../../releases)

# 🗺️ ArcGIS Pro MCP 优化版 · 客户版 v1.1.4 · 稳定版（推荐）🎁

用一句自然语言，让 AI 助手直接操作你正开着的 ArcGIS Pro 工程：加图层、改符号、查属性、跑地理处理、出图导出。数据不出本机，所有通信都走 `127.0.0.1` 🔒。

> **📌 版本说明（2026-11-16 更新）**：v1.1.4（add-in 1.1.4，113 工具）为 **稳定版 · 推荐使用**。另一条 2.0 线（v2.0.0 / v2.0.1）为**测试版**，优化尚未完成，其 GitHub Release 资产与 tag 已移除、**不推荐使用**（NuGet 2.0.x 将无法下载）；2.0 代码仅保留在 `customer-v2.0.1` / `customer-v2.0.0` 分支中。分支 / 标签 / Release 对照见 [BRANCHES.md](https://github.com/XNYGggg/arcgis-pro-mcp-optimized/blob/main/BRANCHES.md)。

> **🙏 上游项目与原作者**：本客户版基于 **[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**（原作者 **Pisut Nakmuenwai**，AGPL-3.0-or-later），在其基础上累计本地补丁至 **v1.1.4**。完整署名与修改声明见包根 `NOTICE` 与 `source/SOURCE-NOTICE.md`。

本客户版把运行所需的**全部组件**打包在一起，离线可用，**不需要安装 Python、不需要 pip、不需要联网**，除 ArcGIS Pro 本身外没有任何额外依赖。电池都给你装好了 🔋

---

## 🖥️ 系统要求

| 项目 | 要求 |
|---|---|
| 🪟 操作系统 | Windows 10 / 11（64 位；以 ArcGIS Pro 3.7 官方支持的系统为准，Pro 3.7 推荐 Windows 11） |
| 🗺️ ArcGIS Pro | **3.7 或更高**（本 add-in 基于 3.7 / .NET 10 构建） |
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

**按工具名调用**：下表中的每个工具都可以被客户端按名字直接调用；`run_batch` 可在一次往返里串起多条命令；没有专用工具时用 `execute_arcpy_code` 兜底。

**编辑规则**：编辑在 `save_edits` 之前一直挂起，挂起期间数据被锁定（删除/覆盖会失败）；`discard_edits` 丢弃挂起编辑。`delete_features` / `delete_dataset` 不可撤销——先让助手说明影响范围再执行。

**批量与提速**：重复度高、数据量大时，让助手用 RunCode 写一个幂等 arcpy 脚本一次执行；大工程按范围/OID 分块并留检查点。

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

**它解决什么**：ArcGIS Pro 第一次运行第三方 `.pyt` / Python 工具（如 RunCode.pyt）会弹"第三方代码库"信任框；无人值守时任务会卡住。1.1.4 没有 MCP 守卫工具，而是包内自带一个**独立守护脚本**：启动后轮询 Pro 窗口，只点这一类"是(Y)"，按设定时长自动退出。

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
- 2.x 把同一能力升级成了 `guard_summon` / `guard_status`，并在 RunCode 调用前自动 arm/settle；1.1.4 需要手动启动脚本。

---

## 🐍 RunCode 与 Python 桥怎么用（提示词指南）

1.1.4 的 arcpy 通道有两级：**RunCode 工具箱**适合一次性/批量脚本；**Python 桥**适合需要跨调用保持变量的交互式会话。

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

## 🧪 关于蒸馏模块（2.x 功能）

1.1.4 **不包含**模板蒸馏：没有 `template_*` 工具，也没有 `guard_summon` / `guard_status`。如果你想把样图版式变成可复用模板、让多张图版式统一，请升级到 **V2.01（v2.0.1）**：

- 2.x 提供 10 个 `template_*` 工具：import / measure / patch / validate / preview / review / approve / apply / list / get；
- 模板库在 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`，升级安装会保留已有模板；
- 典型用法："把 `D:\samples\fig3.png` 蒸馏成模板，名称研究区总览图；按 import → measure → patch → validate → preview 流程，预览给我确认后再 review + approve。"

1.1.4 与 2.x 的能力对照：

| 能力 | 1.1.4（本分支） | 2.0.1（推荐） |
|---|---|---|
| 原生 MCP 工具 | 113 个（12 组） | 125 个（14 组） |
| RunCode / Python 桥 | ✅ | ✅ |
| 弹窗守护 | 独立脚本 `03-dialog-guard.cmd` | `guard_summon` / `guard_status` + RunCode 自动 arm/settle |
| 模板蒸馏 | ❌ | ✅ 10 个 `template_*` 工具 + schema v1.1 |

---

## 🧰 全部 113 个 MCP 工具（12 组）

助手能做的全部事情，按名字列在这里。点开看看 👇

<details>
<summary><b>📜 完整工具清单 —— 会话 15 · 图层 19 · 数据 11 · 选择 6 · 字段/数据集 11 · 地理处理 7 · 符号化 6 · 视图 7 · 布局 16 · 栅格 5 · 目录 6 · 实用工具 4</b></summary>

### 会话与工程（15）

| 工具 | 用途 |
|---|---|
| `ping` | 检查桥接连通性，并返回所连接的工程。 |
| `get_capabilities` | 列出桥接支持的全部命令（按领域分组），用于发现具名工具之外的能力。 |
| `get_queue_status` | 查看三车道队列状态：各车道深度、当前命令与最近完成记录；调用变慢或被排队时先看它。 |
| `diagnose` | 连接自检：Pro 版本、许可、打开的工程、活动地图、地图视图与写权限；出问题先跑它。 |
| `get_arcgis_info` | ArcGIS Pro 版本、许可级别、可用扩展与当前工程路径。 |
| `get_project_info` | 工程路径、默认地理数据库与工具箱、地图、布局、文件夹与数据库连接。 |
| `save_project` | 保存工程（.aprx），或另存副本到指定位置。 |
| `list_maps` | 列出所有地图与场景，含坐标系与图层数量。 |
| `create_map` | 在工程中新建地图或场景。 |
| `remove_map` | 从工程中删除地图。 |
| `activate_map` | 在 Pro 界面中打开/激活地图视图；相机类命令与 export_map_view 之前必需。 |
| `set_map_properties` | 重命名地图或修改其坐标系。 |
| `get_map_extent` | 地图中全部数据图层的合并范围。 |
| `get_environment` | 读取当前 arcpy 地理处理环境设置。 |
| `set_environment` | 设置 arcpy 环境（工作空间、输出坐标系、范围、掩膜、像元大小、覆盖输出、并行因子等）。 |

### 图层（19）

| 工具 | 用途 |
|---|---|
| `get_layers` | 列出地图中的图层（含绘制顺序与组嵌套）与独立表；了解工程内容从这里开始。 |
| `get_layer_info` | 单图层完整详情：数据源、坐标系、范围、字段、要素数、渲染器与标注状态。 |
| `add_layer` | 按路径或服务 URL 向地图添加数据：要素类、shapefile、栅格、表、.lyrx 图层文件或 Web 服务。 |
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
| `repair_layer_source` | 将图层重新指向新的工作空间或数据集，修复失效数据源。 |
| `add_join` | 按公共字段把表连接到图层。 |
| `remove_join` | 移除图层的连接。 |

### 数据与属性（11）

| 工具 | 用途 |
|---|---|
| `get_features` | 读取图层、表或数据集路径的属性行，支持 where、字段子集、排序与 WKT 几何。 |
| `count_features` | 统计要素数量，可带 where 条件。 |
| `get_unique_values` | 返回字段的唯一值及各自行数。 |
| `get_field_statistics` | 数值字段的 min/max/mean/median/sum/标准差。 |
| `summarize_features` | 按一个或多个字段分组聚合，是回答“每类多少/总量多少”的快捷方式（无需 GP）。 |
| `insert_features` | 向图层或表插入新行。 |
| `update_features` | 按 where 条件更新行的属性与/或几何。 |
| `delete_features` | 按 where 条件删除行。 |
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
| `create_feature_class` | 创建空要素类（默认放在工程默认地理数据库）并加入地图。 |
| `create_table` | 创建空独立表。 |
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
| `describe_geoprocessing_tool` | 运行前查询工具的参数、数据类型、默认值与用法说明。 |
| `run_python_toolbox_tool` | 运行磁盘上自定义 .pyt / .atbx / .tbx 工具箱中的工具。 |
| `check_extension` | 检查并按需检出 ArcGIS 扩展许可。 |
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
| `export_map_view` | 把地图视图渲染为 PNG 并返回图片，用于目视检查；需要地图选项卡打开。 |
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
| `set_layout_element` | 移动、缩放、重命名、隐藏或修改任意布局元素的文本。 |
| `delete_layout_element` | 从布局中删除元素。 |
| `export_layout` | 导出布局为 PDF / PNG / JPEG / SVG / TIFF；PNG/JPEG 以图片内联返回便于检查。 |
| `preview_layout` | 把布局渲染为临时图片返回（不写文件），便于边建边看。 |
| `export_map_series` | 把布局的地图系列（地图册）导出为多页 PDF。 |

### 栅格（5）

| 工具 | 用途 |
|---|---|
| `get_raster_info` | 栅格详情：波段、大小、像元大小、像素类型、统计与坐标系。 |
| `set_raster_symbology` | 设置栅格图层的配色器与色带。 |
| `raster_calculator` | 栅格计算（地图代数）：为栅格绑定短名后写 Python 表达式；需要 Spatial Analyst 扩展。 |
| `sample_raster_values` | 按地图坐标或点图层要素读取栅格像元值。 |
| `zonal_statistics` | 分区统计：汇总分区多边形内的栅格值并返回表格；需要 Spatial Analyst 扩展。 |

### 目录与数据发现（6）

| 工具 | 用途 |
|---|---|
| `list_workspace_contents` | 列出地理数据库或文件夹内的数据集；默认工程默认地理数据库。 |
| `list_folder` | 列出磁盘上的 GIS 文件与子文件夹；默认工程主目录。 |
| `describe_dataset` | 按路径描述任意数据集：类型、几何、坐标系、范围、字段与行数（无需加入地图）。 |
| `search_data` | 在工程地理数据库、主目录与文件夹连接中按名称查找数据，是加数据前最快的定位方式。 |
| `get_project_items` | 工程中注册的文件夹连接、数据库连接与工具箱。 |
| `add_folder_connection` | 把文件夹注册到工程，便于浏览其数据。 |

### 实用工具（4）

| 工具 | 用途 |
|---|---|
| `execute_arcpy_code` | 在 ArcGIS Pro 内运行 Python：arcpy 已导入，arcpy.mp.ArcGISProject('CURRENT') 为打开工程，变量跨调用保持，print 输出被捕获，末尾表达式值会返回。没有专用工具时用它。 |
| `get_pump_status` | 主线程调度器是否已安装；它决定命令能否访问打开的 Pro 工程，没有时只能做基于路径的工作。 |
| `stop_pump` | 移除主线程调度器；实时工程类命令将停止工作，直到用户重新安装。 |
| `run_batch` | 一次往返运行多条命令，多步工作流会快很多。 |

</details>

---

## 📦 包里有什么

| 路径 | 内容 | 说明 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | 🧩 add-in v1.1.4 | 113 个原生工具 + 全部 GP 工具；HTTP MCP 6520 / 旧 TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | 🌉 stdio 桥 | 给只能启动本地进程的客户端（如 Claude Desktop）转接 6520；纯 PowerShell，无需 Python |
| `install\runtime\runcode\RunCode.pyt` | 🐍 RunCode 工具箱 | 需要高频执行任意 arcpy 时使用；首次运行会弹一次信任框 |
| `install\runtime\guard\` | 🛡️ 弹窗守护 | 自动点击“第三方代码库执行”信任框；失败时置顶提醒（≥30 秒、响铃） |
| `install\runtime\plugin\` | 🔌 Python 桥（可选） | 在 Pro 的 Python 窗口里启动，提供 `execute_arcpy_code` 持久会话（端口 6511） |
| `install\*.cmd / *.ps1` | 🛠️ 安装与自检 | 安装 / 自检 / 配置 Claude / 弹窗守护 / 卸载 |
| `clients\` | 📁 客户端接入说明 | Claude Code、Claude Desktop、Codex、通用 JSON |
| `docs\` | 📖 使用指南、排障、工作规则、更新日志 | 先看使用指南 |
| `source\` | 📜 完整源码快照 | 与本包二进制对应的源码（AGPL-3.0 合规要求），含上游 LICENSE / NOTICE |

### 🔍 依赖审计（为什么说“全打包”）

- 🧩 add-in：自包含 C# 程序集，运行在 Pro 3.7 受支持环境自带的 .NET Desktop Runtime 10 x64 上——本包无需另装 .NET。
- 🌉 stdio 桥、🛡️ 弹窗守护：Windows 自带的 PowerShell 5.1 + WinForms——无需另装。
- 🐍 RunCode 工具箱、Python 桥：运行在 ArcGIS Pro 自带的 Python（arcpy）里——无需另装 Python。
- ✅ **因此本包没有任何需要 pip / 联网安装的外部依赖。**“离线可用”指本包自身的安装与运行链路；ArcGIS Pro 3.7 的安装前提（.NET Desktop Runtime 10 x64、WebView2 ≥132）由 Pro 的安装程序提供。

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

- 🏷️ 版本：**1.1.4**（构建日期见 `VERSION.json`；工具数 113）
- 👤 维护者：**Law**（[@XNYGggg](https://github.com/XNYGggg)）
- 🙏 上游项目与原作者：**[Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)**，作者 **Pisut Nakmuenwai**（AGPL-3.0-or-later）。本包在其基础上累计补丁至 1.1.4（修改点见 `docs\CHANGELOG.md` 与 `RELEASE-NOTES.md`）。
- ⚖️ 许可证：**AGPL-3.0-or-later**（本包附带完整源码快照与上游 NOTICE）
- 🏷️ 商标：“ArcGIS”为 Esri 商标；本项目为第三方工具，与 Esri 无隶属关系。

🆘 常见问题见 `docs\排障手册.md`；进阶用法见 `docs\使用指南.md`。祝你出图愉快 🎨 (｀・ω・´)ゞ
