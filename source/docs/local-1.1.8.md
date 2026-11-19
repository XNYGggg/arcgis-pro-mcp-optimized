# ArcGIS Pro MCP 本地升级 1.1.8（P1 修复 · 仅本机）

- 日期：2026-11-18
- 基线：v1.1.7（`3B3CA9A5238CAFA0D4EAC360BEAE28D765A64F2220ACB23C8BF3756EC31DEE63`，451,790 字节）
- 依据：`docs\插件改进方案-2026-11-18.md` §七（第 29 轮三方会谈 P1 排序）
- 源码：`release\v1.1.6-src`（版本 1.1.8）；p0-backup\ 仍为 1.1.6 原文件

## 变更（P1）

| 项 | 内容 | 主要文件 |
|---|---|---|
| D2 独立表写入 | insert/update/delete 接受地图内独立表与数据集全路径：layer 仍走 EditOperation（可撤销），表/路径改走 Table API（CreateRow/Store/Delete），结果标注 `target:"table"` 与"无编辑会话" | EditCommands.cs、DataSource.cs（导出 Table） |
| D3 采样 | `sample_raster_values` 改为原生像素采样：`points` 数组（{x,y} 或 [x,y]）与 `point_layer`（任意位置，自动投影到栅格 SR）都支持；值内联返回，不再写默认 GDB 的 `raster_samples`；`write_table=true` 保留旧 sa.Sample 路径（默认唯一表名） | RasterCommands.cs（新原生命令；GpWrapper 移除旧 Wrap） |
| S1 场景 | `create_map(map_type=SCENE)` 使用 `MapViewingMode.SceneLocal`，不再报"3D 地图视图无法更改为地图"；MAP 行为不变 | MapCommands.cs |
| J1/J2 连接读取 | GP AddJoin 会让 .NET 图层视图失效（"does not have a valid definition"）。改为注册表 + 客户端连接：add_join 在 MCT 上先捕获基表路径与连接表字段；get_features 对连接图层直接读基表+连接表按关键字段合并；未限定字段自动解析，省略 fields 时附带 `Table.Field`，缺失字段报错带完整候选；list_fields 同步回退到基表并列出连接字段 | JoinRegistry.cs（新）、DataCommands.cs、EditCommands.cs、GpWrapperCommands.cs |
| L1 布局坐标 | 元素 x/y 统一为**页面单位左下角**；创建/移动后读 `GetBounds` 回读实际框；偏移 >0.02 页面单位报错；超出页面报错。修图例/图片/指北针 SetY 取上边缘导致的错位 | LayoutCommands.cs |
| 批次计时 | run_batch 每步返回 `ms`，并标注 `execution:"serial"` 与"主线程串行、同资源写入不会交错" | SessionCommands.cs |
| 入图竞态 | GP 输出入图是延后完成的：Wrap 会等待成员出现（最多 2 秒）；DataSource.Resolve 对简单名称做 1 秒最终一致性重试。修复"建表后立即插入报找不到表" | GpWrapperCommands.cs、DataSource.cs |

- 版本：Config.daml / csproj / pyproject = 1.1.8；命令数 112、工具数 115 不变；tools.json 重新生成（sample_raster_values 参数更新）。
- 已知范围：客户端连接只覆盖通过 add_join 建立的连接；Pro 界面手工建的连接仍可能触发图层视图失效，建议先 remove_join 再按路径读取。

## 构建与部署

- 构建：`scripts/build.ps1`（Release，未签名）。
  **最终产物 SHA256** `42085488BDED9C5E7C825A77D7D0DA5B9CD945A4504E94E9D25E5FA85C407CA4`，465,565 字节。
- 安装：`scripts/install_addin.ps1 -AddInPath <candidate> -Reinstall`。
- 登记：`scripts/update_installed_118.py` 经 RunCode.pyt 回写 installed.json（version=1.1.8）。
- 重启后握手 **1.1.8.0**、115 工具 / 112 命令。
- 回滚：恢复 `Documents\ArcGIS\AddIns\ArcGISPro\ArcGISProMCP.esriAddinX.bak-*` 中的 1.1.7 版本（或 `release\v1.1.7-artifacts` 目录），并回写 installed.json 后重启 Pro。

## 实机回归（`output\mcp_tests_20261118\p1_regression_118.py`）

- 连续两轮 **0 失败**（每轮 27 项断言）：D2 表名/路径增改删与计数；D3 points 与 point_layer 数值一致、无 raster_samples 副作用；S1 场景创建/加 TIN/删除；J1 未限定连接字段读取、J2 全字段含 `P1REF.NOTE`、缺失字段候选；L1 图例/图片左下角定位、set 元素 y 回读、越界拒绝；批次 ms 与 serial 标注。
- 过程缺陷（均已修复并复测）：连接后图层视图失效 → 客户端连接；add_join 路径/字段在非 MCT 线程解析为空 → QueuedTask.Run；元素 SetY 上边缘语义 → 左下角换算；入图延后 → 等待+重试。
