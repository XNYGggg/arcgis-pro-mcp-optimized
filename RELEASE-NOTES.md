# 发布说明 · 客户版 v1.1.4（2026-11-16）

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
