# Codex 接入

Codex 支持 Streamable HTTP，可直接连接 add-in。

编辑 `~/.codex/config.toml`，加入（如已有旧 `arcgis` 段，整段替换，包括旧的 `env` 子表）：

```toml
[mcp_servers.arcgis]
url = "http://127.0.0.1:6520/mcp"
```

保存后重启 Codex。启动 ArcGIS Pro 并确认 MCP 选项卡 running 即可使用。

> 参考：上游 README 的客户端接入说明（Knight60/ArcGIS-Pro-MCP）。
