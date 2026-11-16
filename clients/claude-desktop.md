# Claude Desktop 接入

Claude Desktop 不能直接连接 HTTP 服务，需要通过本包自带的 **stdio 桥**（纯 PowerShell，无需 Python）。

## 方法一：自动配置（推荐）

1. **完全退出 Claude Desktop**（含托盘图标）。
2. 双击 `install\02-configure-claude.cmd`。脚本会：
   - 找到本机实际的 Claude Desktop 配置目录（`%APPDATA%\Claude`、`%APPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude-3p`、`%LOCALAPPDATA%\Claude` 中已存在的那个）；
   - 备份原配置后写入 `mcpServers.arcgis`。
3. 重新打开 Claude Desktop，工具列表出现 `arcgis` 即成功。

## 方法二：手工写入

先退出 Claude Desktop，编辑其 `claude_desktop_config.json`，加入：

```json
{
  "mcpServers": {
    "arcgis": {
      "command": "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe",
      "args": [
        "-NoProfile",
        "-ExecutionPolicy", "Bypass",
        "-File",
        "C:\\Users\\<你的用户名>\\AppData\\Local\\ArcGIS Pro MCP\\stdio-bridge.ps1"
      ]
    }
  }
}
```

注意：Claude Desktop 运行期间会用自己的内存配置覆盖外部修改，**务必先退出应用再改**。
