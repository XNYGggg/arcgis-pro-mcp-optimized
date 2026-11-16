# Claude Code 接入

1. 确认 ArcGIS Pro 已打开，MCP 选项卡显示 running（端口 6520 在监听）。
2. 在终端执行：

```powershell
claude mcp add --transport http arcgis http://127.0.0.1:6520/mcp
```

3. 验证：

```powershell
claude mcp list
```

列表中应出现 `arcgis`。之后在 Claude Code 里直接说“把当前工程按土地利用类型配色并导出 PNG”之类的话即可。

- 如需重新添加：先 `claude mcp remove arcgis`，再执行添加命令。
- 本客户版的 `02-configure-claude.cmd` 会自动完成这两步（检测到 `claude` 命令时）。
