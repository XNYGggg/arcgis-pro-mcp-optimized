---
name: template-distill
description: Distill a sample map figure into a reusable layout template spec (planned; skeleton only).
---

# Template Distill（规划中）

本技能将指导助手把一张样本成图蒸馏为可复用的版式模板（`library/specs/<id>.json`）。

完整流程设计见 `docs/design-2026-11-16.md` §4：

1. `arcgis-distill import <image>` —— 哈希去重、入库、生成 draft 骨架；
2. `arcgis-distill measure <id>` —— 确定性几何测量（图幅、长宽比、强线、主色）；
3. 助手按本技能说明补齐版式分区、图例样式、色板语义、字体层级与易翻车点；
4. `arcgis-distill validate` → `preview` → 用户 review → `approve` 置 ready；
5. `arcgis-distill apply --plan` 并入制图任务上下文。

约束：不手改 `measured` 数值；spec 必须通过 schema 与几何一致性校验；不运行任何常驻进程。
