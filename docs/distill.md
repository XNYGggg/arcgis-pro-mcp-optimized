# Distill · 按需模板蒸馏（客户版 1.1.x；1.1.12 移植自 2.0.2，1.1.13 起含 delete）

把一张样本成图蒸馏成可复用的版式 spec：`import → measure → patch → validate →
preview → review → approve → apply`。一次作业、无守护进程、以输入 sha256 幂等；
measure/validate/preview 全部确定性、无 LLM。语义判读由调用方助手按
`skills/template-distill/SKILL.md` 完成，工具本身零 API key。

## 1. 状态机

```
imported ──measure──► measured ──patch(语义字段)──► analyzed
    ▲                    │                            │
    │(--force 重来)      │                            ▼
    └────────────────────┴──────── validate ◄─────────┘
                                        │ review + approve
                                        ▼
                                      ready ──apply──► 制图任务上下文
```

- 作业单：`%LOCALAPPDATA%\ArcGIS-AgentKit\jobs\<tpl-id>.json`，记录每一步的
  `tool_version / at / output_hash`；中断后重跑可续做。
- `measured` 只能由 `measure` 写；`patch` 拒绝修改 `measured / id / image_sha256 /
  source_image`。`ready` 模板不会被静默降级或改写。

## 2. 命令

```text
arcgis-distill doctor
arcgis-distill import <image> --name NAME [--tag T] [--width-in 6.7] [--id ID]
arcgis-distill measure <id> [--width-in IN] [--force]
arcgis-distill patch <id> --file spec.patch.json      # 或 --patch-json '{"...":...}'
arcgis-distill validate <id>
arcgis-distill preview <id> [--out preview.png] [--width 900]
arcgis-distill review <id> --by NAME [--notes TEXT]
arcgis-distill approve <id> [--force]
arcgis-distill list [--status S] [--tag T]
arcgis-distill get <id>
arcgis-distill export <id> [--out bundle.zip]
arcgis-distill bundle-import <bundle.zip>
arcgis-distill apply <id> [--out context.json]
arcgis-distill inbox [--process-new] [--width-in 6.7]
arcgis-distill delete <id> [--force]
```

全部支持 `--json`；退出码 0 成功 / 2 参数 / 3 环境 / 4 运行失败 / 5 校验失败。
`approve` 在存在校验错误或缺 review 时拒绝（`--force` 可覆盖并写入 review）。
`delete` 移除模板的 spec、索引条目、作业单与不再被其他模板引用的去重图片；
删除不存在的 id 报 `not_found`（退出码 3，重复删除同样安全），`ready`（已批准）
模板需 `--force`（否则退出码 5）；`inbox` 不受影响。

## 3. 库与 schema v1.1

```
library/
├─ schema.json          # v1.1，只增不改
├─ index.json           # 原子更新（锁文件）
├─ images/              # 按 sha256 去重：<sha16>.<ext>
├─ specs/<id>.json
└─ inbox/               # 普通文件夹，不监听
```

库根默认是仓库 `library/`；客户版是 `%LOCALAPPDATA%\ArcGIS Pro MCP\library`；
可用 `ARCGIS_DISTILL_LIBRARY` 或 `--library` 覆盖。新增字段：

- `regions[]`：`{name, role, x, y, w, h, notes}`（归一化 0–1）；
- `measured.palette_top[]`：主色 + 占比；
- `pitfalls[]`：`{issue, check, severity}`；
- `review{by, date, decisions[], force?}`。

**版权**：公开仓库只放自制示例模板；期刊/教材样本图及其派生 spec 默认不公开。

## 4. measure 输出

`figure_bbox / aspect / canvas_height_in / h_lines / v_lines / palette_top / dpi`。
强线检测会把被细边框切开的亮带合并（gap ≤ 1% 高度），因此图框顶/底边会作为
`h_lines` 出现，而不是把图框内部误当作整幅图。

## 5. 与制图流程的衔接

1. `template_list` 选模板；
2. `template_apply` 写 `%LOCALAPPDATA%\ArcGIS-AgentKit\context\<id>.json`；
3. 助手按 spec 的 map_frame / palette / typography / pitfalls 在 Pro 中制图；
4. 沿用 `arcgis-pro-mapping` 技能的自检 → 保存 → 人工验收流程。

## 6. MCP 工具

`template_list / template_get / template_import / template_measure / template_patch /
template_validate / template_review / template_approve / template_preview /
template_apply / template_delete`。它们都是 add-in 对 `arcgis-distill` CLI 的薄封装；客户版用
Pro 自带 Python 运行 `runtime/distill/run-distill.ps1`，无需另装 Python 或 pip。
`template_preview` 直接返回 PNG 图像内容。
