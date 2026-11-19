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


_TEMPLATE_ID = re.compile(r"^[A-Za-z0-9][A-Za-z0-9._-]{0,79}$")


def valid_template_id(template_id: str) -> bool:
    """Ids become file names; path separators and '..' components are out."""
    if not template_id or ".." in template_id:
        return False
    return bool(_TEMPLATE_ID.fullmatch(template_id))


# Kept for the original private spelling used by older callers.
_valid_template_id = valid_template_id


def _try_os_lock(descriptor: int) -> None:
    """Non-blocking exclusive lock on the open lock file.

    The OS releases it when the holding process exits, so there is no stale
    lock to steal and no check-then-unlink window between two waiters.
    """
    if os.name == "nt":
        import msvcrt

        os.lseek(descriptor, 0, os.SEEK_SET)
        msvcrt.locking(descriptor, msvcrt.LK_NBLCK, 1)
    else:
        import fcntl

        fcntl.flock(descriptor, fcntl.LOCK_EX | fcntl.LOCK_NB)


def _release_os_lock(descriptor: int) -> None:
    if os.name == "nt":
        import msvcrt

        os.lseek(descriptor, 0, os.SEEK_SET)
        msvcrt.locking(descriptor, msvcrt.LK_UNLCK, 1)
    else:
        import fcntl

        fcntl.flock(descriptor, fcntl.LOCK_UN)


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
        # An OS advisory lock on a persistent file: released automatically
        # when the holder dies (crash included), and two waiters cannot both
        # enter because nobody ever unlinks the lock by name.
        lock = self.root / ".index.lock"
        descriptor = os.open(str(lock), os.O_CREAT | os.O_RDWR)
        acquired = False
        deadline = time.monotonic() + 10
        try:
            while time.monotonic() < deadline:
                try:
                    _try_os_lock(descriptor)
                    acquired = True
                    break
                except OSError:
                    time.sleep(0.05)
            if not acquired:
                raise TimeoutError(f"template library is locked: {lock}")
            try:
                os.ftruncate(descriptor, 0)
                os.lseek(descriptor, 0, os.SEEK_SET)
                os.write(descriptor, str(os.getpid()).encode())
                yield
            finally:
                try:
                    _release_os_lock(descriptor)
                except OSError:
                    pass
        finally:
            os.close(descriptor)

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

    def _canonical_id(self, template_id: str,
                      entries: list[dict[str, Any]]) -> str:
        """Resolve the stored spelling of an id.

        Windows paths are case-insensitive while the index is not: without
        this, deleting 'ABC' would remove specs/abc.json and leave the index
        entry behind. The index entry wins, then the on-disk spec name.
        """
        lowered = template_id.lower()
        for entry in entries:
            if str(entry.get("id", "")).lower() == lowered:
                return str(entry["id"])
        if not (self.specs / f"{template_id}.json").is_file():
            for path in self.specs.glob("*.json"):
                if path.stem.lower() == lowered:
                    return path.stem
        return template_id

    def _id_taken(self, template_id: str,
                  entries: list[dict[str, Any]]) -> bool:
        """An id is taken if a spec OR an index entry uses it.

        An interrupted delete can leave the entry behind without the spec; a
        new import must not claim that id, or the pending cleanup would later
        delete the wrong template.
        """
        lowered = template_id.lower()
        if self.spec_path(template_id).exists():
            return True
        return any(str(entry.get("id", "")).lower() == lowered
                   for entry in entries)

    def _apply_upsert(self, data: dict[str, Any], spec: dict[str, Any]) -> None:
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

    def _upsert(self, spec: dict[str, Any]) -> None:
        with self._lock():
            self._apply_upsert(self.index(), spec)

    # -- templates ---------------------------------------------------------

    def spec_path(self, template_id: str) -> Path:
        return self.specs / f"{template_id}.json"

    def get(self, template_id: str) -> dict[str, Any]:
        path = self.spec_path(template_id)
        if not path.is_file():
            raise FileNotFoundError(f"template '{template_id}' not found in {self.root}")
        return json.loads(path.read_text(encoding="utf-8-sig"))

    def save(self, spec: dict[str, Any]) -> None:
        with self._lock():
            write_json_atomic(self.spec_path(spec["id"]), spec)
            self._apply_upsert(self.index(), spec)

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
        # One lock for the whole import: the dedupe check, the image copy, the
        # spec write and the index commit must not interleave with a delete,
        # which decides image liveness from the same records under this lock.
        with self._lock():
            entries = self.index().get("templates", [])
            for entry in entries:
                if entry.get("image_sha256") == digest:
                    existing = self.get(entry["id"])
                    return {"spec": existing, "skipped": True,
                            "reason": "image_sha256 already imported"}

            template_id = template_id or _slug(name)
            if not valid_template_id(template_id):
                raise ValueError(
                    f"invalid template id: {template_id!r} (letters, digits, "
                    "'.', '_' and '-' only; it becomes a file name)")
            if self._id_taken(template_id, entries):
                # Truncate before suffixing: the result must still satisfy the
                # same 80-character rule, or the template could never be deleted.
                base = template_id[:75]
                template_id = f"{base}-{secrets.token_hex(2)}"
                while self._id_taken(template_id, entries):
                    template_id = f"{base}-{secrets.token_hex(2)}"

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
            write_json_atomic(self.spec_path(spec["id"]), spec)
            self._apply_upsert(self.index(), spec)
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

    # -- deletion ----------------------------------------------------------

    def delete(self, template_id: str, force: bool = False) -> dict[str, Any]:
        """Delete a template: spec, index entry, job sheet and unshared image.

        A second delete of the same id reports ``not_found`` and changes
        nothing (the CLI maps that to exit code 3). An approved (``ready``)
        template is the product of a review someone signed off on, so it needs
        ``force`` - the same gate ``approve`` uses in reverse.
        """
        with self._lock():
            if not valid_template_id(template_id):
                raise ValueError(
                    f"invalid template id: {template_id!r} (letters, digits, "
                    "'.', '_' and '-' only; it becomes a file name)")
            data = self.index()
            entries = data.get("templates", [])
            template_id = self._canonical_id(template_id, entries)
            entry = next((item for item in entries if item.get("id") == template_id), None)
            path = self.spec_path(template_id)
            if entry is None and not path.is_file():
                raise FileNotFoundError(
                    f"template '{template_id}' not found in {self.root}")
            spec: dict[str, Any] = {}
            if path.is_file():
                try:
                    spec = json.loads(path.read_text(encoding="utf-8-sig"))
                except (OSError, ValueError):
                    spec = {}
            status = spec.get("status") or (entry or {}).get("status") or "unknown"
            if status == "ready" and not force:
                raise ValueError(
                    f"template '{template_id}' is ready; deleting an approved "
                    "template needs force")

            image_relative = spec.get("source_image") or (entry or {}).get("image")
            image_digest = spec.get("image_sha256") or (entry or {}).get("image_sha256")
            errors: list[str] = []

            # The image is stored once per sha256 and may be shared by templates
            # that arrived through bundle-import, so it is removed only when no
            # remaining spec references the same file or digest.
            remaining = [item for item in entries if item.get("id") != template_id]
            image_referenced_by: list[str] = []
            image_removed = False
            if image_relative:
                for other in remaining:
                    if other.get("image") == image_relative or (
                            image_digest and other.get("image_sha256") == image_digest):
                        image_referenced_by.append(other.get("id"))
                if not image_referenced_by:
                    for other_path in self.specs.glob("*.json"):
                        if other_path.stem == template_id:
                            continue
                        try:
                            other_spec = json.loads(
                                other_path.read_text(encoding="utf-8-sig"))
                        except (OSError, ValueError):
                            # An unreadable spec may reference the image; keep it.
                            image_referenced_by.append(other_path.stem)
                            continue
                        if other_spec.get("source_image") == image_relative or (
                                image_digest
                                and other_spec.get("image_sha256") == image_digest):
                            image_referenced_by.append(
                                other_spec.get("id") or other_path.stem)
                if not image_referenced_by:
                    candidate = (self.root / str(image_relative)).resolve()
                    try:
                        candidate.relative_to(self.root.resolve())
                        inside_images = candidate.parent == self.images.resolve()
                    except ValueError:
                        inside_images = False
                    if inside_images and candidate.is_file():
                        try:
                            candidate.unlink()
                            image_removed = True
                        except OSError as exc:
                            errors.append(f"image {image_relative}: {exc}")

            job_path = self.jobs / f"{template_id}.json"
            job_removed = False
            if job_path.is_file():
                try:
                    job_path.unlink()
                    job_removed = True
                except OSError as exc:
                    errors.append(f"job sheet {template_id}.json: {exc}")

            # Files first, index last: if the spec cannot be removed the index
            # entry stays, so a retry still finds the template and can finish.
            removed_spec = False
            if path.is_file():
                try:
                    path.unlink()
                    removed_spec = True
                except OSError as exc:
                    errors.append(f"spec {template_id}.json: {exc}")
            removed_entry = False
            if not errors and (removed_spec or not path.is_file()):
                data["templates"] = remaining
                self._write_index(data)
                removed_entry = entry is not None

            return {
                "template_id": template_id,
                "status": status,
                "entry_present": entry is not None,
                "removed": {
                    "index_entry": removed_entry,
                    "spec": removed_spec,
                    "image": image_relative if image_removed else None,
                    "image_kept_for": sorted(item for item in image_referenced_by if item),
                    "job": job_removed,
                },
                "errors": errors,
            }

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
            # Id allocation, image reuse/write, spec write and index commit all
            # under one lock: a concurrent delete must not see the image as
            # unreferenced between the existence check and the spec commit.
            with self._lock():
                declared = str(spec.get("id") or "")
                template_id = (declared if valid_template_id(declared)
                               else _slug(declared or bundle_path.stem))
                entries = self.index().get("templates", [])
                target_id = template_id
                if self._id_taken(target_id, entries):
                    base = target_id[:75]
                    target_id = f"{base}-{secrets.token_hex(2)}"
                    while self._id_taken(target_id, entries):
                        target_id = f"{base}-{secrets.token_hex(2)}"
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
                write_json_atomic(self.spec_path(spec["id"]), spec)
                self._apply_upsert(self.index(), spec)
        return {"spec": spec, "imported": True}

    def inbox_images(self) -> list[Path]:
        return sorted(path for path in self.inbox.iterdir()
                      if path.is_file() and path.suffix.lower() in
                      {".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff"})
