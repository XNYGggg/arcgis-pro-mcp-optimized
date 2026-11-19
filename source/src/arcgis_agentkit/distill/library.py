"""Template library: atomic index, image dedupe, bundles and job sheets."""

from __future__ import annotations

import contextlib
import hashlib
import json
import os
import re
import secrets
import shutil
import time
import zipfile
from pathlib import Path
from typing import Any, Iterator

from ..common.runtime import MODULE_VERSION, agentkit_home, ensure_dir, write_json_atomic

PACKAGE_ROOT = Path(__file__).resolve().parents[3]
PACKAGE_LIBRARY = PACKAGE_ROOT / "library"
PACKAGE_DEFAULTS = Path(__file__).resolve().parent / "defaults"

DEFAULT_INDEX: dict[str, Any] = {
    "schema_version": 1,
    "revision": "1.1",
    "templates": [],
}


def default_schema() -> dict[str, Any]:
    for path in (PACKAGE_DEFAULTS / "schema.json", PACKAGE_LIBRARY / "schema.json"):
        if path.is_file():
            return json.loads(path.read_text(encoding="utf-8-sig"))
    return {
        "schema_version": 1,
        "type": "object",
        "required": ["schema_version", "id", "name", "status", "source_image", "canvas"],
        "properties": {},
    }


def _sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def _slug(text: str, fallback_prefix: str = "tpl") -> str:
    value = re.sub(r"[^A-Za-z0-9]+", "-", text or "").strip("-").lower()
    if value:
        return value[:48]
    return fallback_prefix + "-" + time.strftime("%Y%m%d-%H%M%S") + "-" + secrets.token_hex(2)


class TemplateLibrary:
    def __init__(self, root: str | Path | None = None,
                 config: dict[str, Any] | None = None) -> None:
        config = config or {}
        configured = root or config.get("library_root") or os.environ.get("ARCGIS_DISTILL_LIBRARY")
        if configured:
            self.root = Path(os.path.expandvars(str(configured)))
        elif PACKAGE_LIBRARY.is_dir():
            self.root = PACKAGE_LIBRARY
        else:
            self.root = agentkit_home() / "library"
        self.images = self.root / "images"
        self.specs = self.root / "specs"
        self.inbox = self.root / "inbox"
        self.schema_path = self.root / "schema.json"
        self.index_path = self.root / "index.json"
        self.jobs = agentkit_home() / "jobs"
        self.ensure()

    # -- setup -------------------------------------------------------------

    def ensure(self) -> None:
        for directory in (self.root, self.images, self.specs, self.inbox, self.jobs):
            ensure_dir(directory)
        if not self.schema_path.is_file():
            write_json_atomic(self.schema_path, default_schema())
        if not self.index_path.is_file():
            write_json_atomic(self.index_path, DEFAULT_INDEX)

    # -- index -------------------------------------------------------------

    @contextlib.contextmanager
    def _lock(self) -> Iterator[None]:
        lock = self.root / ".index.lock"
        acquired = False
        deadline = time.monotonic() + 5
        while time.monotonic() < deadline:
            try:
                descriptor = os.open(str(lock), os.O_CREAT | os.O_EXCL | os.O_WRONLY)
                os.write(descriptor, str(os.getpid()).encode())
                os.close(descriptor)
                acquired = True
                break
            except FileExistsError:
                try:
                    if time.time() - lock.stat().st_mtime > 10:
                        lock.unlink(missing_ok=True)
                        continue
                except OSError:
                    pass
                time.sleep(0.05)
        if not acquired:
            raise TimeoutError(f"template library is locked: {lock}")
        try:
            yield
        finally:
            try:
                lock.unlink(missing_ok=True)
            except OSError:
                pass

    def index(self) -> dict[str, Any]:
        try:
            data = json.loads(self.index_path.read_text(encoding="utf-8-sig"))
        except (OSError, ValueError):
            data = json.loads(json.dumps(DEFAULT_INDEX))
        data.setdefault("schema_version", 1)
        data.setdefault("templates", [])
        return data

    def _write_index(self, data: dict[str, Any]) -> None:
        data["schema_version"] = 1
        data["revision"] = "1.1"
        write_json_atomic(self.index_path, data)

    def _entry(self, template_id: str) -> dict[str, Any] | None:
        for entry in self.index().get("templates", []):
            if entry.get("id") == template_id:
                return entry
        return None

    def _upsert(self, spec: dict[str, Any]) -> None:
        with self._lock():
            data = self.index()
            entries = [entry for entry in data["templates"]
                       if entry.get("id") != spec["id"]]
            entries.append({
                "id": spec["id"],
                "name": spec.get("name", spec["id"]),
                "status": spec.get("status", "imported"),
                "image_sha256": spec.get("image_sha256"),
                "tags": spec.get("tags", []),
                "spec": f"specs/{spec['id']}.json",
                "image": spec.get("source_image"),
                "updated_at": time.strftime("%Y-%m-%dT%H:%M:%S"),
            })
            data["templates"] = sorted(entries, key=lambda item: item["id"])
            self._write_index(data)

    # -- templates ---------------------------------------------------------

    def spec_path(self, template_id: str) -> Path:
        return self.specs / f"{template_id}.json"

    def get(self, template_id: str) -> dict[str, Any]:
        path = self.spec_path(template_id)
        if not path.is_file():
            raise FileNotFoundError(f"template '{template_id}' not found in {self.root}")
        return json.loads(path.read_text(encoding="utf-8-sig"))

    def save(self, spec: dict[str, Any]) -> None:
        write_json_atomic(self.spec_path(spec["id"]), spec)
        self._upsert(spec)

    def list(self, status: str | None = None, tag: str | None = None) -> list[dict[str, Any]]:
        entries = self.index().get("templates", [])
        if status:
            entries = [entry for entry in entries if entry.get("status") == status]
        if tag:
            entries = [entry for entry in entries if tag in (entry.get("tags") or [])]
        return entries

    def import_image(self, image_path: str | Path, name: str,
                     tags: list[str] | None = None, width_in: float = 6.7,
                     template_id: str | None = None) -> dict[str, Any]:
        image_path = Path(image_path)
        if not image_path.is_file():
            raise FileNotFoundError(image_path)
        try:
            from PIL import Image

            with Image.open(image_path) as probe:
                probe.verify()
        except Exception as exc:
            raise ValueError(f"not a readable image: {image_path.name}: {exc}") from exc
        digest = _sha256(image_path)
        for entry in self.index().get("templates", []):
            if entry.get("image_sha256") == digest:
                existing = self.get(entry["id"])
                return {"spec": existing, "skipped": True, "reason": "image_sha256 already imported"}

        template_id = template_id or _slug(name)
        if self.spec_path(template_id).exists():
            template_id = f"{template_id}-{secrets.token_hex(2)}"
            while self.spec_path(template_id).exists():
                template_id = f"{template_id}-{secrets.token_hex(2)}"

        extension = image_path.suffix.lower() or ".png"
        stored_name = digest[:16] + extension
        stored = self.images / stored_name
        if not stored.exists():
            shutil.copyfile(image_path, stored)

        spec = {
            "schema_version": 1,
            "id": template_id,
            "name": name,
            "status": "imported",
            "source_image": f"images/{stored_name}",
            "image_sha256": digest,
            "tags": tags or [],
            "canvas": {
                "page": "custom",
                "width_in": width_in,
                "background": "#FFFFFF",
            },
            "map_frame": {},
            "title": {},
            "legend": {},
            "scale_bar": {},
            "north_arrow": {},
            "palette": {},
            "typography": {},
            "labeling": {},
            "export": {},
            "regions": [],
            "pitfalls": [],
            "notes": "",
        }
        self.save(spec)
        job = {
            "template_id": template_id,
            "image_sha256": digest,
            "created_at": time.strftime("%Y-%m-%dT%H:%M:%S"),
            "steps": {},
        }
        write_json_atomic(self.jobs / f"{template_id}.json", job)
        return {"spec": spec, "skipped": False}

    def image_path(self, spec: dict[str, Any]) -> Path:
        relative = spec.get("source_image", "")
        candidate = (self.root / relative).resolve()
        try:
            candidate.relative_to(self.root.resolve())
        except ValueError as exc:
            raise ValueError("source_image escapes the library root") from exc
        return candidate

    # -- job sheets --------------------------------------------------------

    def job(self, template_id: str) -> dict[str, Any]:
        path = self.jobs / f"{template_id}.json"
        if path.is_file():
            return json.loads(path.read_text(encoding="utf-8-sig"))
        return {"template_id": template_id, "steps": {}}

    def save_job(self, template_id: str, job: dict[str, Any]) -> None:
        write_json_atomic(self.jobs / f"{template_id}.json", job)

    def mark_step(self, template_id: str, step: str, output_hash: str,
                  extra: dict[str, Any] | None = None) -> None:
        job = self.job(template_id)
        record = {
            "tool_version": MODULE_VERSION,
            "at": time.strftime("%Y-%m-%dT%H:%M:%S"),
            "output_hash": output_hash,
        }
        if extra:
            record.update(extra)
        job.setdefault("steps", {})[step] = record
        self.save_job(template_id, job)

    # -- bundles -----------------------------------------------------------

    def export_bundle(self, template_id: str, out_path: str | Path) -> Path:
        spec = self.get(template_id)
        image = self.image_path(spec)
        out_path = Path(out_path)
        ensure_dir(out_path.parent)
        with zipfile.ZipFile(out_path, "w", zipfile.ZIP_DEFLATED) as archive:
            archive.writestr("bundle.json", json.dumps({
                "format": "arcgis-agentkit-template",
                "version": 1,
                "template_id": template_id,
                "exported_at": time.strftime("%Y-%m-%dT%H:%M:%S"),
            }, ensure_ascii=False, indent=2))
            archive.write(self.spec_path(template_id), f"specs/{template_id}.json")
            if image.is_file():
                archive.write(image, f"images/{image.name}")
        return out_path

    def import_bundle(self, bundle_path: str | Path) -> dict[str, Any]:
        bundle_path = Path(bundle_path)
        if not bundle_path.is_file():
            raise FileNotFoundError(bundle_path)
        with zipfile.ZipFile(bundle_path) as archive:
            names = archive.namelist()
            spec_name = next((name for name in names
                              if name.startswith("specs/") and name.endswith(".json")), None)
            if not spec_name:
                raise ValueError("bundle has no specs/*.json")
            spec = json.loads(archive.read(spec_name).decode("utf-8-sig"))
            template_id = spec.get("id") or bundle_path.stem
            target_id = template_id
            if self.spec_path(target_id).exists():
                target_id = f"{target_id}-{secrets.token_hex(2)}"
            spec["id"] = target_id
            digest = spec.get("image_sha256")
            source_image = spec.get("source_image", "")
            stored_name = Path(source_image).name
            if not stored_name:
                stored_name = (digest or target_id)[:16] + ".png"
            if not (self.images / stored_name).exists():
                image_name = next((name for name in names
                                   if name.startswith("images/")), None)
                if not image_name:
                    raise ValueError("bundle has no images/* file")
                with archive.open(image_name) as source:
                    data = source.read()
                (self.images / stored_name).write_bytes(data)
                if not digest:
                    digest = hashlib.sha256(data).hexdigest()
            spec["source_image"] = f"images/{stored_name}"
            spec["image_sha256"] = digest
            self.save(spec)
        return {"spec": spec, "imported": True}

    def inbox_images(self) -> list[Path]:
        return sorted(path for path in self.inbox.iterdir()
                      if path.is_file() and path.suffix.lower() in
                      {".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff"})
