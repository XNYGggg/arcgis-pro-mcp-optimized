import json
from pathlib import Path

import pytest

pytest.importorskip("PIL")

from PIL import Image, ImageDraw

from arcgis_agentkit.distill.cli import main
from arcgis_agentkit.distill.library import TemplateLibrary


def make_sample(path: Path) -> Path:
    image = Image.new("RGB", (600, 400), "white")
    draw = ImageDraw.Draw(image)
    draw.rectangle([45, 55, 555, 345], outline="black", width=2)
    draw.rectangle([60, 80, 300, 235], fill=(180, 210, 160))
    draw.rectangle([310, 80, 540, 235], fill=(160, 190, 220))
    draw.rectangle([60, 245, 540, 330], fill=(235, 225, 190))
    draw.rectangle([460, 260, 530, 325], outline="black", width=2)
    image.save(path)
    return path


@pytest.fixture()
def pipeline(tmp_path, monkeypatch):
    library_root = tmp_path / "library"
    monkeypatch.setenv("ARCGIS_DISTILL_LIBRARY", str(library_root))
    sample = make_sample(tmp_path / "sample.png")
    assert main(["import", str(sample), "--name", "Demo figure", "--json"]) == 0
    library = TemplateLibrary()
    template_id = library.list()[0]["id"]
    return tmp_path, library, template_id


def test_import_is_deduplicated_by_sha(pipeline):
    tmp_path, library, template_id = pipeline
    sample = tmp_path / "sample.png"
    assert main(["import", str(sample), "--name", "Again", "--json"]) == 0
    assert len(library.list()) == 1

    spec = library.get(template_id)
    assert spec["image_sha256"]
    assert spec["status"] == "imported"


def test_full_pipeline_reaches_ready_and_applies(pipeline):
    tmp_path, library, template_id = pipeline

    assert main(["measure", template_id, "--json"]) == 0
    spec = library.get(template_id)
    measured = spec["measured"]
    assert measured["aspect"] > 0
    assert len(measured["h_lines"]) >= 1
    assert spec["status"] == "measured"

    y0 = measured["h_lines"][0]
    patch = {
        "map_frame": {"x": 0.07, "y": y0, "width": 0.86, "height": 0.72},
        "title": {"text": "Demo figure", "size": 20},
        "legend": {"x": 0.73, "y": 0.62, "w": 0.16, "h": 0.18},
        "scale_bar": {"x": 0.08, "y": 0.9, "w": 0.2, "h": 0.04},
        "north_arrow": {"x": 0.88, "y": 0.1, "w": 0.05, "h": 0.08},
        "typography": {"title_size": 20, "legend_size": 9, "label_size": 10},
        "palette": {"colors": ["#B4D2A0", "#A0BEDC"]},
        "regions": [
            {"name": "map", "role": "map", "x": 0.07, "y": y0, "w": 0.86, "h": 0.72},
        ],
        "pitfalls": [{"issue": "legend overlap", "check": "legend inside frame",
                      "severity": "warn"}],
    }
    patch_file = tmp_path / "patch.json"
    patch_file.write_text(json.dumps(patch), encoding="utf-8")
    assert main(["patch", template_id, "--file", str(patch_file), "--json"]) == 0
    assert library.get(template_id)["status"] == "analyzed"

    assert main(["validate", template_id, "--json"]) == 0
    assert library.get(template_id)["status"] == "validated"

    preview = tmp_path / "preview.png"
    assert main(["preview", template_id, "--out", str(preview), "--json"]) == 0
    assert preview.is_file() and preview.stat().st_size > 0

    assert main(["review", template_id, "--by", "pytest", "--json"]) == 0
    assert main(["approve", template_id, "--json"]) == 0
    assert library.get(template_id)["status"] == "ready"

    context = tmp_path / "context.json"
    assert main(["apply", template_id, "--out", str(context), "--json"]) == 0
    assert json.loads(context.read_text(encoding="utf-8"))["template_id"] == template_id

    bundle = tmp_path / "bundle.zip"
    assert main(["export", template_id, "--out", str(bundle), "--json"]) == 0
    assert bundle.is_file()


def test_patch_rejects_measured_and_outside_geometry(pipeline, tmp_path):
    _tmp, library, template_id = pipeline
    assert main(["measure", template_id, "--json"]) == 0

    measured_patch = tmp_path / "measured.json"
    measured_patch.write_text(json.dumps({"measured": {"aspect": 9}}), encoding="utf-8")
    assert main(["patch", template_id, "--file", str(measured_patch), "--json"]) == 2

    bad = tmp_path / "bad.json"
    bad.write_text(json.dumps({"legend": {"x": 1.1, "y": 0.2, "w": 0.2, "h": 0.2}}),
                   encoding="utf-8")
    assert main(["patch", template_id, "--file", str(bad), "--json"]) == 5
    assert library.get(template_id)["legend"] == {}


def test_approve_requires_review(pipeline):
    _tmp, library, template_id = pipeline
    assert main(["measure", template_id, "--json"]) == 0
    assert main(["approve", template_id, "--json"]) == 5
    assert library.get(template_id)["status"] != "ready"


def test_bundle_round_trip(pipeline, tmp_path, monkeypatch):
    _tmp, library, template_id = pipeline
    assert main(["measure", template_id, "--json"]) == 0
    bundle = tmp_path / "bundle.zip"
    assert main(["export", template_id, "--out", str(bundle), "--json"]) == 0

    other_root = tmp_path / "other-library"
    monkeypatch.setenv("ARCGIS_DISTILL_LIBRARY", str(other_root))
    assert main(["bundle-import", str(bundle), "--json"]) == 0
    other = TemplateLibrary()
    assert len(other.list()) == 1
    assert other.list()[0]["status"] == "measured"
