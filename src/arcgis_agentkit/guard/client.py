"""Client side of the summon protocol.

The guard engine is a short-lived PowerShell process. Summoning is a file
handshake, language-agnostic on purpose so the C# add-in and any script can
use it:

    sessions/<token>/ready.json   written by the engine once armed
    sessions/<token>/settle       written by the caller when the operation ends
    sessions/<token>/result.json  written by the engine before it exits

A failed summon never blocks the caller's real work: the lease comes back
``ready=False`` with a warning, which is surfaced in the operation result.
"""

from __future__ import annotations

import json
import os
import secrets
import subprocess
import time
from pathlib import Path
from typing import Any

from ..common.runtime import MODULE_VERSION, agentkit_home, ensure_dir, load_config
from .config import find_engine, load_rules, powershell_path

SESSION_ROOT_NAME = "sessions"
REQUEST_DIR_NAME = "requests"


def new_token() -> str:
    return time.strftime("%Y%m%d-%H%M%S") + "-" + secrets.token_hex(4)


class GuardClient:
    def __init__(self, config: dict[str, Any] | None = None) -> None:
        self.config = config or load_config("guard")
        self.rules, self.rules_path = load_rules(config=self.config)
        self.engine = find_engine(self.config)
        state = Path(str(self.config.get("state_dir") or agentkit_home()))
        self.session_root = Path(
            os.path.expandvars(str(self.config.get("session_root")
                                   or state / "guard" / SESSION_ROOT_NAME)))
        self.requests = self.session_root / REQUEST_DIR_NAME
        self.limits = self.rules.get("limits", {})
        self.log_dir = state / "logs"

    # -- process plumbing -------------------------------------------------

    def _command(self, token: str, scope: str, timeout_s: int,
                 alert_only: bool, reason: str) -> list[str]:
        assert self.engine is not None
        command = [
            powershell_path(),
            "-NoProfile",
            "-ExecutionPolicy", "Bypass",
            "-File", str(self.engine),
            "-Token", token,
            "-Scope", scope,
            "-TimeoutSeconds", str(timeout_s),
            "-SessionRoot", str(self.session_root),
        ]
        if self.rules_path:
            command += ["-ConfigPath", str(self.rules_path)]
        if alert_only:
            command.append("-AlertOnly")
        if reason:
            command += ["-Reason", reason]
        return command

    def _spawn(self, token: str, scope: str, timeout_s: int,
               alert_only: bool, reason: str) -> subprocess.Popen:
        ensure_dir(self.requests)
        flags = 0
        if os.name == "nt":
            # CREATE_NO_WINDOW only. DETACHED_PROCESS (0x8) combined with
            # DEVNULL handles makes PowerShell exit before running the script
            # (verified on Windows PowerShell 5.1); the engine still outlives
            # this process because Popen never kills its children.
            flags = 0x08000000
        return subprocess.Popen(
            self._command(token, scope, timeout_s, alert_only, reason),
            cwd=str(self.session_root),
            stdin=subprocess.DEVNULL,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
            creationflags=flags,
            close_fds=True,
        )

    def _wait_ready(self, token: str, timeout_ms: int) -> dict[str, Any] | None:
        ready = self.session_root / token / "ready.json"
        deadline = time.monotonic() + max(0, timeout_ms) / 1000.0
        while time.monotonic() < deadline:
            try:
                if ready.is_file():
                    return json.loads(ready.read_text(encoding="utf-8-sig"))
            except (OSError, ValueError):
                pass
            time.sleep(0.05)
        try:
            if ready.is_file():
                return json.loads(ready.read_text(encoding="utf-8-sig"))
        except (OSError, ValueError):
            return None
        return None

    # -- public API --------------------------------------------------------

    def available(self) -> bool:
        return self.engine is not None

    def summon(self, scope: str = "op", timeout_s: int | None = None,
               alert_only: bool = False, reason: str = "") -> "GuardLease":
        """Summon a guard. Never raises for a missing engine (fail-open)."""
        token = new_token()
        if self.engine is None:
            return GuardLease(self, token, False,
                              "guard engine not found; the trust dialog must be "
                              "clicked by hand", None)
        if scope == "op":
            default_timeout = int(self.limits.get("op_timeout_s", 180))
        else:
            default_timeout = int(self.limits.get("manual_timeout_s", 600))
        timeout_s = int(timeout_s if timeout_s is not None else default_timeout)
        timeout_s = max(10, timeout_s)
        try:
            process = self._spawn(token, scope, timeout_s, alert_only, reason)
        except OSError as exc:
            return GuardLease(self, token, False,
                              f"guard engine could not start: {exc}", None)
        ready_ms = int(self.limits.get("arm_ready_ms", 2000))
        ready = self._wait_ready(token, ready_ms)
        if ready is None:
            return GuardLease(
                self, token, False,
                "guard did not become ready in time; the trust dialog may need "
                f"a manual click (engine log: {self.log_dir / 'guard.jsonl'})",
                process)
        return GuardLease(self, token, True, None, process, ready)

    def summon_detached(self, scope: str = "manual", timeout_s: int = 120,
                        alert_only: bool = False, reason: str = "") -> dict[str, Any]:
        """Summon and return immediately; the engine exits after the timeout."""
        lease = self.summon(scope=scope, timeout_s=timeout_s,
                            alert_only=alert_only, reason=reason)
        return {
            "ok": lease.ready,
            "token": lease.token,
            "ready": lease.ready,
            "engine": str(self.engine) if self.engine else None,
            "engine_version": MODULE_VERSION,
            "scope": scope,
            "timeout_s": timeout_s,
            "alert_only": alert_only,
            "session": str(lease.session_dir),
            "warning": lease.warning,
            "log": str(self.log_dir / "guard.jsonl"),
        }

    def settle(self, token: str, outcome: str = "ok") -> None:
        session = self.session_root / token
        ensure_dir(session)
        try:
            (session / "settle").write_text(
                json.dumps({"outcome": outcome, "at": time.time()}),
                encoding="utf-8")
        except OSError:
            pass

    def read_result(self, token: str) -> dict[str, Any] | None:
        path = self.session_root / token / "result.json"
        try:
            if path.is_file():
                return json.loads(path.read_text(encoding="utf-8-sig"))
        except (OSError, ValueError):
            return None
        return None

    def wait_result(self, token: str, timeout_s: float = 8.0) -> dict[str, Any] | None:
        deadline = time.monotonic() + timeout_s
        while time.monotonic() < deadline:
            result = self.read_result(token)
            if result is not None:
                return result
            time.sleep(0.1)
        return self.read_result(token)

    def status(self) -> dict[str, Any]:
        sessions: list[dict[str, Any]] = []
        if self.session_root.exists():
            for path in sorted(self.session_root.iterdir(), key=lambda p: p.name,
                               reverse=True):
                if not path.is_dir() or path.name == REQUEST_DIR_NAME:
                    continue
                entry: dict[str, Any] = {"token": path.name}
                for name in ("ready.json", "settle", "result.json"):
                    item = path / name
                    if item.exists():
                        entry[name] = True
                        if name.endswith(".json"):
                            try:
                                entry[name.replace(".json", "")] = json.loads(
                                    item.read_text(encoding="utf-8-sig"))
                            except (OSError, ValueError):
                                pass
                sessions.append(entry)
                if len(sessions) >= 20:
                    break
        return {
            "ok": True,
            "engine": str(self.engine) if self.engine else None,
            "engine_found": self.engine is not None,
            "engine_version": MODULE_VERSION,
            "rules": str(self.rules_path) if self.rules_path else None,
            "session_root": str(self.session_root),
            "log": str(self.log_dir / "guard.jsonl"),
            "sessions": sessions,
        }


class GuardLease:
    """Handle on one summoned guard; ``settle`` releases it."""

    def __init__(self, client: GuardClient, token: str, ready: bool,
                 warning: str | None, process: subprocess.Popen | None,
                 ready_info: dict[str, Any] | None = None) -> None:
        self.client = client
        self.token = token
        self.ready = ready
        self.warning = warning
        self.process = process
        self.ready_info = ready_info or {}
        self._settled = False

    @property
    def session_dir(self) -> Path:
        return self.client.session_root / self.token

    def settle(self, outcome: str = "ok") -> None:
        if self._settled:
            return
        self._settled = True
        self.client.settle(self.token, outcome)

    def wait_result(self, timeout_s: float = 8.0) -> dict[str, Any] | None:
        if self.ready:
            return self.client.wait_result(self.token, timeout_s=timeout_s)
        return None

    def __enter__(self) -> "GuardLease":
        return self

    def __exit__(self, exc_type, exc, tb) -> None:
        self.settle("error" if exc_type else "ok")
