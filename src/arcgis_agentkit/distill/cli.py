"""arcgis-distill CLI (skeleton).

Implemented commands in this skeleton: doctor
Planned: import, measure, patch, validate, preview, review, approve, list, get,
         export, bundle-import, apply, inbox
"""

from __future__ import annotations

import argparse

_COMMANDS = [
    "doctor", "import", "measure", "patch", "validate", "preview",
    "review", "approve", "list", "get", "export", "bundle-import", "apply", "inbox",
]


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        prog="arcgis-distill",
        description="On-demand template distillation for ArcGIS Pro layouts (skeleton; see docs/design-2026-11-16.md §4).",
    )
    parser.add_argument("command", nargs="?", default="doctor", choices=_COMMANDS)
    parser.add_argument("--json", action="store_true", help="machine-readable output")
    args = parser.parse_args(argv)

    if args.command == "doctor":
        msg = "arcgis-distill skeleton 0.1.0: distillation pipeline not implemented yet"
        print('{"ok": true, "module": "distill", "status": "skeleton"}' if args.json else msg)
        return 0

    print(f"arcgis-distill {args.command}: not implemented in the skeleton (docs/design-2026-11-16.md §4)")
    return 3


if __name__ == "__main__":  # pragma: no cover
    raise SystemExit(main())
