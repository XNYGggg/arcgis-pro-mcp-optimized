"""Dump every geoprocessing tool's parameter order and details for the add-in.

The order file is what lets the commands accept named parameters -- arcpy
knows the parameter names, the .NET API does not. The details file adds
datatype, ValueList enumerations and short defaults, so
describe_geoprocessing_tool can answer without probing a tool by trial.

Run with ArcGIS Pro's Python:

    "C:\\Program Files\\ArcGIS\\Pro\\bin\\Python\\envs\\arcgispy3\\python.exe" \\
        scripts/dump_gp_parameters.py

(The file is already written when arcpy occasionally segfaults on shutdown,
so a non-zero exit code here is not a failure.)
"""

import datetime
import json
import pathlib
import sys

RESOURCES = (pathlib.Path(__file__).resolve().parents[1]
             / "addin" / "ArcGISProMCP" / "Resources")
OUTPUT = RESOURCES / "gp-parameters.json"
DETAILS = RESOURCES / "gp-parameter-details.json"
DETAIL_BUDGET = 2 * 1024 * 1024  # bytes; defaults go first when over

_arcpy = None


def short_default(parameter):
    """A default worth storing: short, plain text, no paths or XML blobs."""
    try:
        text = parameter.valueAsText
    except Exception:
        return None
    if not text:
        return None
    text = str(text).strip()
    if not text or len(text) > 60:
        return None
    if "\\" in text or "/" in text or text.startswith("<"):
        return None
    return text


def details_for(tool):
    """Rows worth carrying: only parameters with an enum or a short default."""
    rows = []
    for parameter in _arcpy.GetParameterInfo(tool):
        row = {}
        try:
            value_filter = parameter.filter
            if value_filter is not None:
                value_list = getattr(value_filter, "list", None)
                if value_list:
                    items = [str(item) for item in value_list]
                    if 0 < len(items) <= 50:
                        row["filter"] = {"type": "ValueList", "list": items}
        except Exception:
            pass
        default = short_default(parameter)
        if default is not None:
            row["default"] = default
        if not row:
            continue
        row = {"name": parameter.name, **row}
        try:
            if parameter.datatype:
                row["datatype"] = str(parameter.datatype)
        except Exception:
            pass
        rows.append(row)
    return rows


def main() -> int:
    global _arcpy
    try:
        import arcpy
    except ImportError:
        print("This must run with ArcGIS Pro's Python (arcpy is required).")
        return 1
    _arcpy = arcpy

    tools = sorted(arcpy.ListTools("*"))
    print(f"{len(tools)} tools reported by arcpy")

    catalogue = {}
    details = {}
    skipped = 0
    for tool in tools:
        try:
            parameters = arcpy.GetParameterInfo(tool)
        except Exception:
            skipped += 1
            continue
        names = [p.name for p in parameters]
        if not names:
            continue
        # "Buffer_analysis" -> "analysis.Buffer", the form tools are called by
        # everywhere else in this project. Both forms go into the order file.
        if "_" in tool:
            name, _, alias = tool.rpartition("_")
            catalogue[f"{alias}.{name}"] = names
        catalogue[tool] = names

        rows = details_for(tool)
        if not rows:
            continue
        # One canonical key per tool keeps the resource small; the add-in
        # flips between the two forms when looking a tool up.
        if "_" in tool:
            name, _, alias = tool.rpartition("_")
            details[f"{alias}.{name}"] = {"parameters": rows}
        else:
            details[tool] = {"parameters": rows}

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(
        json.dumps(catalogue, separators=(",", ":"), sort_keys=True),
        encoding="utf-8")

    try:
        pro_version = arcpy.GetInstallInfo().get("Version")
    except Exception:
        pro_version = None

    payload = {
        "schema_version": 1,
        "generated_at": datetime.datetime.now(
            datetime.timezone.utc).isoformat(timespec="seconds"),
        "arcgis_pro_version": pro_version,
        "arcpy_version": getattr(arcpy, "__version__", None),
        "tools": details,
    }
    text = json.dumps(payload, separators=(",", ":"), sort_keys=True)
    if len(text.encode("utf-8")) > DETAIL_BUDGET:
        print("details over budget -- dropping defaults")
        for entry in details.values():
            for row in entry["parameters"]:
                row.pop("default", None)
        text = json.dumps(payload, separators=(",", ":"), sort_keys=True)
    if len(text.encode("utf-8")) > DETAIL_BUDGET:
        print("details still over budget -- trimming long enumerations")
        for entry in details.values():
            for row in entry["parameters"]:
                value_filter = row.get("filter")
                if value_filter and len(value_filter["list"]) > 20:
                    value_filter["list"] = value_filter["list"][:20]
        text = json.dumps(payload, separators=(",", ":"), sort_keys=True)
    if len(text.encode("utf-8")) > DETAIL_BUDGET:
        print("details still over budget -- dropping enumerations")
        for entry in details.values():
            for row in entry["parameters"]:
                row.pop("filter", None)
        text = json.dumps(payload, separators=(",", ":"), sort_keys=True)
    DETAILS.write_text(text, encoding="utf-8")

    order_kb = OUTPUT.stat().st_size / 1024
    detail_mb = DETAILS.stat().st_size / (1024 * 1024)
    print(f"{len(catalogue)} order entries -> {OUTPUT} ({order_kb:.0f} KB)")
    print(f"{len(details)} detail entries -> {DETAILS} ({detail_mb:.2f} MB)")
    print("arcpy often segfaults as it shuts down after this; the files above "
          "are already written, so a non-zero exit code here is not a failure.")
    if skipped:
        print(f"{skipped} tools could not be inspected and were left out")
    return 0


if __name__ == "__main__":
    sys.exit(main())
