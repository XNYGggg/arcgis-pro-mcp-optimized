# Guard · 按需弹窗守卫（2.0.1）

守卫只做一件事：在 ArcGIS Pro 弹出“第三方代码库 Python 命令执行”信任框时，
替你点「是 / 允许」；点不到就置顶警报（≥30 秒、每 10 秒响铃）。它**只在操作期间
存在**：没有危险操作时，系统中没有守卫进程、没有轮询。

## 1. 何时会被召唤

| 场景 | 召唤方式 |
|---|---|
| MCP `run_python_toolbox_tool`（RunCode.pyt 等） | add-in `GeoprocessingCommands` 自动 `Arm` → 执行 → `Settle` |
| MCP `run_geoprocessing_tool` 且工具来自 `.pyt/.tbx/.atbx` | 同上（按工具箱扩展名判断） |
| ENVI→Pro 导入、手工跑脚本等不经过 GP 的步骤 | `guard_summon` 工具，或 `arcgis-guard summon` |
| 用户自己的批处理 | `arcgis-guard wrap -- <命令>` |

守卫不可用时**不阻塞业务**：操作照常执行，结果里带 `guard_warning`（MCP）或
CLI 红色警告，提示人工点击。

## 2. 握手协议

状态根：`%LOCALAPPDATA%\ArcGIS-AgentKit\guard\sessions`（`ARCGIS_AGENTKIT_HOME`
可覆盖）。

```
requests\<token>.json     召唤请求（进程启动后立即写）
<token>\ready.json        {pid, token, scope, armed_at, engine_version, pro_pids, rules}
<token>\settle            调用方在操作结束时写
<token>\result.json       {detected, clicked, alerted, resolved, exit_reason, events[]}
```

- 单引擎多令牌：互斥锁 `Local\ArcGISProDialogWatch_v2`；后到的召唤把请求写入
  `requests\`，由已运行的引擎 arm 并写 `ready.json`，随后自身退出。
- 所有令牌 settle/超时后，引擎再等 3 s 宽限（期间无新弹窗）才退出。
- 绝对超时：`op` 默认 180 s，`manual` 默认 600 s；可在规则 JSON 调整。

## 3. 检测与安全边界

- 主通道：UIA `WindowOpened` 事件（按 Pro PID 过滤）；兜底：被召唤期间 400 ms
  轮询（宽限期 1.5 s）。事件通道失败时自动退化为纯轮询，不影响安全边界。
- 判定规则外置在 `guard.schema.json`：

| 条件 | 动作 |
|---|---|
| 命中 deny（覆盖/删除/移除/卸载/overwrite/delete/replace…） | 只警报，**绝不点击** |
| 强上下文（第三方/代码库/third-party/code library）+「是/允许」 | 点击 |
| 强上下文 + 仅「确定/OK」 | 点击 |
| 弱上下文（Python）+「允许/执行/信任」按钮 | 点击（日志标 weak） |
| 弱上下文但没有肯定按钮 / 无法判定 | 不点击；强上下文时警报 |

`alert.min_seconds` 低于 30 一律按 30 执行（代码内硬校验）。

## 4. CLI

```text
arcgis-guard doctor                      环境与引擎自检
arcgis-guard summon [--scope manual] [--timeout 120] [--alert-only] [--no-wait]
arcgis-guard wrap -- <任意命令>          召唤 → 执行 → 收队，透传退出码
arcgis-guard status                      最近 20 个会话（ready/settle/result）
arcgis-guard test [--seconds 30]         警报窗可见性测试（≥30 s）
arcgis-guard self-test [--text "..."]    扫描当前 Pro 窗口，或用文本测试规则
```

全部支持 `--json`；退出码：0 成功 / 2 参数 / 3 环境 / 4 运行失败 / 5 校验。

## 5. MCP 工具

- `guard_summon(scope="manual", timeout_s=600, alert_only=false)`：返回 token、
  ready、session 路径；守卫按 timeout 自行退出，也可由操作方写 settle。
- `guard_status()`：引擎路径/版本、会话根目录、最近 20 个会话摘要。

## 6. 旧版迁移

`watch_pro_dialog.ps1` 仍可用，但已改为兼容 shim，转发到新引擎并接受旧参数
（`-DurationSeconds / -AlertOnly / -NotifyTest / -TestSeconds / -AlertMinSeconds /
-SelfTest`）。原 CLAUDE.md 中“每次会话首次 RunCode 前人工启动守护”的规则可以删除：
2.0 的 add-in 会自动 arm/settle。
