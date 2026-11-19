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

def test_import_rejects_non_image(tmp_path, monkeypatch):
    library_root = tmp_path / "library"
    monkeypatch.setenv("ARCGIS_DISTILL_LIBRARY", str(library_root))
    fake = tmp_path / "not_an_image.txt"
    fake.write_text("this is not an image", encoding="utf-8")
    assert main(["import", str(fake), "--name", "Not an image", "--json"]) != 0
    specs = library_root / "specs"
    assert not specs.exists() or not any(specs.iterdir())


def test_forced_approve_records_a_complete_review(pipeline):
    tmp_path, library, template_id = pipeline
    assert main(["approve", template_id, "--force", "--json"]) == 0
    spec = library.get(template_id)
    assert spec["status"] == "ready"
    review = spec.get("review") or {}
    assert review.get("by") and review.get("date")


def test_delete_removes_template_and_unshared_image(pipeline):
    _tmp, library, template_id = pipeline
    image = library.image_path(library.get(template_id))
    assert image.is_file()
    job = library.jobs / f"{template_id}.json"
    assert job.is_file()

    assert main(["delete", template_id, "--json"]) == 0

    assert library.list() == []
    assert not library.spec_path(template_id).exists()
    assert not image.exists()
    assert not job.exists()


def test_delete_keeps_image_shared_by_another_template(pipeline, tmp_path):
    _tmp, library, template_id = pipeline
    bundle = tmp_path / "bundle.zip"
    assert main(["export", template_id, "--out", str(bundle), "--json"]) == 0
    # bundle-import reuses the stored image file, so two specs now share it.
    assert main(["bundle-import", str(bundle), "--json"]) == 0
    assert len(library.list()) == 2
    image = library.image_path(library.get(template_id))
    assert image.is_file()

    assert main(["delete", template_id, "--json"]) == 0
    assert image.is_file(), "the image is still referenced by the other template"

    remaining = library.list()[0]["id"]
    assert main(["delete", remaining, "--json"]) == 0
    assert not image.exists(), "last reference gone -> image is removed"


def test_delete_unknown_id_is_not_found(pipeline, capsys):
    _tmp, library, template_id = pipeline
    assert main(["delete", "no-such-template", "--json"]) == 3
    envelope = json.loads(capsys.readouterr().out.strip().splitlines()[-1])
    assert envelope["ok"] is False
    assert envelope["code"] == "not_found"

    assert main(["delete", template_id, "--json"]) == 0
    # Idempotent: a second delete reports not_found and changes nothing.
    assert main(["delete", template_id, "--json"]) == 3
    assert library.list() == []


def test_delete_ready_template_requires_force(pipeline):
    _tmp, library, template_id = pipeline
    assert main(["approve", template_id, "--force", "--json"]) == 0
    assert library.get(template_id)["status"] == "ready"

    assert main(["delete", template_id, "--json"]) == 5
    assert library.get(template_id)["status"] == "ready"

    assert main(["delete", template_id, "--force", "--json"]) == 0
    assert library.list() == []


def test_delete_rejects_path_like_ids(pipeline):
    _tmp, library, template_id = pipeline
    outside = _tmp / "outside.json"
    outside.write_text("{}", encoding="utf-8")

    assert main(["delete", "../outside", "--json"]) == 4
    assert outside.is_file(), "path-like ids must never reach outside specs/"
    assert [entry["id"] for entry in library.list()] == [template_id]


def test_delete_clears_orphan_index_entry(pipeline):
    _tmp, library, template_id = pipeline
    # Simulate an interrupted delete that removed the spec before the entry.
    library.spec_path(template_id).unlink()
    assert main(["delete", template_id, "--json"]) == 0
    assert library.list() == []


def test_lock_serializes_concurrent_writers(pipeline):
    import threading
    import time

    _tmp, library, template_id = pipeline
    order = []
    barrier = threading.Barrier(2)

    def worker(name):
        barrier.wait()
        with library._lock():
            order.append(name + "-enter")
            time.sleep(0.2)
            order.append(name + "-exit")

    threads = [threading.Thread(target=worker, args=(name,))
               for name in ("A", "B")]
    for thread in threads:
        thread.start()
    for thread in threads:
        thread.join()
    assert order in (["A-enter", "A-exit", "B-enter", "B-exit"],
                     ["B-enter", "B-exit", "A-enter", "A-exit"]), order


def test_import_does_not_reuse_id_reserved_by_orphan_entry(pipeline, tmp_path):
    _tmp, library, template_id = pipeline
    # An interrupted delete leaves the index entry without the spec; the id
    # must stay reserved until the cleanup finishes.
    library.spec_path(template_id).unlink()
    image = Image.open(tmp_path / "sample.png").convert("RGB")
    image.putpixel((2, 2), (0, 0, 255))
    image.save(tmp_path / "sample4.png")

    assert main(["import", str(tmp_path / "sample4.png"), "--name", "Reuse",
                 "--id", template_id, "--json"]) == 0
    ids = [entry["id"] for entry in library.list()]
    assert template_id in ids, "the orphan entry keeps its id"
    assert any(entry_id != template_id for entry_id in ids), \
        "the new import must take a suffixed id"

    # Finishing the orphan cleanup must not touch the new template.
    assert main(["delete", template_id, "--json"]) == 0
    assert any(entry["id"] != template_id for entry in library.list())
    remaining = [entry["id"] for entry in library.list()
                 if entry["id"] != "demo-figure"]
    for tid in remaining:
        assert main(["delete", tid, "--json"]) == 0


def test_reimported_long_id_stays_deletable(pipeline, tmp_path):
    _tmp, library, template_id = pipeline
    long_id = "a" * 80
    base = Image.open(tmp_path / "sample.png").convert("RGB")
    base.putpixel((0, 0), (255, 0, 0))
    base.save(tmp_path / "sample2.png")
    base.putpixel((1, 1), (0, 255, 0))
    base.save(tmp_path / "sample3.png")

    assert main(["import", str(tmp_path / "sample2.png"), "--name", "L1",
                 "--id", long_id, "--json"]) == 0
    # Second import of the same id hits the collision suffix; the suffixed id
    # must still satisfy the deletion id rule.
    assert main(["import", str(tmp_path / "sample3.png"), "--name", "L2",
                 "--id", long_id, "--json"]) == 0
    ids = sorted(entry["id"] for entry in library.list()
                 if entry["id"].startswith(long_id[:70]))
    assert len(ids) == 2
    for tid in ids:
        assert main(["delete", tid, "--json"]) == 0
    assert not any(entry["id"].startswith(long_id[:70])
                   for entry in library.list())
