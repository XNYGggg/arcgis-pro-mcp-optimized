"""On-demand template distillation (skeleton).

Design (docs/design-2026-11-16.md §4):
- One-shot pipeline: import -> measure -> semantic patch -> validate -> preview
  -> review -> ready -> apply. Idempotent per input sha256; no daemon.
- Deterministic steps (measure/validate/preview/library) carry no LLM.
- Semantic step is guided by skills/template-distill/SKILL.md.
"""

from .cli import main  # noqa: F401
