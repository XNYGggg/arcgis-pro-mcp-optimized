"""Schema + geometric consistency validation for template specs.

A template only becomes ``ready`` when every check below passes. Geometry
checks are deliberately conservative: a region or legend that pokes outside
the canvas is a broken layout, not a taste question.
"""

from __future__ import annotations

import json
import re
from typing import Any

from . import schema as schema_validator
from .library import TemplateLibrary

ALLOWED_STATUS = ["imported", "draft", "measured", "analyzed", "validated", "ready"]
HEX = re.compile(r"^#[0-9A-Fa-f]{6}$")
BOX_KEYS = ("x", "y", "w", "h")


def _issue(severity: str, code: str, message: str, path: str = "$") -> dict[str, str]:
    return {"severity": severity, "code": code, "message": message, "path": path}


def _number(value: Any) -> bool:
    return isinstance(value, (int, float)) and not isinstance(value, bool)


def _box(container: Any, path: str, issues: list[dict[str, str]],
         required: bool = True, name_key: str = "w") -> None:
    if not isinstance(container, dict) or not container:
        if required:
            issues.append(_issue("error", "box_missing", "missing box geometry", path))
        return
    keys = ("x", "y", "w", "h") if name_key == "w" else ("x", "y", "width", "height")
    for key in keys:
        if key not in container:
            issues.append(_issue("error", "box_incomplete", f"missing '{key}'", f"{path}.{key}"))
        elif not _number(container[key]):
            issues.append(_issue("error", "box_type", f"'{key}' must be a number", f"{path}.{key}"))
    if all(key in container and _number(container[key]) for key in keys):
        x, y, w, h = (float(container[key]) for key in keys)
        if w <= 0 or h <= 0:
            issues.append(_issue("error", "box_size", "width/height must be positive", path))
        if x < -0.001 or y < -0.001 or x + w > 1.001 or y + h > 1.001:
            issues.append(_issue("error", "box_outside",
                                 "box extends outside the canvas (normalised 0-1)", path))


def _check_hex_colors(value: Any, path: str, issues: list[dict[str, str]]) -> None:
    if isinstance(value, str):
        if not HEX.match(value):
            issues.append(_issue("error", "color_invalid", f"invalid colour {value!r}", path))
        return
    if isinstance(value, dict):
        for key, child in value.items():
            _check_hex_colors(child, f"{path}.{key}", issues)
        return
    if isinstance(value, list):
        for index, child in enumerate(value):
            _check_hex_colors(child, f"{path}[{index}]", issues)


def validate_spec(spec: dict[str, Any], library: TemplateLibrary) -> dict[str, Any]:
    errors: list[dict[str, str]] = []
    warnings: list[dict[str, str]] = []

    try:
        raw_schema = library.schema_path.read_text(encoding="utf-8-sig")
        schema_doc = json.loads(raw_schema)
    except (OSError, ValueError) as exc:
        errors.append(_issue("error", "schema_unreadable", str(exc), "$schema"))
        schema_doc = None

    if schema_doc:
        for item in schema_validator.validate(spec, schema_doc):
            errors.append(_issue("error", "schema_" + item["code"],
                                 item["message"], item["path"]))

    if spec.get("status") not in ALLOWED_STATUS:
        errors.append(_issue("error", "status_invalid",
                             f"status {spec.get('status')!r} is not one of {ALLOWED_STATUS}",
                             "$.status"))

    canvas = spec.get("canvas") or {}
    if not isinstance(canvas, dict):
        errors.append(_issue("error", "canvas_type", "canvas must be an object", "$.canvas"))
        canvas = {}
    width_in = canvas.get("width_in")
    height_in = canvas.get("height_in")
    if width_in is not None and (not _number(width_in) or width_in <= 0):
        errors.append(_issue("error", "canvas_width", "width_in must be positive", "$.canvas.width_in"))
    if height_in is None:
        warnings.append(_issue("warn", "canvas_height_missing",
                               "canvas.height_in is missing; run measure first",
                               "$.canvas.height_in"))
    elif not _number(height_in) or height_in <= 0:
        errors.append(_issue("error", "canvas_height", "height_in must be positive", "$.canvas.height_in"))

    map_frame = spec.get("map_frame")
    if isinstance(map_frame, dict) and map_frame:
        _box(map_frame, "$.map_frame", errors, required=True, name_key="width")
    elif spec.get("status") == "ready":
        errors.append(_issue("error", "map_frame_missing",
                             "a ready template needs map_frame geometry", "$.map_frame"))

    for section in ("legend", "scale_bar", "north_arrow"):
        value = spec.get(section)
        if isinstance(value, dict) and value and ("x" in value or "y" in value):
            _box(value, f"$.{section}", errors, required=True)

    regions = spec.get("regions")
    if regions is not None:
        if not isinstance(regions, list):
            errors.append(_issue("error", "regions_type", "regions must be a list", "$.regions"))
        else:
            for index, region in enumerate(regions):
                path = f"$.regions[{index}]"
                if not isinstance(region, dict):
                    errors.append(_issue("error", "region_type", "region must be an object", path))
                    continue
                for key in ("name", "role"):
                    if not region.get(key):
                        errors.append(_issue("error", "region_field", f"missing '{key}'", f"{path}.{key}"))
                _box(region, path, errors, required=True)

    typography = spec.get("typography") or {}
    if isinstance(typography, dict):
        title = typography.get("title_size")
        legend = typography.get("legend_size")
        label = typography.get("label_size")
        if _number(title) and _number(legend) and title < legend:
            errors.append(_issue("error", "type_order",
                                 "title_size should not be smaller than legend_size",
                                 "$.typography"))
        if _number(title) and _number(label) and title < label:
            errors.append(_issue("error", "type_order",
                                 "title_size should not be smaller than label_size",
                                 "$.typography"))

    _check_hex_colors(spec.get("palette") or {}, "$.palette", errors)

    measured = spec.get("measured") or {}
    if isinstance(measured, dict) and measured:
        measured_height = measured.get("canvas_height_in")
        if _number(measured_height) and _number(height_in) and height_in:
            ratio = abs(float(measured_height) - float(height_in)) / float(height_in)
            if ratio > 0.03:
                errors.append(_issue(
                    "error", "measured_canvas_mismatch",
                    f"canvas.height_in differs from measured by {ratio:.1%} (limit 3%); "
                    "re-measure or fix the canvas instead of hand-editing measured",
                    "$.canvas.height_in"))
        if measured.get("tool_version") is None:
            warnings.append(_issue("warn", "measured_provenance",
                                   "measured block has no tool_version", "$.measured"))
        if _number(measured_height) and height_in is None:
            warnings.append(_issue("warn", "canvas_height_suggested",
                                   f"set canvas.height_in to {measured_height}",
                                   "$.canvas.height_in"))
    elif spec.get("status") in ("measured", "analyzed", "validated", "ready"):
        warnings.append(_issue("warn", "measured_missing",
                               "no measured block; run measure before approving",
                               "$.measured"))

    pitfalls = spec.get("pitfalls") or []
    if isinstance(pitfalls, list):
        for index, pitfall in enumerate(pitfalls):
            path = f"$.pitfalls[{index}]"
            if not isinstance(pitfall, dict):
                errors.append(_issue("error", "pitfall_type", "pitfall must be an object", path))
                continue
            for key in ("issue", "check"):
                if not pitfall.get(key):
                    errors.append(_issue("error", "pitfall_field", f"missing '{key}'", f"{path}.{key}"))
            severity = pitfall.get("severity", "warn")
            if severity not in ("info", "warn", "error"):
                errors.append(_issue("error", "pitfall_severity",
                                     f"invalid severity {severity!r}", f"{path}.severity"))
    elif pitfalls:
        errors.append(_issue("error", "pitfalls_type", "pitfalls must be a list", "$.pitfalls"))

    review = spec.get("review")
    if review is not None:
        if not isinstance(review, dict):
            errors.append(_issue("error", "review_type", "review must be an object", "$.review"))
        else:
            for key in ("by", "date"):
                if not review.get(key):
                    errors.append(_issue("error", "review_field", f"missing '{key}'", f"$.review.{key}"))
    if spec.get("status") == "ready" and not review:
        errors.append(_issue("error", "review_missing",
                             "a ready template needs a review record (review + approve)",
                             "$.review"))

    return {"ok": not errors, "errors": errors, "warnings": warnings}
