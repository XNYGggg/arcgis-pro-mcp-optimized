"""On-demand template distillation (one-shot, deterministic steps only).

Pipeline: import -> measure -> patch (semantic fields) -> validate -> preview
-> review -> ready -> apply. Every step is idempotent per input sha256; the
semantic step is guided by ``skills/template-distill/SKILL.md``.
"""

