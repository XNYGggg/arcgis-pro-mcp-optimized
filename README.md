# ArcGIS Pro MCP 优化版 · 客户版 v1.1.4

用一句自然语言，让 AI 助手直接操作你正开着的 ArcGIS Pro 工程：加图层、改符号、查属性、跑地理处理、出图导出。数据不出本机，所有通信都走 `127.0.0.1`。

本客户版把运行所需的**全部组件**打包在一起，离线可用，**不需要安装 Python、不需要 pip、不需要联网**，除 ArcGIS Pro 本身外没有任何额外依赖。

---

## 系统要求

| 项目 | 要求 |
|---|---|
| 操作系统 | Windows 10 / 11（64 位；以 ArcGIS Pro 3.7 官方支持的系统为准，Pro 3.7 推荐 Windows 11） |
| ArcGIS Pro | **3.7 或更高**（本 add-in 基于 3.7 / .NET 10 构建） |
| 运行环境 | ArcGIS Pro 3.7+ 的受支持安装（自带 .NET Desktop Runtime 10 x64 与 WebView2 ≥132）；本包不另行安装 |
| 权限 | 普通用户即可，安装**不需要管理员** |
| 其他依赖 | 无（PowerShell 与 .NET 由 Windows / Pro 自带） |

---

## 5 分钟上手

1. **解压**本包到任意目录（例如 `D:\ArcGIS-Pro-MCP-客户版`）。
2. **关闭 ArcGIS Pro**，双击 `install\00-install.cmd`。脚本会自动安装 add-in、部署运行时、并尝试配置 Claude Desktop / Claude Code。
3. **打开 ArcGIS Pro**。首次加载时如提示“加载项不受信任”，点“是 / 信任”。在 Pro 顶部找到 **MCP 选项卡**，确认状态为 **running**（端口 `127.0.0.1:6520`）。
4. **接入你的 AI 客户端**（任选其一，见下节），然后双击 `install\01-check.cmd` 自检，全部 `[OK]` 即可使用。

> 自检时若 ArcGIS Pro 没开，端口检查会显示警告——先打开 Pro 再跑一次即可。

---

## 接入 AI 客户端

| 客户端 | 做法 |
|---|---|
| **Claude Code** | 终端运行：`claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp`，然后 `claude mcp list` 确认 |
| **Claude Desktop** | 完全退出应用后运行 `install\02-configure-claude.cmd`（自动写配置并备份） |
| **Codex** | 在 `~/.codex/config.toml` 中加入 `[mcp_servers.arcgis]` 与 `url = "http://127.0.0.1:6520/mcp"` |
| **其他 MCP 客户端** | 以 Streamable HTTP 方式连接 `http://127.0.0.1:6520/mcp` |

详细说明见 `clients\` 目录。

---

## 包里有什么

| 路径 | 内容 | 说明 |
|---|---|---|
| `install\ArcGISProMCP.esriAddinX` | add-in v1.1.4 | 113 个原生工具 + 全部 GP 工具；HTTP MCP 6520 / 旧 TCP 6510 |
| `install\runtime\stdio-bridge.ps1` | stdio 桥 | 给只能启动本地进程的客户端（如 Claude Desktop）转接 6520；纯 PowerShell，无需 Python |
| `install\runtime\runcode\RunCode.pyt` | RunCode 工具箱 | 需要高频执行任意 arcpy 时使用；首次运行会弹一次信任框 |
| `install\runtime\guard\` | 弹窗守护 | 自动点击“第三方代码库执行”信任框；失败时置顶提醒（≥30 秒、响铃） |
| `install\runtime\plugin\` | Python 桥（可选） | 在 Pro 的 Python 窗口里启动，提供 `execute_arcpy_code` 持久会话（端口 6511） |
| `install\*.cmd / *.ps1` | 安装与自检 | 安装 / 自检 / 配置 Claude / 弹窗守护 / 卸载 |
| `clients\` | 客户端接入说明 | Claude Code、Claude Desktop、Codex、通用 JSON |
| `docs\` | 使用指南、排障、工作规则、更新日志 | 先看使用指南 |
| `source\` | 完整源码快照 | 与本包二进制对应的源码（AGPL-3.0 合规要求），含上游 LICENSE / NOTICE |

### 依赖审计（为什么说“全打包”）

- add-in：自包含 C# 程序集，运行在 Pro 3.7 受支持环境自带的 .NET Desktop Runtime 10 x64 上——本包无需另装 .NET。
- stdio 桥、弹窗守护：Windows 自带的 PowerShell 5.1 + WinForms——无需另装。
- RunCode 工具箱、Python 桥：运行在 ArcGIS Pro 自带的 Python（arcpy）里——无需另装 Python。
- **因此本包没有任何需要 pip / 联网安装的外部依赖。**“离线可用”指本包自身的安装与运行链路；ArcGIS Pro 3.7 的安装前提（.NET Desktop Runtime 10 x64、WebView2 ≥132）由 Pro 的安装程序提供。

---

## 可选组件怎么用

**RunCode 工具箱**（高频 arcpy 场景）
- 路径：`%LOCALAPPDATA%\ArcGIS Pro MCP\runcode\RunCode.pyt`
- 让 AI 调用 `run_python_toolbox_tool` 时传入该路径即可；或在 Pro 里“插入 → 工具箱 → 添加工具箱”选择它。
- 每个 Pro 会话首次运行会弹“第三方代码库”信任框：可先运行 `03-dialog-guard.cmd`，守护会自动点“是”。

**Python 桥**（需要跨调用保持 arcpy 会话时）
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

**弹窗守护**（推荐在跑 RunCode / ENVI→Pro 联合流程前启动）
```
03-dialog-guard.cmd                 # 600 秒守护
03-dialog-guard.cmd -DurationSeconds 120
03-dialog-guard.cmd -NotifyTest     # 只测提醒窗（30 秒）
03-dialog-guard.cmd -AlertOnly      # 只提醒，不自动点击
```
安全边界：只处理带“第三方 / 代码库 / third-party / code library”上下文的信任框；**绝不触碰覆盖、删除等其他确认框**。日志在 `%LOCALAPPDATA%\ArcGIS Pro MCP\guard\watch_pro_dialog.log`。

---

## 安全说明

- add-in 与所有桥接服务只监听 `127.0.0.1`，局域网其他机器无法连接。
- 本软件仅在 `127.0.0.1` 提供本机服务，不向第三方提供网络服务；如你在修改后通过网络向他人提供服务，AGPL-3.0 §13 的源码提供义务适用。
- add-in 为未签名构建：首次加载时 ArcGIS Pro 会要求你确认信任，这是正常流程；请核对本包 `SHA256SUMS.txt`。
- 本包不包含任何遥测或联网上报。
- 卸载：`99-uninstall.cmd`（可选择保留客户端配置）。

---

## 版本与许可

- 版本：**1.1.4**（构建日期见 `VERSION.json`；工具数 113）
- 许可证：**AGPL-3.0-or-later**（衍生自 [Knight60/ArcGIS-Pro-MCP](https://github.com/Knight60/ArcGIS-Pro-MCP)，其同样为 AGPL-3.0-or-later；本包附带完整源码与上游 NOTICE）
- 商标：“ArcGIS”为 Esri 商标；本项目为第三方工具，与 Esri 无隶属关系。

常见问题见 `docs\排障手册.md`；进阶用法见 `docs\使用指南.md`。
