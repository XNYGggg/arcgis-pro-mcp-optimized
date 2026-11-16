"""On-demand dialog guard (skeleton).

Design (docs/design-2026-11-16.md §3):
- No resident process. Summoned per operation that may raise the ArcGIS Pro
  "third-party code library" trust dialog.
- Handshake: sessions/<token>/{ready.json, settle, result.json}.
- Event-first UIA detection with a defensive poll only while armed.
- Safety boundary: only the third-party-code trust dialog; never overwrite or
  delete confirmations. Alert window >= 30 s when auto-click fails.
"""

from .cli import main  # noqa: F401
