"""Wireframe preview: draw the spec next to the sample figure."""

from __future__ import annotations

from pathlib import Path
from typing import Any

from ..common.runtime import ensure_dir

COLORS = {
    "canvas": "#FFFFFF",
    "frame": "#DCE8F5",
    "frame_line": "#2F6DB5",
    "title": "#1F2328",
    "text": "#5B6573",
    "region": "#B45F06",
    "legend": "#E8F0E3",
    "legend_line": "#38761D",
    "scale": "#333333",
    "north": "#7A1F1F",
    "sample_border": "#C9CED6",
}


def _font(size: int):
    from PIL import ImageFont

    for candidate in ("msyh.ttc", "msyhbd.ttc", "arial.ttf", "segoeui.ttf"):
        path = Path(r"C:\Windows\Fonts") / candidate
        if path.is_file():
            try:
                return ImageFont.truetype(str(path), size)
            except OSError:
                continue
    return ImageFont.load_default()


def _box(container: Any) -> tuple[float, float, float, float] | None:
    if not isinstance(container, dict):
        return None
    if {"x", "y", "w", "h"} <= set(container):
        return (float(container["x"]), float(container["y"]),
                float(container["w"]), float(container["h"]))
    if {"x", "y", "width", "height"} <= set(container):
        return (float(container["x"]), float(container["y"]),
                float(container["width"]), float(container["height"]))
    return None


def _scaled(box: tuple[float, float, float, float],
            origin: tuple[int, int], size: tuple[int, int]) -> tuple[int, int, int, int]:
    x, y, w, h = box
    left = origin[0] + int(x * size[0])
    top = origin[1] + int(y * size[1])
    right = origin[0] + int((x + w) * size[0])
    bottom = origin[1] + int((y + h) * size[1])
    return left, top, right, bottom


def _draw_spec(spec: dict[str, Any], panel: Any, origin: tuple[int, int],
               size: tuple[int, int]) -> None:
    from PIL import ImageDraw

    draw = ImageDraw.Draw(panel)
    width, height = size
    frame = _box(spec.get("map_frame"))
    if frame:
        draw.rectangle(_scaled(frame, origin, size),
                       fill=COLORS["frame"], outline=COLORS["frame_line"], width=3)
    else:
        draw.rectangle([origin[0] + int(width * 0.06), origin[1] + int(height * 0.14),
                        origin[0] + int(width * 0.94), origin[1] + int(height * 0.92)],
                       outline=COLORS["frame_line"], width=2)

    title = spec.get("title") or {}
    title_text = title.get("text") if isinstance(title, dict) else None
    if not title_text:
        title_text = spec.get("name", "")
    font_title = _font(max(14, min(34, width // 24)))
    draw.text((origin[0] + width // 2, origin[1] + int(height * 0.05)),
              str(title_text), fill=COLORS["title"], font=font_title, anchor="mm")

    for region in spec.get("regions") or []:
        box = _box(region)
        if not box:
            continue
        draw.rectangle(_scaled(box, origin, size), outline=COLORS["region"], width=2)
        label = str(region.get("role") or region.get("name") or "")
        if label:
            draw.text((origin[0] + int((box[0] + 0.01) * width),
                       origin[1] + int((box[1] + 0.01) * height)),
                      label, fill=COLORS["region"], font=_font(max(12, width // 48)))

    legend = spec.get("legend") or {}
    legend_box = _box(legend)
    if legend_box:
        draw.rectangle(_scaled(legend_box, origin, size),
                       fill=COLORS["legend"], outline=COLORS["legend_line"], width=2)
        draw.text((origin[0] + int((legend_box[0] + 0.02) * width),
                   origin[1] + int((legend_box[1] + 0.02) * height)),
                  "Legend", fill=COLORS["legend_line"], font=_font(max(12, width // 45)))

    scale = spec.get("scale_bar") or {}
    scale_box = _box(scale)
    if scale_box:
        left, top, right, _bottom = _scaled(scale_box, origin, size)
        y = top + max(6, (right - left) // 8)
        draw.line([left, y, right, y], fill=COLORS["scale"], width=3)
        draw.line([left, y - 6, left, y + 6], fill=COLORS["scale"], width=3)
        draw.line([right, y - 6, right, y + 6], fill=COLORS["scale"], width=3)

    north = spec.get("north_arrow") or {}
    north_box = _box(north)
    if north_box:
        left, top, right, bottom = _scaled(north_box, origin, size)
        cx = (left + right) // 2
        draw.polygon([(cx, top), (left, bottom), (cx, bottom - (bottom - top) // 3),
                      (right, bottom)], outline=COLORS["north"], fill=COLORS["north"])
        draw.text((cx, bottom + 2), "N", fill=COLORS["north"],
                  font=_font(max(12, width // 50)), anchor="ma")


def render_preview(spec: dict[str, Any], sample_path: str | Path | None,
                   out_path: str | Path, panel_width: int = 900) -> dict[str, Any]:
    from PIL import Image, ImageDraw

    canvas = spec.get("canvas") or {}
    measured = spec.get("measured") or {}
    aspect = None
    if isinstance(measured, dict) and measured.get("aspect"):
        aspect = float(measured["aspect"])
    if not aspect:
        width_in = canvas.get("width_in")
        height_in = canvas.get("height_in")
        if isinstance(width_in, (int, float)) and isinstance(height_in, (int, float)) and height_in:
            aspect = float(width_in) / float(height_in)

    sample = None
    if sample_path and Path(sample_path).is_file():
        sample = Image.open(sample_path).convert("RGB")
        if not aspect:
            aspect = sample.width / sample.height
    if not aspect or aspect <= 0:
        aspect = 1.414

    panel_height = max(320, int(panel_width / aspect))
    panel_width = max(320, int(panel_height * aspect))
    margin = 28
    header = 42
    gap = 24

    panels = 2 if sample is not None else 1
    total_width = margin * 2 + panel_width * panels + gap * (panels - 1)
    total_height = margin * 2 + panel_height + header
    output = Image.new("RGB", (total_width, total_height), COLORS["canvas"])
    draw = ImageDraw.Draw(output)
    font = _font(18)

    if sample is not None:
        fitted = sample.resize((panel_width, panel_height), Image.LANCZOS)
        left = margin
        output.paste(fitted, (left, margin + header))
        draw.rectangle([left, margin + header, left + panel_width,
                        margin + header + panel_height],
                       outline=COLORS["sample_border"], width=1)
        draw.text((left + panel_width // 2, margin + header // 2), "Sample",
                  fill=COLORS["text"], font=font, anchor="mm")

    spec_origin = (margin if panels == 1 else margin + panel_width + gap,
                   margin + header)
    draw.rectangle([spec_origin[0], spec_origin[1],
                    spec_origin[0] + panel_width, spec_origin[1] + panel_height],
                   outline=COLORS["sample_border"], width=1, fill="#FBFCFE")
    _draw_spec(spec, output, spec_origin, (panel_width, panel_height))
    draw.text((spec_origin[0] + panel_width // 2, margin + header // 2),
              f"Spec · {spec.get('id')}", fill=COLORS["text"], font=font, anchor="mm")

    status = spec.get("status", "?")
    draw.text((margin, total_height - 18), f"status: {status} · "
              f"schema v1.1 · intent: layout wireframe, not a rendered map",
              fill=COLORS["text"], font=_font(14), anchor="lm")

    out_path = Path(out_path)
    ensure_dir(out_path.parent)
    output.save(out_path)
    return {"out": str(out_path), "width": total_width, "height": total_height,
            "panels": panels}
