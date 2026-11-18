# -*- coding: utf-8 -*-
"""v1.1.6 local deployment, run inside ArcGIS Pro through RunCode.pyt.

Steps: verify the candidate's hash, back up the installed add-in (next to it
and into release/v1.1.6-artifacts), install the candidate, and record
installed.json for the local-upgrade bookkeeping.
"""
import hashlib
import json
import os
import shutil
import time

SRC = (r"E:\ai工具\claude code_app\project_total\01_project_arcgispro"
       r"\release\v1.1.6-src\artifacts\candidates\pro-3.7\ArcGISProMCP.esriAddinX")
EXPECTED_SHA = "96C7829E2A26E16C270722A84605F9945DA2348DFCF202C658A64EA9E012E625"
DOCS = r"C:\Users\123\Documents\ArcGIS\AddIns\ArcGISPro"
TARGET = os.path.join(DOCS, "ArcGISProMCP.esriAddinX")
ART = (r"E:\ai工具\claude code_app\project_total\01_project_arcgispro"
       r"\release\v1.1.6-artifacts")
INSTALLED = os.path.join(os.environ["LOCALAPPDATA"], "ArcGIS Pro MCP", "installed.json")


def sha(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


report = {"target": TARGET}
report["source_sha256"] = sha(SRC)
report["expected"] = EXPECTED_SHA
if report["source_sha256"] != EXPECTED_SHA:
    raise RuntimeError("source hash mismatch: " + report["source_sha256"])

stamp = time.strftime("%Y%m%d-%H%M%S")
if os.path.exists(TARGET):
    report["old_sha256"] = sha(TARGET)
    report["old_size"] = os.path.getsize(TARGET)
    docs_backup = TARGET + ".bak-" + stamp
    shutil.copy2(TARGET, docs_backup)
    report["backup_in_docs"] = docs_backup
    os.makedirs(ART, exist_ok=True)
    art_backup = os.path.join(ART, "installed-before-upgrade.esriAddinX")
    shutil.copy2(TARGET, art_backup)
    report["backup_artifact"] = art_backup

shutil.copy2(SRC, TARGET)
report["new_sha256"] = sha(TARGET)
report["new_size"] = os.path.getsize(TARGET)

previous = {}
try:
    with open(INSTALLED, encoding="utf-8") as handle:
        previous = json.load(handle)
except Exception:
    pass
previous.update({
    "version": "1.1.6",
    "addin_sha256": report["new_sha256"],
    "addin_size": report["new_size"],
    "installed_at": time.strftime("%Y-%m-%dT%H:%M:%S%z"),
    "package_root": (r"E:\ai工具\claude code_app\project_total\01_project_arcgispro"
                     r"\release\v1.1.6-src"),
})
os.makedirs(os.path.dirname(INSTALLED), exist_ok=True)
with open(INSTALLED, "w", encoding="utf-8") as handle:
    json.dump(previous, handle, ensure_ascii=False, indent=1)
report["installed_json"] = INSTALLED
report["installed_json_content"] = previous

print(json.dumps(report, ensure_ascii=False, indent=1))
