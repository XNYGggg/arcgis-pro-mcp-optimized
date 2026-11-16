# 发布说明 · 客户版 v2.0.1（V2.01）· r2 文档修订版

## 这是什么版本

V2.01（v2.0.1）是 2.0 线的**实机验证补丁版**，也是本仓库当前推荐版本。本包为 **r2 文档修订包**：README / RELEASE-NOTES / VERSION.json 已更新（V2.01 升级说明、Windows 11 支持范围、已知边界、分支对照），**add-in 与全部运行时脚本零改动**（add-in SHA256 `3eda4fec…`，与初版资产一致）。

## V2.01 相对 V2.0 的升级

1. **守卫误报警**：信任弹窗的文字先于「是/允许」按钮出现在 UIA 树中。2.0.0 在首次扫描找不到按钮时立即弹 30 秒警报，即使一秒后自动点击成功。V2.01 在强上下文但按钮暂缺时等待 2.5 秒（日志 `dialog_button_wait`）再警报。实机用全新未信任 `.pyt` 验证：检测 → 点「是(Y)」→ 弹窗消失，全程无警报。
2. **`template_preview` 失败**：`run-distill.ps1` 使用 `[Parameter(ValueFromRemainingArguments)]`，PowerShell 把 `--out` 误绑定为公共参数 `-OutVariable`。V2.01 改为简单脚本 + `$args`，`template_preview` 重新返回 PNG。
3. **升级丢失模板库**：运行时安装原本整目录替换 `%LOCALAPPDATA%\ArcGIS Pro MCP`，会把用户的 `library/` 一起移到 `.bak`。V2.01 在替换前把 `library/` 合并进新运行时；本机重装实测模板列表仍为 1，`template_preview` 正常。

## 下载与安装

- 📦 当前资产：`ArcGIS-Pro-MCP-v2.0.1-r2.zip`（本包）
- 🗄️ 初版资产 `ArcGIS-Pro-MCP-v2.0.1.zip` 保留可查；`ArcGIS-Pro-MCP-2.0.1.zip` 为 NuGet 安装器兼容别名（与 r2 字节相同）
- 🛺 一条命令：`dotnet tool install --global ArcGISProMCP.Install --version 2.0.1`
- 📜 校验：同名 `.sha256` 附件与包内 `SHA256SUMS.txt`（231 个文件全覆盖）

## 组件

| 组件 | 版本 / 内容 |
|---|---|
| add-in | **2.0.1**，125 个 MCP 工具（113 上游 + `guard_summon` / `guard_status` + 10 `template_*`） |
| 按需守卫 | `guard.ps1` 2.0.1：按钮等待修复；规则 schema；旧版兼容层 |
| 模板蒸馏 | `runtime\distill\`：`run-distill.ps1` 参数修复；Pro 自带 Python（含 Pillow） |
| 既有运行时 | stdio 桥、RunCode、Python 桥、安装 / 自检 / 卸载脚本、文档 |
| 源码快照 | `source/`（AGPL-3.0-or-later，含 2.0.1 修改） |

## 实机验证（ArcGIS Pro 3.7，本机）

- MCP initialize 返回 `2.0.1.0`，`tools/list` 125 个；`ping`、`get_project_info`、`get_layers`、`activate_map`、`export_map_view` 均通过。
- `run_python_toolbox_tool` 执行 RunCode.pyt：自动召唤守卫（`guard.ready=true`），全新未信任工具箱触发信任框并被自动点击「是(Y)」，RunCode 输出正确。
- `guard_summon` 手动召唤：ready=true，按超时收队。
- `template_*` 全链路：import → measure → patch → validate → preview（返回 PNG，含样本并排）→ review → approve → apply 全部通过；库位于 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`。

## 已知边界

- 🔏 add-in 未签名：首次加载需在 Pro 内点「信任」；部分组织的安全策略可能阻止未签名加载项，需管理员放行。
- 🧪 干净 Windows 虚拟机全流程演练尚未完成——建议先在测试机试装。
- 🪟 仅 Windows；ArcGIS Pro 3.7 已验证，更新版本未验证（ArcGIS Pro 3.7 官方支持 Windows 11）。
- 🚨 覆盖 / 删除类确认框的负向测试仍以规则测试为主，未在真实弹窗矩阵上穷举。
