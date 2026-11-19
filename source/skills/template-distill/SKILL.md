---
name: template-distill
description: Distill a sample map figure into a reusable layout template spec (import → measure → patch → validate → preview → review → approve → apply).
---

# Template Distill · 语义蒸馏作业指导（客户版 1.1.x，模块移植自 2.x；1.1.13 起含删除）

把一张样本成图变成可复用的版式模板。确定性步骤（测量/校验/预览/入库）全部由
`arcgis-distill` 完成；你只负责**看图写语义字段**。全程一次性命令，无守护进程。

## 0. 前置

- 用户明确说“把这张图蒸馏成模板”才执行，不要自动触发。
- 样例图若是期刊/教材图，默认**只入本地库**、不导出公开 bundle（版权）。
- 库位置：仓库 `library/`；客户版 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`；
  可用 `ARCGIS_DISTILL_LIBRARY` 覆盖。

## 1. 入库与测量（禁止手改 measured）

（客户版命令入口：`%LOCALAPPDATA%\ArcGIS Pro MCP\distill\run-distill.ps1`，
下文示例中的 `arcgis-distill` 均指它；MCP 场景直接调用对应的 `template_*` 工具。）

```text
arcgis-distill import <样本图> --name "研究区总览图" --tag 论文风格 --width-in 6.7 --json
arcgis-distill measure <id> --json
```

- `import` 按图片 sha256 去重；返回 `template_id`。
- `measure` 写入 `measured`：`figure_bbox / aspect / canvas_height_in / h_lines /
  v_lines / palette_top / dpi`。**永远不要手改这些数值**；要改画布就重跑
  `measure --force` 或用 `canvas.height_in` 对齐（容差 3%）。

## 2. 看样本图，写 patch

对照样本图与 `measure` 的 `h_lines/v_lines`（归一化 0–1，左/上为 0），产出一个
patch JSON（只写要改的字段，深合并）：

```json
{
  "map_frame": {"x": 0.076, "y": 0.139, "width": 0.849, "height": 0.723},
  "title": {"text": "研究区总览图", "size": 22, "align": "center", "color": "#1F2328"},
  "legend": {"x": 0.76, "y": 0.62, "w": 0.14, "h": 0.17, "columns": 1},
  "scale_bar": {"x": 0.10, "y": 0.90, "w": 0.20, "h": 0.04, "units": "km"},
  "north_arrow": {"x": 0.86, "y": 0.16, "w": 0.05, "h": 0.08},
  "typography": {"title_size": 22, "legend_size": 9, "label_size": 10},
  "palette": {"colors": ["#B4D2A0", "#A0BEDC", "#EBE1BE"]},
  "regions": [
    {"name": "标题区", "role": "title", "x": 0.25, "y": 0.02, "w": 0.50, "h": 0.08},
    {"name": "主图区", "role": "map", "x": 0.076, "y": 0.139, "w": 0.849, "h": 0.723},
    {"name": "图例区", "role": "legend", "x": 0.76, "y": 0.62, "w": 0.14, "h": 0.17}
  ],
  "pitfalls": [
    {"issue": "图例压住主图", "check": "legend box inside map_frame, not covering key content", "severity": "warn"},
    {"issue": "标题字号过小", "check": "title_size >= 16", "severity": "info"}
  ],
  "notes": "本模板来自 2024 年某期刊图 3，按样图版式整理。"
}
```

字段约束：

- 所有 x/y/w/h 都是画布归一化坐标，必须留在 0–1；
- `map_frame` 用 `x/y/width/height`；图例/比例尺/指北针/regions 用 `x/y/w/h`；
- `title_size ≥ legend_size` 且 `title_size ≥ label_size`；
- 颜色用 `#RRGGBB`；
- `pitfalls[]` 把“这个版式容易翻车的点”写成可执行检查，不要只写散文。

提交（合并前会跑校验；失败不写库）：

```text
arcgis-distill patch <id> --file spec.patch.json --json
```

禁止在 patch 里出现 `id / image_sha256 / measured / source_image / schema_version`。

## 3. 校验与预览

```text
arcgis-distill validate <id> --json
arcgis-distill preview <id> --out preview.png --json
```

- `validate` 全绿会置为 `validated`；有错按 `path` 修正 patch 后重交。
- 必须把 `preview.png` 给用户看，并与样本图并排核对：图幅范围、图例位置、标题、
  比例尺、指北针、分区是否符合。

## 4. 人工评审与批准

```text
arcgis-distill review <id> --by "用户姓名" --notes "版式核对通过" --json
arcgis-distill approve <id> --json
```

`approve` 需要 review；存在校验错误或缺 review 时会拒绝。只有用户点头后才执行；
`--force` 仅用于用户明确要求的例外，并会写入 review 记录。

## 5. 用于出图

```text
arcgis-distill apply <id> --out context/template.json --json
```

助手读取 context 中的 `map_frame / palette / typography / pitfalls`，按
`arcgis-pro-mapping` 技能在 ArcGIS Pro 中制图；完成后按该技能“自检 → 保存 →
人工验收”收尾。

## 6. 分享与复用

```text
arcgis-distill export <id> --out <name>.zip
arcgis-distill bundle-import <name>.zip
```

公开分享前确认样本图版权；期刊/教材图不要导出公开 bundle。

## 7. 删除模板（清理）

```text
arcgis-distill delete <id> --json            # imported/measured/analyzed/validated
arcgis-distill delete <id> --force --json    # ready（已批准）模板必须先征得用户同意
```

- 删除内容：spec、index 条目、作业单，以及**不再被其他模板引用**的去重样本图；
  图片仍被引用时保留，返回的 `removed.image_kept_for` 列出引用者 id。
- `ready` 模板需 `--force`，且仅当用户明确要求清理时执行（否则退出码 5 拒绝）。
- 删除不存在的 id 返回 `not_found`（退出码 3）；重复删除安全、不会损坏索引。
- `inbox` 中的待处理图片不受 delete 影响。
