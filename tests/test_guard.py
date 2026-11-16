import json
import os
import subprocess
import sys
from pathlib import Path

import pytest

from arcgis_agentkit.guard.config import PACKAGE_ENGINE, load_rules, powershell_path


def test_alert_minimum_is_never_below_30(tmp_path):
    override = tmp_path / "rules.json"
    override.write_text(json.dumps({"alert": {"min_seconds": 5}}), encoding="utf-8")
    rules, source = load_rules(explicit_path=override)
    assert rules["alert"]["min_seconds"] == 30
    assert source == override


def test_rules_have_the_safety_lists():
    rules, _ = load_rules()
    match = rules["match"]
    assert "第三方" in match["strong"]
    assert "Python" in match["weak"]
    assert "覆盖" in match["deny"]
    assert "delete" in match["deny"]


@pytest.mark.skipif(os.name != "nt", reason="guard engine is Windows PowerShell")
def test_rule_classification_via_engine():
    samples = {
        "third-party code library Python command, allow?": "click",
        "Overwrite the existing output dataset?": "alert",
        "Save changes to the project?": "none",
    }
    for text, expected in samples.items():
        result = subprocess.run(
            [powershell_path(), "-NoProfile", "-ExecutionPolicy", "Bypass",
             "-File", str(PACKAGE_ENGINE), "-SelfTest", "-TestText", text],
            capture_output=True, text=True, encoding="utf-8", errors="replace",
            timeout=60)
        assert result.returncode == 0, result.stderr
        payload = json.loads(result.stdout.strip().splitlines()[-1])
        assert payload["action"] == expected, (text, payload)


@pytest.mark.skipif(os.name != "nt", reason="guard engine is Windows PowerShell")
def test_summon_handshake_and_settle(tmp_path, monkeypatch):
    monkeypatch.setenv("ARCGIS_AGENTKIT_HOME", str(tmp_path / "state"))
    from arcgis_agentkit.guard.client import GuardClient

    client = GuardClient()
    lease = client.summon(scope="op", timeout_s=10, reason="pytest")
    try:
        assert lease.ready, lease.warning
        assert (lease.session_dir / "ready.json").is_file()
        ready = json.loads((lease.session_dir / "ready.json").read_text(encoding="utf-8"))
        assert ready["engine_version"] == "2.0.0"
    finally:
        lease.settle("test")
    result = lease.wait_result(timeout_s=15)
    assert result is not None
    assert result["token"] == lease.token
