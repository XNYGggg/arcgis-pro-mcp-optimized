"""arcgis-guard CLI (skeleton).

Implemented commands in this skeleton: doctor
Planned: summon, wrap, status, test, self-test
"""

from __future__ import annotations

import argparse

_COMMANDS = ["doctor", "summon", "wrap", "status", "test", "self-test"]


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        prog="arcgis-guard",
        description="On-demand dialog guard for ArcGIS Pro (skeleton; see docs/design-2026-11-16.md §3).",
    )
    parser.add_argument("command", nargs="?", default="doctor", choices=_COMMANDS)
    parser.add_argument("--json", action="store_true", help="machine-readable output")
    args = parser.parse_args(argv)

    if args.command == "doctor":
        msg = "arcgis-guard skeleton 0.1.0: guard engine not implemented yet"
        print('{"ok": true, "module": "guard", "status": "skeleton"}' if args.json else msg)
        return 0

    print(f"arcgis-guard {args.command}: not implemented in the skeleton (docs/design-2026-11-16.md §3)")
    return 3


if __name__ == "__main__":  # pragma: no cover
    raise SystemExit(main())
