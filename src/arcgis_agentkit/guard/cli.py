"""arcgis-guard command line.

Commands
--------
doctor      environment and engine check
summon      summon a guard and wait for it to finish (or --no-wait)
wrap        summon -> run a command -> settle, propagating its exit code
status      active and recent guard sessions
test        alert-window visibility test (>= 30 s)
self-test   scan the live Pro windows for trust dialogs
"""

from __future__ import annotations

import os
import subprocess
import sys
from pathlib import Path
from typing import Any

from ..common.runtime import (
    EXIT_ENV,
    EXIT_OK,
    EXIT_RUNTIME,
    EXIT_USAGE,
    fail,
    load_config,
    ok,
    emit,
)
from .client import GuardClient
from .config import find_engine, load_rules, powershell_path


def _split_json(argv: list[str]) -> tuple[list[str], bool]:
    as_json = "--json" in argv
    return [item for item in argv if item != "--json"], as_json


def _run_engine(client: GuardClient, extra: list[str],
                timeout_s: float = 90) -> subprocess.CompletedProcess:
    assert client.engine is not None
    command = [
        powershell_path(), "-NoProfile", "-ExecutionPolicy", "Bypass",
        "-File", str(client.engine),
    ] + extra
    return subprocess.run(command, capture_output=True, text=True,
                          timeout=timeout_s, encoding="utf-8", errors="replace")


def _pro_running() -> bool:
    if os.name != "nt":
        return False
    try:
        result = subprocess.run(
            ["tasklist", "/FI", "IMAGENAME eq ArcGISPro.exe", "/NH"],
            capture_output=True, text=True, timeout=10,
            encoding="utf-8", errors="replace")
        return "ArcGISPro.exe" in (result.stdout or "")
    except (OSError, subprocess.SubprocessError):
        return False


def _doctor(client: GuardClient, args, as_json: bool) -> int:
    checks: list[dict[str, Any]] = []

    def check(name: str, passed: bool, detail: str, critical: bool = False) -> None:
        checks.append({"check": name, "ok": passed, "detail": detail,
                       "critical": critical})

    check("platform", os.name == "nt", sys.platform)
    check("powershell", Path(powershell_path()).is_file() or powershell_path() != "powershell",
          powershell_path())
    check("engine", client.engine is not None,
          str(client.engine) if client.engine else "guard.ps1 not found", True)
    check("rules", client.rules_path is not None,
          str(client.rules_path) if client.rules_path else "built-in rules only")
    try:
        client.session_root.mkdir(parents=True, exist_ok=True)
        probe = client.session_root / ".write-probe"
        probe.write_text("ok", encoding="utf-8")
        probe.unlink(missing_ok=True)
        writable = True
        detail = str(client.session_root)
    except OSError as exc:
        writable = False
        detail = str(exc)
    check("state_dir", writable, detail, True)
    check("arcgis_pro", _pro_running(),
          "ArcGISPro.exe running" if _pro_running() else "not running")
    check("alert_min_seconds", client.rules["alert"]["min_seconds"] >= 30,
          str(client.rules["alert"]["min_seconds"]))

    ok_all = all(item["ok"] for item in checks if item["critical"])
    payload = ok(**{
        "module": "guard",
        "version": "2.0.0",
        "status": "ready" if ok_all else "not-ready",
        "checks": checks,
    })
    if as_json:
        emit(payload, True)
    else:
        print("arcgis-guard doctor")
        for item in checks:
            mark = "OK  " if item["ok"] else "FAIL"
            critical = " (critical)" if item["critical"] and not item["ok"] else ""
            print(f"  [{mark}] {item['check']}: {item['detail']}{critical}")
    return EXIT_OK if ok_all else EXIT_ENV


def _summon(client: GuardClient, args, as_json: bool) -> int:
    lease = client.summon(scope=args.scope, timeout_s=args.timeout,
                          alert_only=args.alert_only, reason="cli summon")
    payload = ok(**{
        "token": lease.token,
        "ready": lease.ready,
        "scope": args.scope,
        "session": str(lease.session_dir),
        "engine": str(client.engine) if client.engine else None,
        "warning": lease.warning,
    })
    if not args.no_wait:
        timeout = (args.timeout if args.timeout is not None
                   else int(client.limits.get("manual_timeout_s", 600))) + 15
        result = lease.wait_result(timeout_s=timeout)
        payload["result"] = result
    if as_json:
        emit(payload, True)
    else:
        print(f"guard token: {lease.token}")
        print(f"ready: {lease.ready}  session: {lease.session_dir}")
        if lease.warning:
            print(f"warning: {lease.warning}")
        if not args.no_wait and payload.get("result"):
            print(f"result: {payload['result'].get('exit_reason')}")
    return EXIT_OK if lease.ready else EXIT_RUNTIME


def _wrap(client: GuardClient, args, as_json: bool) -> int:
    command = args.command
    if command and command[0] == "--":
        command = command[1:]
    if not command:
        return fail("wrap needs a command, e.g. arcgis-guard wrap -- run.cmd",
                    "usage", exit_code=EXIT_USAGE, as_json=as_json)
    timeout = args.timeout
    lease = client.summon(scope="op", timeout_s=timeout,
                          reason="wrap: " + " ".join(command)[:200])
    code = EXIT_RUNTIME
    try:
        try:
            child = subprocess.Popen(command)
        except FileNotFoundError:
            lease.settle("not-found")
            return fail(f"command not found: {command[0]}", "env_missing",
                        exit_code=EXIT_ENV, as_json=as_json)
        try:
            code = child.wait(timeout=timeout)
        except subprocess.TimeoutExpired:
            child.kill()
            code = EXIT_RUNTIME
    finally:
        lease.settle("ok" if code == 0 else "error")
    result = lease.wait_result(timeout_s=10)
    if as_json:
        emit(ok(**{"command": command, "exit_code": code, "token": lease.token,
                   "guard_ready": lease.ready, "guard_result": result,
                   "warning": lease.warning}), True)
    else:
        if lease.warning:
            print(f"warning: {lease.warning}", file=sys.stderr)
        print(f"exit code: {code}")
    return int(code) if code is not None else EXIT_RUNTIME


def _status(client: GuardClient, args, as_json: bool) -> int:
    payload = client.status()
    if as_json:
        emit(payload, True)
    else:
        print(f"engine: {payload['engine'] or 'not found'}")
        print(f"sessions: {payload['session_root']}")
        if not payload["sessions"]:
            print("  (none)")
        for session in payload["sessions"]:
            flags = [name for name in ("ready.json", "settle", "result.json")
                     if session.get(name)]
            print(f"  {session['token']}: {', '.join(flags) or 'new'}")
    return EXIT_OK


def _test(client: GuardClient, args, as_json: bool) -> int:
    if client.engine is None:
        return fail("guard engine not found", "env_missing", exit_code=EXIT_ENV,
                    as_json=as_json)
    seconds = max(30, args.seconds)
    result = _run_engine(client, ["-NotifyTest", "-TestSeconds", str(seconds)],
                         timeout_s=seconds + 20)
    payload = ok(**{"test": "notify", "seconds": seconds,
                    "exit_code": result.returncode,
                    "stdout": (result.stdout or "").strip()})
    emit(payload, as_json, human=f"alert window shown for {seconds}s (exit {result.returncode})")
    return EXIT_OK if result.returncode == 0 else EXIT_RUNTIME


def _self_test(client: GuardClient, args, as_json: bool) -> int:
    if client.engine is None:
        return fail("guard engine not found", "env_missing", exit_code=EXIT_ENV,
                    as_json=as_json)
    extra = ["-SelfTest"]
    if args.text:
        extra += ["-TestText", args.text]
    result = _run_engine(client, extra, timeout_s=60)
    payload = ok(**{"test": "self-test", "exit_code": result.returncode,
                    "stdout": (result.stdout or "").strip(),
                    "stderr": (result.stderr or "").strip()})
    if as_json:
        emit(payload, True)
    else:
        print((result.stdout or "").strip())
        if result.stderr:
            print((result.stderr or "").strip(), file=sys.stderr)
    return EXIT_OK if result.returncode == 0 else EXIT_RUNTIME


def _build_parser():
    import argparse
    parser = argparse.ArgumentParser(
        prog="arcgis-guard",
        description="On-demand dialog guard for ArcGIS Pro trust dialogs.")
    sub = parser.add_subparsers(dest="command", required=False)

    doctor = sub.add_parser("doctor", help="environment and engine check")
    doctor.set_defaults(func=_doctor)

    summon = sub.add_parser("summon", help="summon a guard")
    summon.add_argument("--scope", choices=("op", "manual"), default="manual")
    summon.add_argument("--timeout", type=int, default=None)
    summon.add_argument("--alert-only", action="store_true")
    summon.add_argument("--no-wait", action="store_true")
    summon.set_defaults(func=_summon)

    wrap = sub.add_parser("wrap", help="summon, run a command, settle")
    wrap.add_argument("--timeout", type=int, default=180)
    wrap.add_argument("command", nargs=argparse.REMAINDER)
    wrap.set_defaults(func=_wrap)

    status = sub.add_parser("status", help="guard session status")
    status.set_defaults(func=_status)

    test = sub.add_parser("test", help="alert-window visibility test")
    test.add_argument("--seconds", type=int, default=30)
    test.set_defaults(func=_test)

    self_test = sub.add_parser("self-test", help="scan live Pro windows")
    self_test.add_argument("--text", default="",
                           help="classify this text against the rules instead")
    self_test.set_defaults(func=_self_test)

    return parser


def main(argv: list[str] | None = None) -> int:
    argv, as_json = _split_json(list(sys.argv[1:] if argv is None else argv))
    parser = _build_parser()
    args = parser.parse_args(argv)
    if not getattr(args, "command", None):
        args.command = "doctor"
        args.func = _doctor
    try:
        config = load_config("guard")
        _rules, _source = load_rules(config=config)
        client = GuardClient(config)
    except ValueError as exc:
        return fail(str(exc), "config_error", exit_code=EXIT_ENV, as_json=as_json)
    try:
        return int(args.func(client, args, as_json))
    except subprocess.TimeoutExpired:
        return fail("guard engine did not finish in time", "timeout",
                    exit_code=EXIT_RUNTIME, as_json=as_json)


if __name__ == "__main__":  # pragma: no cover
    raise SystemExit(main())
