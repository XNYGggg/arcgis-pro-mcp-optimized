# Contributing

- Python 3.10+；开发安装：`python -m pip install -e .`
- 测试：`python -m pytest -q`
- 代码风格：标准库优先；新增依赖需要说明理由。
- 设计约束：guard 不得常驻、只允许点击第三方代码信任框；distill 的确定性步骤不得引入 LLM。
- 提交前请确认 `docs/design-2026-11-16.md` 中的验收标准仍然适用。

许可：提交即表示同意以 AGPL-3.0-or-later 发布。
