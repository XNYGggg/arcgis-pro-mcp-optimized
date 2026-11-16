# 分支、标签与 Release 对照表（BRANCHES.md）

> 最后更新：2026-11-16 · 本表是仓库分支治理的权威记录；新增/删除分支、移动标签、修订 Release 资产时请同步更新。

## 当前分支

| 分支 | 角色 | 说明 |
|---|---|---|
| **`main`**（默认分支） | 开发主线 | AgentKit 2.0.1：MCP add-in 2.0.1 + 守卫 2.0 + 蒸馏 2.0；README 与全部文档以此为准 |
| **`customer-v2.0.1`** | 当前客户版包快照 | 与 Release v2.0.1 的 `ArcGIS-Pro-MCP-v2.0.1-r2.zip` 内容一致（安装器、运行时、文档、AGPL 源码快照） |
| `customer-v1.1.4` | legacy 1.x 客户版 | add-in 1.1.4 完整离线包，仅作历史保留；README 顶部已标注 legacy |

## 标签（Tag）

| 标签 | 类型 | 指向 | 说明 |
|---|---|---|---|
| `v2.0.1` | annotated | commit `61043d7` | V2.01 正式版标签。**不可移动**：同名版本必须始终对应同一提交 |
| `v2.0.0` | annotated | commit `f24da90` | 2.0 首个预览版，历史保留 |
| `v1.1.4` | lightweight | commit `1d15a45` | 1.x 稳定版，历史保留 |

## Release

| Release | 状态 | 资产 | 说明 |
|---|---|---|---|
| **v2.0.1** | ✅ Latest（正式版） | `ArcGIS-Pro-MCP-v2.0.1-r2.zip` + `.sha256`（当前）<br>`ArcGIS-Pro-MCP-2.0.1.zip` + `.sha256`（安装器兼容别名，与 r2 字节相同）<br>`ArcGIS-Pro-MCP-v2.0.1.zip` + `.sha256`（初版资产，保留） | V2.01 正式发布；r2 为文档修订包，二进制与初版完全一致 |
| v2.0.0 | 🧪 Pre-release（保留） | `ArcGIS-Pro-MCP-v2.0.0.zip` + 兼容别名 `ArcGIS-Pro-MCP-2.0.0.zip` | 历史预览，已被 v2.0.1 取代 |
| v1.1.4 | 📦 Published（保留） | `ArcGIS-Pro-MCP-v1.1.4.zip` + 兼容别名 `ArcGIS-Pro-MCP-1.1.4.zip` | 1.x legacy，已被 v2.0.1 取代 |

### v2.0.1 资产说明（2026-11-16 修订）

| 资产 | SHA256 | 大小 | 状态 |
|---|---|---|---|
| `ArcGIS-Pro-MCP-v2.0.1-r2.zip` | `478ace7999724ca22f4aa00fb5c4b4429a39cc566de949802c5a294b9192c950` | 1,000,593 B | **当前下载**；仅 README / RELEASE-NOTES / VERSION.json 更新 |
| `ArcGIS-Pro-MCP-2.0.1.zip` | 同上（字节相同） | 1,000,593 B | NuGet 安装器 2.0.1 的兼容别名 |
| `ArcGIS-Pro-MCP-v2.0.1.zip` | `1af34f03d23709f21fa4506ded92eec993a488d7f7b2c8da62b136f278103d9f` | 996,033 B | 初版资产，保留可查（不再作为下载入口） |

- **二进制不可变承诺**：r2 与初版相比，231 个文件中仅 3 个文档变化；add-in（`3eda4fec…`）与全部运行时脚本、源码快照逐文件哈希一致（见仓库 `release/out/rebuild-diff-v2.0.1-r2.txt`）。
- **NuGet 别名**：已发布的 NuGet 工具（1.1.4 / 2.0.0 / 2.0.1）按 `ArcGIS-Pro-MCP-{版本}.zip`（不带 `v`）拼下载 URL，而历史资产名都带 `v`。为不让已发布工具 404，三个 Release 各补了一个**字节相同**的兼容别名资产。安装器源码已改为"先试带 `v` 名、404 回退无 `v` 名"，随下一个 NuGet 版本发布。

## 已归档分支（已删除，SHA 可恢复）

| 分支 | 归档前 SHA | 归档日期 | 内容与恢复 |
|---|---|---|---|
| `customer-v2.0.0` | `e8ebda5` | 2026-11-16 | 与 `main` 树完全相同（代码布局），已由 `customer-v2.0.1` 取代。恢复：`git push origin e8ebda5:refs/heads/customer-v2.0.0` |
| `history-main-pre-2.0` | `6be852d` | 2026-11-16 | 2.0 合并前的 main，是 `main` 的祖先提交，内容未丢失。恢复：`git push origin 6be852d:refs/heads/history-main-pre-2.0` |

## 变更记录

- **2026-11-16**：V2.01 正式发布与仓库重整。
  - 默认分支由 `customer-v1.1.4` 改回 `main`；新建 `customer-v2.0.1` 包快照分支。
  - 删除 `customer-v2.0.0`、`history-main-pre-2.0`（SHA 见上表）。
  - `v2.0.1` 保持不可变，Release 由 pre-release 转正并设为 Latest；新增 r2 文档修订包与兼容别名资产。
  - 安装器源码修复资产名回退逻辑（随下一 NuGet 版本发布）。
- **2026-11-16（早）**：v1.1.4 发布、NuGet 安装器 1.1.4 上线；2.0.0/2.0.1 预览发布。

## 约定

1. **标签不可变**：任何已发布的 tag 不移动、不重打；需要新内容时升版本号或新增带后缀的资产。
2. **Release 资产只增不改**：已上传资产不覆盖；文档修订用新文件名（如 `-r2`），旧资产保留。
3. **客户版包快照**：每个正式版本的离线包在独立 `customer-vX.Y.Z` 分支留档，内容与 Release 资产一致。
4. **删除分支前**：先把依赖它的默认分支/文档链接改完，并在本表记录 SHA 与恢复命令。
