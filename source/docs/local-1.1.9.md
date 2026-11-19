# ArcGIS Pro MCP 本地升级 1.1.9（P2 修复 · 仅本机）

- 日期：2026-11-18
- 基线：v1.1.8（`42085488BDED9C5E7C825A77D7D0DA5B9CD945A4504E94E9D25E5FA85C407CA4`，465,565 字节）
- 依据：`docs\插件改进方案-2026-11-18.md` §七 P2 项
- 源码：`release\v1.1.6-src`（版本 1.1.9）

## 变更

| 项 | 内容 | 主要文件 |
|---|---|---|
| I1/R1 路径解析 | `MapHelpers.FindLayer` 支持"数据集路径 → 唯一已加载图层"，多匹配时报错并列出图层名；`get_layer_info`、`set_raster_symbology`、`repair_layer_source` 随之接受路径 | MapHelpers.cs、RasterCommands.cs、LayerCommands.cs |
| D4 栅格修复 | `repair_layer_source` 增加 BasicRasterLayer 分支，经 RasterDataset 重定向；实测可把复制图层指到副本栅格 | LayerCommands.cs |
| search 去重 | 默认搜索根会重复包含同一 GDB（工作区与其父目录），按工作区去重，结果不再重复 | CatalogCommands.cs |
| check_extension | 返回 `status:"not_inspectable", inspectable:false`，不再用易误解的 "unknown"；说明这是不可探测，不是许可状态 | GeoprocessingCommands.cs |
| outputs 空串 | GP 包装命令的 outputs 过滤空字符串（zonal_statistics 不再带尾随 ""） | GpWrapperCommands.cs |
| get_features 报错自洽 | 传入栅格图层名时，"找不到"错误会指明它是栅格，并提示用 get_raster_info / sample_raster_values | DataSource.cs |
| 候选错误 | 未改变：J1/J2 候选清单、D1 fail-loud 等 1.1.7/1.1.8 行为保持 | — |

## 决策记录

- **pyt 预热不实现**：实测每个 Pro 进程首个 RunCode ≈0.67 s、热调用 ≈25 ms。预热只是把这次冷启动挪到别处，只有"确定马上要跑批"时才划算；已在 CHANGELOG 记录，不引入隐式调用。
- **SKIP 项**：Python 桥三工具（需用户手动粘贴启动，非插件缺陷）；网络分析（工程无网络数据集且需 Network 扩展）；地图系列（未构造测试布局，留待有实际版面需求时补测）。

## 构建与部署

- 构建：`scripts/build.ps1`（Release，未签名）。
  **最终产物 SHA256** `1574BF0D5D1CB5D600506F02F82781EF9DC784E68C18CE3E29D1EE4000C6F39C`，466,234 字节。
- 安装：`scripts/install_addin.ps1 -AddInPath <candidate> -Reinstall`；登记：`scripts/update_installed_119.py`（RunCode.pyt）。
- 重启后握手 **1.1.9.0**、115 工具 / 112 命令。
- 回滚：恢复 `Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX.bak-*` 的 1.1.8 版本并回写 installed.json。

## 实机回归

- `p2_regression_119.py`：**10/10 PASS**——路径图层信息/栅格符号化、多匹配拒绝、search 去重、check_extension、zonal outputs、栅格提示、栅格修复（副本栅格 CopyRaster 后重定向 connected=true）。
- `p1_regression_118.py` 在 1.1.9 上重跑：**0 失败**（确认无回退）。
