"""Shared runtime contract for the AgentKit modules.

Everything here is deliberately dependency-free so the distill CLI keeps
working on a bare Windows installation with no pip packages at all.

Contract (docs/distill.md, ported from the 2.x design):

* state directory ``%LOCALAPPDATA%\\ArcGIS-AgentKit`` (``ARCGIS_AGENTKIT_HOME``
  overrides it),
* JSON output envelope ``{"ok": true, ...}`` /
  ``{"ok": false, "code": "...", "message": "...", "hint": "..."}``,
* exit codes 0 ok / 2 usage / 3 environment / 4 runtime / 5 validation.
"""

from __future__ import annotations

import json
import os
import sys
import time
from pathlib import Path
from typing import Any, Iterable

EXIT_OK = 0
EXIT_USAGE = 2
EXIT_ENV = 3
EXIT_RUNTIME = 4
EXIT_VALIDATION = 5

MODULE_VERSION = "1.1.13"


def agentkit_home() -> Path:
    """The shared state directory, honouring ``ARCGIS_AGENTKIT_HOME``."""
    override = os.environ.get("ARCGIS_AGENTKIT_HOME", "").strip()
    if override:
        return Path(os.path.expandvars(os.path.expanduser(override)))
    local = os.environ.get("LOCALAPPDATA", "").strip()
    if not local:
        local = str(Path.home() / "AppData" / "Local")
    return Path(local) / "ArcGIS-AgentKit"


def ensure_dir(path: Path | str) -> Path:
    path = Path(path)
    path.mkdir(parents=True, exist_ok=True)
    return path


def load_config(module: str | None = None) -> dict[str, Any]:
    """Load ``<home>/config.json`` and flatten the module section.

    Merge order is built-in defaults < user ``config.json`` < CLI arguments.
    A malformed config file is reported instead of silently ignored, because
    silently dropping user settings is how "it worked yesterday" happens.
    """
    config: dict[str, Any] = {"state_dir": str(agentkit_home())}
    path = agentkit_home() / "config.json"
    if path.exists():
        try:
            data = json.loads(path.read_text(encoding="utf-8-sig"))
        except (OSError, ValueError) as exc:
            raise ValueError(f"cannot read {path}: {exc}") from exc
        if not isinstance(data, dict):
            raise ValueError(f"{path} must contain a JSON object")
        for key, value in data.items():
            if key == "modules" and isinstance(value, dict):
                continue
            config[key] = value
        if module and isinstance(data.get("modules"), dict):
            section = data["modules"].get(module)
            if isinstance(section, dict):
                config.update(section)
    return config


def ok(**data: Any) -> dict[str, Any]:
    payload = {"ok": True}
    payload.update(data)
    return payload


def error(code: str, message: str, hint: str | None = None, **extra: Any) -> dict[str, Any]:
    payload = {"ok": False, "code": code, "message": message}
    if hint:
        payload["hint"] = hint
    payload.update(extra)
    return payload


def emit(payload: dict[str, Any], as_json: bool, human: str | None = None) -> None:
    """Print a payload, JSON or human-readable, to stdout."""
    if as_json:
        print(json.dumps(payload, ensure_ascii=False, default=str))
        return
    if human is not None:
        print(human)
        return
    print(json.dumps(payload, ensure_ascii=False, indent=2, default=str))


def fail(message: str, code: str = "runtime_error", hint: str | None = None,
         exit_code: int = EXIT_RUNTIME, as_json: bool = False) -> int:
    emit(error(code, message, hint), as_json, human=f"error: {message}")
    return exit_code


class JsonlLogger:
    """Append-only JSONL log with simple size rotation."""

    def __init__(self, module: str, max_bytes: int = 2 * 1024 * 1024,
                 keep: int = 3) -> None:
        self.module = module
        self.max_bytes = max_bytes
        self.keep = keep
        self.directory = ensure_dir(agentkit_home() / "logs")
        self.path = self.directory / f"{module}.jsonl"

    def _rotate(self) -> None:
        try:
            if not self.path.exists() or self.path.stat().st_size < self.max_bytes:
                return
            for index in range(self.keep - 1, 0, -1):
                source = self.directory / f"{self.module}.{index}.jsonl"
                target = self.directory / f"{self.module}.{index + 1}.jsonl"
                if source.exists():
                    if target.exists():
                        try:
                            target.unlink()
                        except OSError:
                            pass
                    try:
                        source.replace(target)
                    except OSError:
                        import shutil
                        shutil.move(str(source), str(target))
            try:
                self.path.replace(self.directory / f"{self.module}.1.jsonl")
            except OSError:
                import shutil
                shutil.move(str(self.path), str(self.directory / f"{self.module}.1.jsonl"))
        except OSError:
            # Logging must never break the operation it documents.
            pass

    def write(self, event: str, level: str = "info", **fields: Any) -> dict[str, Any]:
        record = {
            "ts": time.strftime("%Y-%m-%dT%H:%M:%S", time.localtime())
                  + f".{int((time.time() % 1) * 1000):03d}",
            "level": level,
            "event": event,
            "module": self.module,
        }
        record.update(fields)
        self._rotate()
        try:
            with self.path.open("a", encoding="utf-8") as handle:
                handle.write(json.dumps(record, ensure_ascii=False, default=str) + "\n")
        except OSError:
            pass
        return record


def find_on_path(name: str) -> str | None:
    """Locate an executable on PATH without requiring ``shutil.which``."""
    candidates = [name]
    if os.name == "nt" and not Path(name).suffix:
        candidates = [name + ext for ext in (".exe", ".cmd", ".bat", ".ps1")]
    for directory in os.environ.get("PATH", "").split(os.pathsep):
        directory = directory.strip('"')
        if not directory:
            continue
        for candidate in candidates:
            path = Path(directory) / candidate
            if path.is_file():
                return str(path)
    return None


def read_json(path: Path | str) -> Any:
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def write_json_atomic(path: Path | str, data: Any) -> None:
    """Write JSON through a temporary file so readers never see half a file."""
    path = Path(path)
    ensure_dir(path.parent)
    temporary = path.with_name(path.name + f".tmp-{os.getpid()}")
    temporary.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n",
                          encoding="utf-8")
    try:
        os.replace(temporary, path)
    except OSError:
        # Some Windows filesystem filters reject os.replace even within one
        # directory (WinError 17); move() falls back to copy+delete.
        import shutil
        shutil.move(str(temporary), str(path))


def mask(value: str, keep: int = 80) -> str:
    """Trim long evidence text for logs."""
    value = " ".join(str(value).split())
    return value if len(value) <= keep else value[: keep - 1] + "…"


def python_executable() -> str:
    return sys.executable


def chunks(items: Iterable[str], size: int) -> list[list[str]]:
    batch: list[str] = []
    result: list[list[str]] = []
    for item in items:
        batch.append(item)
        if len(batch) >= size:
            result.append(batch)
            batch = []
    if batch:
        result.append(batch)
    return result
