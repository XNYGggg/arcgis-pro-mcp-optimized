"""arcgis-distill command line.

One-shot pipeline, no daemon:

    import -> measure -> patch (semantic fields) -> validate -> preview
    -> review -> approve (ready) -> apply

Deterministic steps carry no LLM. The semantic step is described in
``skills/template-distill/SKILL.md`` and submitted through ``patch``, which
merges and validates before writing anything.
"""

from __future__ import annotations

import argparse
import hashlib
import json
import sys
import time
from pathlib import Path
from typing import Any

from ..common.runtime import (
    EXIT_ENV,
    EXIT_OK,
    EXIT_RUNTIME,
    EXIT_USAGE,
    EXIT_VALIDATION,
    MODULE_VERSION,
    agentkit_home,
    emit,
    fail,
    load_config,
    ok,
    write_json_atomic,
)
from .library import TemplateLibrary
from .validate import validate_spec

PROTECTED_PATCH_KEYS = {"id", "image_sha256", "measured", "source_image", "schema_version"}
STATUS_ORDER = ["imported", "draft", "measured", "analyzed", "validated", "ready"]


def _split_json(argv: list[str]) -> tuple[list[str], bool]:
    as_json = "--json" in argv
    return [item for item in argv if item != "--json"], as_json


def _parse_tags(values: list[str] | None) -> list[str]:
    tags: list[str] = []
    for value in values or []:
        tags.extend(part.strip() for part in value.split(",") if part.strip())
    return tags


def _deep_merge(base: dict[str, Any], patch: dict[str, Any]) -> dict[str, Any]:
    result = json.loads(json.dumps(base))
    for key, value in patch.items():
        if isinstance(value, dict) and isinstance(result.get(key), dict):
            result[key] = _deep_merge(result[key], value)
        else:
            result[key] = value
    return result


def _hash_payload(payload: Any) -> str:
    return hashlib.sha256(
        json.dumps(payload, ensure_ascii=False, sort_keys=True).encode("utf-8")
    ).hexdigest()[:16]


def _set_status(spec: dict[str, Any], status: str) -> None:
    # A ready template is never silently downgraded; re-running a step on it
    # only refreshes evidence and the caller must explicitly re-approve.
    if spec.get("status") == "ready" and status != "ready":
        return
    spec["status"] = status


def _library(args=None) -> TemplateLibrary:
    config = load_config("distill")
    root = getattr(args, "library", None) if args else None
    return TemplateLibrary(root=root, config=config)


def _load_spec(library: TemplateLibrary, template_id: str) -> dict[str, Any]:
    return library.get(template_id)


def _human_issues(payload: dict[str, Any]) -> str:
    lines = []
    for issue in payload.get("errors", []):
        lines.append(f"  ERROR {issue['path']}: {issue['message']}")
    for issue in payload.get("warnings", []):
        lines.append(f"  WARN  {issue['path']}: {issue['message']}")
    return "\n".join(lines)


# --- commands ---------------------------------------------------------------

def _doctor(args, as_json: bool) -> int:
    checks: list[dict[str, Any]] = []

    def check(name: str, passed: bool, detail: str, critical: bool = False) -> None:
        checks.append({"check": name, "ok": passed, "detail": detail,
                       "critical": critical})

    library = _library(args)
    check("library", library.root.is_dir(), str(library.root), True)
    check("schema", library.schema_path.is_file(), str(library.schema_path), True)
    check("index", library.index_path.is_file(), str(library.index_path), True)
    try:
        import PIL  # noqa: F401
        from PIL import Image
        check("pillow", True, f"Pillow {PIL.__version__}", True)
        check("webp", "WEBP" in Image.registered_extensions().values()
              or True, "Pillow image plugins loaded")
    except ImportError as exc:
        check("pillow", False, str(exc), True)
    try:
        probe = library.root / ".write-probe"
        probe.write_text("ok", encoding="utf-8")
        probe.unlink(missing_ok=True)
        check("writable", True, str(library.root), True)
    except OSError as exc:
        check("writable", False, str(exc), True)
    check("templates", True, str(len(library.list())) + " in index")
    healthy = all(item["ok"] for item in checks if item["critical"])
    payload = ok(module="distill", version=MODULE_VERSION,
                 status="ready" if healthy else "not-ready", checks=checks)
    if as_json:
        emit(payload, True)
    else:
        print("arcgis-distill doctor")
        for item in checks:
            mark = "OK  " if item["ok"] else "FAIL"
            print(f"  [{mark}] {item['check']}: {item['detail']}")
    return EXIT_OK if healthy else EXIT_ENV


def _import(args, as_json: bool) -> int:
    library = _library(args)
    result = library.import_image(
        args.image, args.name, tags=_parse_tags(args.tag),
        width_in=args.width_in, template_id=args.id)
    spec = result["spec"]
    payload = ok(template_id=spec["id"], spec=spec,
                 skipped=result["skipped"],
                 reason=result.get("reason"),
                 library=str(library.root),
                 next=f"arcgis-distill measure {spec['id']}")
    if as_json:
        emit(payload, True)
    else:
        print(f"{'skipped (dedupe)' if result['skipped'] else 'imported'}: {spec['id']}")
        print(f"library: {library.root}")
        print(f"next: arcgis-distill measure {spec['id']}")
    return EXIT_OK


def _measure(args, as_json: bool) -> int:
    from .measure import measure_image

    library = _library(args)
    spec = _load_spec(library, args.template_id)
    image = library.image_path(spec)
    width_in = args.width_in or (spec.get("canvas") or {}).get("width_in") or 6.7
    measured = measure_image(image, assumed_width_in=float(width_in))
    spec["measured"] = measured
    canvas = spec.setdefault("canvas", {})
    if args.force or not canvas.get("height_in"):
        canvas["height_in"] = measured["canvas_height_in"]
    canvas.setdefault("width_in", measured["assumed_width_in"])
    canvas["dpi"] = measured["dpi"]
    _set_status(spec, "measured")
    library.save(spec)
    library.mark_step(spec["id"], "measure", _hash_payload(measured),
                      {"width_in": width_in})
    payload = ok(template_id=spec["id"], status=spec["status"],
                 measured=measured, spec=spec,
                 next=f"arcgis-distill patch {spec['id']} --file spec.patch.json")
    if as_json:
        emit(payload, True)
    else:
        print(f"measured {spec['id']}: aspect={measured['aspect']} "
              f"canvas={measured['assumed_width_in']}x{measured['canvas_height_in']} in")
        print(f"h_lines={measured['h_lines']}")
        print(f"v_lines={measured['v_lines']}")
        print(f"palette={[entry['hex'] for entry in measured['palette_top'][:6]]}")
    return EXIT_OK


def _patch(args, as_json: bool) -> int:
    library = _library(args)
    spec = _load_spec(library, args.template_id)
    if args.file:
        raw = Path(args.file).read_text(encoding="utf-8-sig")
    elif args.patch_json:
        raw = args.patch_json
    else:
        return fail("patch needs --file or --patch-json", "usage",
                    exit_code=EXIT_USAGE, as_json=as_json)
    try:
        patch = json.loads(raw)
    except ValueError as exc:
        return fail(f"patch is not valid JSON: {exc}", "usage",
                    exit_code=EXIT_USAGE, as_json=as_json)
    if not isinstance(patch, dict):
        return fail("patch must be a JSON object", "usage",
                    exit_code=EXIT_USAGE, as_json=as_json)
    blocked = sorted(PROTECTED_PATCH_KEYS.intersection(patch))
    if blocked:
        return fail(f"patch may not change {', '.join(blocked)}; "
                    "re-run measure instead of hand-editing measured values",
                    "usage", exit_code=EXIT_USAGE, as_json=as_json)
    merged = _deep_merge(spec, patch)
    _set_status(merged, "analyzed")
    report = validate_spec(merged, library)
    payload = ok(template_id=merged["id"], status=merged["status"],
                 validation=report, spec=merged, saved=False)
    if not report["ok"]:
        if as_json:
            emit(payload, True)
        else:
            print("patch rejected by validation; nothing was written")
            print(_human_issues(report))
        return EXIT_VALIDATION
    library.save(merged)
    payload["saved"] = True
    payload["next"] = f"arcgis-distill validate {merged['id']}"
    if as_json:
        emit(payload, True)
    else:
        print(f"patched {merged['id']} -> {merged['status']} (validated merge)")
        if report["warnings"]:
            print(_human_issues(report))
    return EXIT_OK


def _validate(args, as_json: bool) -> int:
    library = _library(args)
    spec = _load_spec(library, args.template_id)
    report = validate_spec(spec, library)
    if report["ok"]:
        _set_status(spec, "validated")
        library.save(spec)
        library.mark_step(spec["id"], "validate", _hash_payload(report))
    payload = ok(template_id=spec["id"], status=spec["status"],
                 validation=report, spec=spec)
    if as_json:
        emit(payload, True)
    else:
        print(f"validate {spec['id']}: {'PASS' if report['ok'] else 'FAIL'} "
              f"({len(report['errors'])} errors, {len(report['warnings'])} warnings)")
        if report["errors"] or report["warnings"]:
            print(_human_issues(report))
    return EXIT_OK if report["ok"] else EXIT_VALIDATION


def _preview(args, as_json: bool) -> int:
    from .preview import render_preview

    library = _library(args)
    spec = _load_spec(library, args.template_id)
    image = library.image_path(spec)
    out = args.out or str(agentkit_home() / "jobs" / f"{spec['id']}-preview.png")
    meta = render_preview(spec, image if image.is_file() else None, out,
                          panel_width=args.width)
    payload = ok(template_id=spec["id"], preview=meta,
                 next=f"arcgis-distill review {spec['id']} --by <name>")
    if as_json:
        emit(payload, True)
    else:
        print(f"preview: {meta['out']} ({meta['width']}x{meta['height']})")
    return EXIT_OK


def _review(args, as_json: bool) -> int:
    import time

    library = _library(args)
    spec = _load_spec(library, args.template_id)
    review = spec.get("review") if isinstance(spec.get("review"), dict) else {}
    decisions = list(review.get("decisions") or [])
    if args.notes:
        decisions.append(args.notes)
    spec["review"] = {
        "by": args.by,
        "date": time.strftime("%Y-%m-%d"),
        "decisions": decisions,
    }
    library.save(spec)
    payload = ok(template_id=spec["id"], status=spec["status"], review=spec["review"],
                 next=f"arcgis-distill approve {spec['id']}")
    emit(payload, as_json,
         human=f"review recorded for {spec['id']} by {args.by}")
    return EXIT_OK


def _approve(args, as_json: bool) -> int:
    library = _library(args)
    spec = _load_spec(library, args.template_id)
    report = validate_spec(spec, library)
    if not report["ok"] and not args.force:
        payload = ok(template_id=spec["id"], status=spec["status"],
                     validation=report, approved=False)
        if as_json:
            emit(payload, True)
        else:
            print("approve refused: validation errors (use --force to override)")
            print(_human_issues(report))
        return EXIT_VALIDATION
    if not spec.get("review") and not args.force:
        return fail("approve needs a review record first "
                    "(arcgis-distill review ... --by <name>)", "review_missing",
                    exit_code=EXIT_VALIDATION, as_json=as_json)
    if args.force:
        review = spec.get("review") if isinstance(spec.get("review"), dict) else {}
        review["force"] = True
        # A forced approve must not leave a ready template whose review record
        # would fail validation: record who forced it and when.
        review.setdefault("by", "force")
        review.setdefault("date", time.strftime("%Y-%m-%d"))
        spec["review"] = review
    spec["status"] = "ready"
    library.save(spec)
    library.mark_step(spec["id"], "approve", _hash_payload(report))
    payload = ok(template_id=spec["id"], status="ready",
                 validation=report, approved=True)
    emit(payload, as_json, human=f"approved: {spec['id']} is ready")
    return EXIT_OK


def _list(args, as_json: bool) -> int:
    library = _library(args)
    entries = library.list(status=args.status, tag=args.tag)
    payload = ok(library=str(library.root), count=len(entries), templates=entries)
    if as_json:
        emit(payload, True)
    else:
        print(f"library: {library.root} ({len(entries)} templates)")
        for entry in entries:
            print(f"  {entry['id']:<28} {entry['status']:<10} {entry.get('name','')}")
    return EXIT_OK


def _get(args, as_json: bool) -> int:
    library = _library(args)
    spec = _load_spec(library, args.template_id)
    emit(ok(template_id=spec["id"], spec=spec), as_json,
         human=json.dumps(spec, ensure_ascii=False, indent=2))
    return EXIT_OK


def _export(args, as_json: bool) -> int:
    library = _library(args)
    out = args.out or str(agentkit_home() / "jobs" / f"{args.template_id}.zip")
    path = library.export_bundle(args.template_id, out)
    emit(ok(template_id=args.template_id, bundle=str(path)),
         as_json, human=f"exported bundle: {path}")
    return EXIT_OK


def _bundle_import(args, as_json: bool) -> int:
    library = _library(args)
    result = library.import_bundle(args.bundle)
    emit(ok(**result, library=str(library.root)), as_json,
         human=f"imported bundle -> {result['spec']['id']}")
    return EXIT_OK


def _apply(args, as_json: bool) -> int:
    import time

    library = _library(args)
    spec = _load_spec(library, args.template_id)
    if spec.get("status") != "ready":
        return fail(f"template '{spec['id']}' is {spec.get('status')}, not ready",
                    "not_ready", exit_code=EXIT_VALIDATION, as_json=as_json)
    out = args.out or str(agentkit_home() / "context" / f"{spec['id']}.json")
    context = {
        "schema_version": 1,
        "revision": "1.1",
        "template_id": spec["id"],
        "applied_at": time.strftime("%Y-%m-%dT%H:%M:%S"),
        "spec": spec,
    }
    write_json_atomic(out, context)
    emit(ok(template_id=spec["id"], context=str(out),
            layout=spec.get("map_frame"), palette=spec.get("palette")),
         as_json, human=f"template context written: {out}")
    return EXIT_OK


def _inbox(args, as_json: bool) -> int:
    from .measure import measure_image

    library = _library(args)
    images = library.inbox_images()
    if not args.process_new:
        emit(ok(inbox=str(library.inbox), count=len(images),
                images=[str(path) for path in images]),
             as_json,
             human="\n".join(str(path) for path in images) or "(inbox is empty)")
        return EXIT_OK
    results = []
    for image in images:
        imported = library.import_image(image, image.stem, width_in=args.width_in)
        spec = imported["spec"]
        if not imported["skipped"]:
            measured = measure_image(library.image_path(spec), args.width_in)
            spec["measured"] = measured
            spec.setdefault("canvas", {})["height_in"] = measured["canvas_height_in"]
            spec.setdefault("canvas", {})["dpi"] = measured["dpi"]
            _set_status(spec, "measured")
            library.save(spec)
            library.mark_step(spec["id"], "measure", _hash_payload(measured))
        results.append({"image": str(image), "template_id": spec["id"],
                        "skipped": imported["skipped"]})
    emit(ok(processed=len(results), results=results), as_json,
         human="\n".join(f"{item['template_id']}: {item['image']}" for item in results)
               or "(inbox is empty)")
    return EXIT_OK


# --- parser -----------------------------------------------------------------

def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="arcgis-distill",
        description="On-demand template distillation for ArcGIS Pro layouts.")
    parser.add_argument("--library", default=None,
                        help="template library root (default: repository library/)")
    sub = parser.add_subparsers(dest="command")

    doctor = sub.add_parser("doctor")
    doctor.set_defaults(func=_doctor)

    imp = sub.add_parser("import", help="import a sample figure")
    imp.add_argument("image")
    imp.add_argument("--name", required=True)
    imp.add_argument("--tag", action="append")
    imp.add_argument("--width-in", type=float, default=6.7)
    imp.add_argument("--id", default=None)
    imp.set_defaults(func=_import)

    measure = sub.add_parser("measure", help="measure a template's sample image")
    measure.add_argument("template_id")
    measure.add_argument("--width-in", type=float, default=None)
    measure.add_argument("--force", action="store_true")
    measure.set_defaults(func=_measure)

    patch = sub.add_parser("patch", help="merge semantic fields and validate")
    patch.add_argument("template_id")
    patch.add_argument("--file", default=None)
    patch.add_argument("--patch-json", default=None)
    patch.set_defaults(func=_patch)

    validate = sub.add_parser("validate", help="schema + geometry checks")
    validate.add_argument("template_id")
    validate.set_defaults(func=_validate)

    preview = sub.add_parser("preview", help="render a wireframe preview")
    preview.add_argument("template_id")
    preview.add_argument("--out", default=None)
    preview.add_argument("--width", type=int, default=900)
    preview.set_defaults(func=_preview)

    review = sub.add_parser("review", help="record a human review")
    review.add_argument("template_id")
    review.add_argument("--by", required=True)
    review.add_argument("--notes", default="")
    review.set_defaults(func=_review)

    approve = sub.add_parser("approve", help="mark a validated template ready")
    approve.add_argument("template_id")
    approve.add_argument("--force", action="store_true")
    approve.set_defaults(func=_approve)

    listing = sub.add_parser("list", help="list templates")
    listing.add_argument("--status", default=None)
    listing.add_argument("--tag", default=None)
    listing.set_defaults(func=_list)

    get = sub.add_parser("get", help="print one template spec")
    get.add_argument("template_id")
    get.set_defaults(func=_get)

    export = sub.add_parser("export", help="export a template bundle (zip)")
    export.add_argument("template_id")
    export.add_argument("--out", default=None)
    export.set_defaults(func=_export)

    bundle = sub.add_parser("bundle-import", help="import a template bundle")
    bundle.add_argument("bundle")
    bundle.set_defaults(func=_bundle_import)

    apply = sub.add_parser("apply", help="write a template context for mapping")
    apply.add_argument("template_id")
    apply.add_argument("--out", default=None)
    apply.set_defaults(func=_apply)

    inbox = sub.add_parser("inbox", help="list or process the inbox once")
    inbox.add_argument("--process-new", action="store_true")
    inbox.add_argument("--width-in", type=float, default=6.7)
    inbox.set_defaults(func=_inbox)

    return parser


def main(argv: list[str] | None = None) -> int:
    argv, as_json = _split_json(list(sys.argv[1:] if argv is None else argv))
    parser = _build_parser()
    try:
        args = parser.parse_args(argv)
    except SystemExit as exc:
        return int(exc.code or 0)
    if not getattr(args, "command", None):
        args.command = "doctor"
        args.func = _doctor
    try:
        return int(args.func(args, as_json))
    except ImportError as exc:
        return fail(f"missing dependency: {exc} (install Pillow)", "env_missing",
                    exit_code=EXIT_ENV, as_json=as_json)
    except FileNotFoundError as exc:
        return fail(str(exc), "not_found", exit_code=EXIT_ENV, as_json=as_json)
    except TimeoutError as exc:
        return fail(str(exc), "timeout", exit_code=EXIT_RUNTIME, as_json=as_json)
    except ValueError as exc:
        return fail(str(exc), "invalid_input", exit_code=EXIT_RUNTIME, as_json=as_json)


if __name__ == "__main__":  # pragma: no cover
    raise SystemExit(main())
