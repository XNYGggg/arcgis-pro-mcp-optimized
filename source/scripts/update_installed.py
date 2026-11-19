# -*- coding: utf-8 -*-
"""Record a locally installed add-in build in installed.json.

Run inside ArcGIS Pro through RunCode.pyt (positional arguments only, which
is the only form the native add-in passes):

    update_installed.py <version> <expected_sha256> [note]

This replaces the one-off update_installed_117/118/119.py scripts: the
version, the expected hash and the note are the only things that change
between releases, so they are arguments instead of a new file each time.

Verifies that the installed add-in's SHA-256 equals <expected_sha256> before
touching the bookkeeping file, then updates version / addin_sha256 /
addin_size / package_root / note.

Environment overrides (for another machine):
    MCP_ADDIN_PATH      the installed .esriAddinX
    MCP_INSTALLED_JSON  the bookkeeping file
    MCP_PACKAGE_ROOT    the source tree this build came from
    MCP_INSTALL_NOTE    default note when the third argument is omitted
"""
import hashlib
import json
import os
import sys
import time

ADDIN = os.environ.get("MCP_ADDIN_PATH") or os.path.join(
    os.path.expanduser("~"), "Documents", "ArcGIS", "AddIns", "ArcGISPro",
    "ArcGISProMCP.esriAddinX")
INSTALLED = os.environ.get("MCP_INSTALLED_JSON") or os.path.join(
    os.environ["LOCALAPPDATA"], "ArcGIS Pro MCP", "installed.json")
# The source tree this script lives in (source/scripts -> source/).
PACKAGE_ROOT = os.environ.get("MCP_PACKAGE_ROOT") or os.path.dirname(
    os.path.dirname(os.path.abspath(__file__)))


def sha(path):
    digest = hashlib.sha256()
    with open(path, "rb") as handle:
        for chunk in iter(lambda: handle.read(1 << 20), b""):
            digest.update(chunk)
    return digest.hexdigest().upper()


def main(argv):
    if len(argv) < 3:
        raise SystemExit(
            "usage: update_installed.py <version> <expected_sha256> [note]")
    version = argv[1]
    expected = argv[2].upper()
    note = argv[3] if len(argv) > 3 else os.environ.get(
        "MCP_INSTALL_NOTE", "本地升级 " + version)

    actual = sha(ADDIN)
    if actual != expected:
        raise RuntimeError(
            "installed add-in hash mismatch: " + actual
            + " (expected " + expected + ")")

    with open(INSTALLED, encoding="utf-8") as handle:
        data = json.load(handle)
    data["version"] = version
    data["installed_at"] = time.strftime("%Y-%m-%dT%H:%M:%S%z")
    data["addin_sha256"] = actual
    data["addin_size"] = os.path.getsize(ADDIN)
    data["package_root"] = PACKAGE_ROOT
    data["note"] = note
    with open(INSTALLED, "w", encoding="utf-8") as handle:
        json.dump(data, handle, ensure_ascii=False, indent=1)
    print(json.dumps(data, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    main(sys.argv)
