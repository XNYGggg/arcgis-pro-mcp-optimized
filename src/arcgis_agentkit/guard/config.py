"""Guard configuration: built-in rules, user overrides and engine discovery."""

from __future__ import annotations

import json
import os
from pathlib import Path
from typing import Any

from ..common.runtime import agentkit_home, load_config

DEFAULT_RULES: dict[str, Any] = {
    "version": 1,
    "match": {
        "strong": [
            "第三方", "代码库", "第三方代码", "third-party", "third party",
            "code library", "code-library",
        ],
        "weak": ["Python"],
        "affirm_buttons": ["是", "允许", "Yes", "Allow"],
        "tallow": ["允许", "执行", "信任", "trust", "allow", "run"],
        "deny": [
            "覆盖", "删除", "移除", "卸载", "清空", "格式化",
            "overwrite", "delete", "remove", "uninstall", "erase", "replace",
        ],
    },
    "alert": {
        "min_seconds": 30,
        "sound": True,
        "position": "top-right",
    },
    "limits": {
        "arm_ready_ms": 2000,
        "settle_grace_s": 3,
        "op_timeout_s": 180,
        "manual_timeout_s": 600,
        "poll_active_ms": 400,
        "poll_grace_ms": 1500,
    },
}

PACKAGE_ENGINE = Path(__file__).resolve().parent / "engine" / "guard.ps1"
PACKAGE_RULES = Path(__file__).resolve().parent / "engine" / "guard.schema.json"


def _deep_merge(base: dict[str, Any], override: dict[str, Any]) -> dict[str, Any]:
    result = json.loads(json.dumps(base))
    for key, value in override.items():
        if isinstance(value, dict) and isinstance(result.get(key), dict):
            result[key] = _deep_merge(result[key], value)
        else:
            result[key] = value
    return result


def _read_rules(path: Path | None) -> dict[str, Any]:
    if path is None or not path.exists():
        return {}
    data = json.loads(path.read_text(encoding="utf-8-sig"))
    if not isinstance(data, dict):
        raise ValueError(f"{path} must contain a JSON object")
    return data


def load_rules(explicit_path: str | Path | None = None,
               config: dict[str, Any] | None = None) -> tuple[dict[str, Any], Path | None]:
    """Merge built-in rules, package rules, user rules and an explicit file."""
    config = config or load_config("guard")
    rules = json.loads(json.dumps(DEFAULT_RULES))

    package = _read_rules(PACKAGE_RULES)
    rules = _deep_merge(rules, package)

    user_path = (os.environ.get("ARCGIS_AGENTKIT_GUARD_RULES", "").strip()
                 or config.get("rules_path") or config.get("guard_rules"))
    if user_path:
        rules = _deep_merge(rules, _read_rules(Path(os.path.expandvars(str(user_path)))))

    source: Path | None = PACKAGE_RULES if PACKAGE_RULES.exists() else None
    if explicit_path:
        explicit = Path(os.path.expandvars(str(explicit_path)))
        rules = _deep_merge(rules, _read_rules(explicit))
        source = explicit

    minimum = rules.get("alert", {}).get("min_seconds", 30)
    try:
        minimum = int(minimum)
    except (TypeError, ValueError):
        minimum = 30
    rules.setdefault("alert", {})["min_seconds"] = max(30, minimum)
    return rules, source


def engine_candidates(config: dict[str, Any] | None = None) -> list[Path]:
    config = config or load_config("guard")
    candidates: list[Path] = []
    env = os.environ.get("ARCGIS_AGENTKIT_GUARD_ENGINE", "").strip()
    if env:
        candidates.append(Path(os.path.expandvars(env)))
    configured = config.get("engine_path")
    if configured:
        candidates.append(Path(os.path.expandvars(str(configured))))
    candidates.append(PACKAGE_ENGINE)
    candidates.append(agentkit_home() / "guard" / "engine" / "guard.ps1")
    local = os.environ.get("LOCALAPPDATA", "")
    if local:
        candidates.append(Path(local) / "ArcGIS Pro MCP" / "guard" / "guard.ps1")
        candidates.append(Path(local) / "ArcGIS Pro MCP" / "guard" / "guard.ps1")
    return candidates


def find_engine(config: dict[str, Any] | None = None) -> Path | None:
    for candidate in engine_candidates(config):
        if candidate.is_file():
            return candidate
    return None


def powershell_path() -> str:
    root = os.environ.get("SystemRoot", r"C:\Windows")
    candidate = Path(root) / "System32" / "WindowsPowerShell" / "v1.0" / "powershell.exe"
    return str(candidate) if candidate.is_file() else "powershell"
