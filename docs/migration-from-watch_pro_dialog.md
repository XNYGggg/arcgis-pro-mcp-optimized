# 从 watch_pro_dialog.ps1 迁移到 Guard 2.0

## 变好了什么

| 维度 | 旧 watch_pro_dialog.ps1 | Guard 2.0 |
|---|---|---|
| 生命周期 | 墙钟常驻 120–600 s | 操作级一次性，结束后零进程 |
| 启动 | 人工按规则启动 | MCP 自动召唤 / CLI 一条命令 |
| 检测 | 700 ms 轮询 | UIA 事件优先 + 兜底轮询（仅被召唤期间） |
| 规则 | 硬编码 | `guard.schema.json` 外置，可覆盖、可测试 |
| 警报窗 | 依赖外部 MacDialog 路径 | 内置渲染，零外部依赖 |
| 日志 | 纯文本 | JSONL（含命中规则与证据文本） |
| 与 MCP | 无 | add-in 自动 arm/settle + 两个工具 |

## 操作变化

1. **不再需要**“每次 Pro 会话首次运行 RunCode 前手动启动守护”。2.0 的
   `run_python_toolbox_tool` 与 `.pyt/.tbx/.atbx` GP 调用会自动召唤；结果里
   `guard.ready` 为 true 表示已接管，出现 `guard_warning` 才需要人工点击。
2. 单独场景（ENVI→Pro 导入、手工跑脚本）：
   ```powershell
   arcgis-guard summon --scope manual --timeout 300
   ```
   或 MCP 调用 `guard_summon`；也可把任意命令包起来：
   ```powershell
   arcgis-guard wrap -- python RunSomething.py
   ```
3. 旧启动器仍可用：`install\03-dialog-guard.cmd` 与
   `runtime\guard\watch_pro_dialog.ps1` 已改为兼容 shim，参数不变。
4. 规则定制：把覆盖片段写到 `%LOCALAPPDATA%\ArcGIS-AgentKit\config.json` 的
   `modules.guard.rules_path`，或设置 `ARCGIS_AGENTKIT_GUARD_ENGINE` /
   `ARCGIS_AGENTKIT_GUARD_RULES`。`alert.min_seconds` 永远不会低于 30。

## 仍要保留的人工确认

守卫只点“第三方代码库执行”信任框。覆盖输出、删除、移除、卸载等确认框一律
只警报、不点击——这是硬编码的安全边界，配置也无法放宽。
