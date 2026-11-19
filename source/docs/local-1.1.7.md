# ArcGIS Pro MCP 本地升级 1.1.7（P0 修复 · 仅本机）

- 日期：2026-11-18
- 基线：v1.1.6（`96C7829E2A26E16C270722A84605F9945DA2348DFCF202C658A64EA9E012E625`，444,124 字节）
- 依据：`docs\插件改进方案-2026-11-18.md` §七（第 29 轮三方会谈 P0 排序）
- 源码：`release\v1.1.6-src`（沿用目录，版本号已升 1.1.7）；原文件备份在 `release\v1.1.6-src\p0-backup\`

## 变更（P0 三项）

| 项 | 内容 | 主要文件 |
|---|---|---|
| D1 字段 fail-loud | create_feature_class / create_table 在创建后逐字段 AddField，并用 describe 回读校验名称/类型/TEXT 长度；不一致或失败即删除本次数据集并抛错；非法字段名/类型在创建前拒绝。移植自 2.0.2 实现，适配 1.1.x 的 Wrap/RunNamedAsync | GpWrapperCommands.cs、CatalogCommands.cs（新增 `DescribeDataset(path)`） |
| 批量图片剥离 | `run_batch` 内 export_map_view / export_layout / preview_layout 默认不内联图片，改为返回 path/size/SHA-256；显式 `return_image=true` 才内联。批次标志由 `Params.InBatch` 显式携带（AsyncLocal 在 MCT 调度下不可靠） | Protocol.cs、SessionCommands.cs、ViewCommands.cs、LayoutCommands.cs、MapHelpers.cs（`Sha256`、`WaitForFile`） |
| get_features 精简 | 结果带 `truncated` / `next_offset`；`geometry_summary=true` 返回 type/extent/length/area；`geometry_precision=N` 对 WKT 坐标取整；几何摘要时强制加载 Shape | DataCommands.cs、catalog.py（→ tools.json 重新生成，115 工具） |

- preview_layout 新增 `output_path`（写出图片）与 `return_image`；未内联时返回 size/hash。
- 导出后短暂等待文件落盘（`WaitForFile`，2 秒上限），避免刚写完的 PNG 被漏读。
- 版本号：Config.daml / csproj / pyproject 均 1.1.7；命令数 112、工具数 115 不变。

## 构建与部署

- 构建：`scripts/build.ps1`（Release，未签名，Pro 3.7 / net10.0-windows）。
  **最终产物 SHA256** `3B3CA9A5238CAFA0D4EAC360BEAE28D765A64F2220ACB23C8BF3756EC31DEE63`，451,790 字节。
- 安装：`scripts/install_addin.ps1 -AddInPath <candidate> -Reinstall`（官方安装器；本机 Add-in security = 0）。
- 登记：`scripts/update_installed_117.py` 经 RunCode.pyt 回写 `%LOCALAPPDATA%\ArcGIS Pro MCP\installed.json`（version=1.1.7、hash、size）。
- 重启：Pro 重启后握手 **1.1.7.0**、115 工具 / 112 命令。
- 回滚：恢复 `Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX` 为最近备份
  `*.bak-20261118-200937`（1.1.6，96C7829E）或 `release\v1.1.7-artifacts\installed-before-upgrade.esriAddinX`；
  回溯源码用 `p0-backup\`。

## 实机回归（`output\mcp_tests_20261118\p0_regression_117.py`，21/21 PASS）

- D1：FC/表字段齐全（含 TEXT 长度 50）；非法类型拒绝且零残留；重复字段触发 fail-loud 并删除半成品。
- get_features：分页 truncated/next_offset 正确；geometry_summary 返回 Point+extent；WKT 精度 2 位使回包 8679→5602 字符。
- 批量：export_map_view 批内回包 691 字符（无 base64，有 bytes/sha）；layout 批内 524 字符。
- 单次：export_map_view / preview_layout 仍以 MCP image 内容块返回图片（服务端拆分 image_base64）。
