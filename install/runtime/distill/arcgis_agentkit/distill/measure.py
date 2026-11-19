"""Deterministic geometry measurement for a sample map figure.

Ported from the collaboration-bridge tool ``measure_figure.py`` (Pillow only,
no OpenCV, no NumPy) and extended with palette extraction and DPI inference.
Never edit the ``measured`` block by hand: re-run ``arcgis-distill measure``.
"""

from __future__ import annotations

import time
from pathlib import Path
from typing import Any

from ..common.runtime import MODULE_VERSION

MAX_SIDE = 1000


def _load_gray(path: Path):
    from PIL import Image

    with Image.open(path) as sourced:
        image = sourced.convert("L")
        if max(image.size) > MAX_SIDE:
            scale = MAX_SIDE / max(image.size)
            image = image.resize(
                (max(1, int(image.width * scale)), max(1, int(image.height * scale))),
                Image.LANCZOS)
        return image.copy()


def _find_runs(mask: list[bool]) -> list[tuple[int, int]]:
    runs: list[tuple[int, int]] = []
    start = None
    for index, flag in enumerate(mask):
        if flag and start is None:
            start = index
        if not flag and start is not None:
            runs.append((start, index - 1))
            start = None
    if start is not None:
        runs.append((start, len(mask) - 1))
    return runs


def _group_lines(indices: list[int], gap: int = 3) -> list[float]:
    if not indices:
        return []
    groups: list[float] = []
    start = previous = int(indices[0])
    for value in indices[1:]:
        value = int(value)
        if value - previous <= gap:
            previous = value
        else:
            groups.append((start + previous) / 2.0)
            start = previous = value
    groups.append((start + previous) / 2.0)
    return groups


def _merge_runs(runs: list[tuple[int, int]], gap: int) -> list[tuple[int, int]]:
    merged: list[tuple[int, int]] = []
    for run in runs:
        if merged and run[0] - merged[-1][1] <= gap:
            merged[-1] = (merged[-1][0], run[1])
        else:
            merged.append(run)
    return merged


def _band_from_rows(row_fraction: list[float], height: int) -> tuple[int, int] | None:
    gap = max(3, int(height * 0.01))
    for threshold in (0.45, 0.30, 0.20):
        runs = _merge_runs(_find_runs([value > threshold for value in row_fraction]), gap)
        if runs:
            runs.sort(key=lambda item: item[1] - item[0], reverse=True)
            y0, y1 = runs[0]
            if (y1 - y0) > height * 0.05:
                return y0, y1
    return None


def _column_range(data: list[int], width: int, y0: int, y1: int) -> tuple[int, int] | None:
    counts = [0] * width
    for y in range(y0, y1 + 1):
        row = data[y * width:(y + 1) * width]
        for x, value in enumerate(row):
            if value > 90:
                counts[x] += 1
    total = max(1, y1 - y0 + 1)
    for threshold in (0.45, 0.20):
        columns = [x for x, count in enumerate(counts) if count / total > threshold]
        if columns:
            return min(columns), max(columns)
    return None


def _line_positions(data: list[int], width: int, y0: int, y1: int,
                    x0: int, x1: int) -> tuple[list[float], list[float]]:
    height = y1 - y0 + 1
    span = x1 - x0 + 1
    row_dark = []
    for y in range(y0, y1 + 1):
        row = data[y * width + x0:y * width + x1 + 1]
        row_dark.append(sum(1 for value in row if value < 110) / span)
    column_dark = [0] * span
    for y in range(y0, y1 + 1):
        row = data[y * width + x0:y * width + x1 + 1]
        for offset, value in enumerate(row):
            if value < 110:
                column_dark[offset] += 1
    column_dark = [count / height for count in column_dark]

    def positions(fractions: list[float]) -> list[float]:
        for threshold in (0.55, 0.35):
            indices = [index for index, value in enumerate(fractions) if value > threshold]
            lines = _group_lines(indices)
            if lines:
                denominator = max(1, len(fractions) - 1)
                return [round(value / denominator, 4) for value in lines]
        return []

    return positions(row_dark), positions(column_dark)


def _palette(path: Path, limit: int = 8) -> list[dict[str, Any]]:
    from PIL import Image

    with Image.open(path) as sourced:
        image = sourced.convert("RGB")
        if max(image.size) > 120:
            scale = 120 / max(image.size)
            image = image.resize(
                (max(1, int(image.width * scale)), max(1, int(image.height * scale))),
                Image.LANCZOS)
        quantized = image.quantize(colors=12, method=Image.MEDIANCUT)
        palette = quantized.getpalette() or []
        colors = quantized.getcolors() or []
    total = sum(count for count, _index in colors) or 1
    entries: list[dict[str, Any]] = []
    for count, index in sorted(colors, reverse=True):
        r, g, b = palette[index * 3:index * 3 + 3]
        if r >= 245 and g >= 245 and b >= 245:
            continue
        if r <= 10 and g <= 10 and b <= 10:
            continue
        entries.append({
            "hex": "#{:02X}{:02X}{:02X}".format(r, g, b),
            "share": round(count / total, 4),
        })
        if len(entries) >= limit:
            break
    return entries


def _dpi(path: Path, width_px: int, width_in: float) -> tuple[int, str]:
    from PIL import Image

    with Image.open(path) as image:
        info = image.info or {}
    value = info.get("dpi")
    if isinstance(value, (tuple, list)) and value:
        try:
            dpi = int(round(float(value[0])))
            if 50 <= dpi <= 1200:
                return dpi, "embedded"
        except (TypeError, ValueError):
            pass
    if width_in and width_px:
        inferred = int(round(width_px / width_in))
        if 50 <= inferred <= 1200:
            return inferred, "inferred"
    return 150, "default"


def measure_image(path: str | Path, assumed_width_in: float = 6.7) -> dict[str, Any]:
    path = Path(path)
    if not path.is_file():
        raise FileNotFoundError(path)
    image = _load_gray(path)
    width, height = image.size
    getter = getattr(image, "get_flattened_data", None)
    data = list(getter() if getter else image.getdata())

    row_fraction = [
        sum(1 for value in data[y * width:(y + 1) * width] if value > 90) / width
        for y in range(height)
    ]
    band = _band_from_rows(row_fraction, height)
    if band is None:
        raise ValueError("no bright figure band detected (image too dark or blank)")
    y0, y1 = band
    columns = _column_range(data, width, y0, y1)
    if columns is None:
        raise ValueError("no figure column range detected")
    x0, x1 = columns

    width_px = x1 - x0 + 1
    height_px = y1 - y0 + 1
    aspect = width_px / height_px
    height_in = round(assumed_width_in / aspect, 2)
    h_lines, v_lines = _line_positions(data, width, y0, y1, x0, x1)
    dpi, dpi_source = _dpi(path, width_px, assumed_width_in)

    return {
        "image": str(path),
        "tool_version": MODULE_VERSION,
        "measured_at": time.strftime("%Y-%m-%dT%H:%M:%S"),
        "pixel_size": [width, height],
        "figure_bbox": [x0, y0, x1, y1],
        "figure_size": [width_px, height_px],
        "aspect": round(aspect, 4),
        "assumed_width_in": assumed_width_in,
        "canvas_height_in": height_in,
        "dpi": dpi,
        "dpi_source": dpi_source,
        "h_lines": h_lines,
        "v_lines": v_lines,
        "palette_top": _palette(path),
    }
