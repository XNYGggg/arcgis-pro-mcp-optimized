"""The MCP catalog and the bridge handlers must stay in step.

The bridge imports arcpy, so it cannot be imported here; the registered
command names are read from the source with the ast module instead.
"""

import ast
import json
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "src"))

from arcgis_pro_mcp.catalog import BY_NAME, CATALOG  # noqa: E402

# Commands that exist on the bridge but are deliberately not exposed as tools.
INTERNAL_ONLY = {"get_last_traceback"}

# Tools the add-in answers in C# itself with no Python handler by design --
# restarting ArcGIS Pro only makes sense from inside it, and the template_*
# tools are thin wrappers over the arcgis-distill CLI (Commands/TemplateCommands.cs).
CSHARP_ONLY = {"restart_pro"} | {
    "template_list", "template_get", "template_import", "template_measure",
    "template_patch", "template_validate", "template_review",
    "template_approve", "template_preview", "template_apply",
}


def bridge_commands():
    names = set()
    for path in (ROOT / "arcgis_pro_plugin" / "arcgis_mcp").glob("*.py"):
        tree = ast.parse(path.read_text(encoding="utf-8"))
        for node in ast.walk(tree):
            if not isinstance(node, ast.FunctionDef):
                continue
            for decorator in node.decorator_list:
                if isinstance(decorator, ast.Call) and \
                        getattr(decorator.func, "id", None) == "command":
                    names.add(decorator.args[0].value)
    return names


def test_every_tool_has_a_handler():
    missing = sorted(set(BY_NAME) - bridge_commands() - CSHARP_ONLY)
    assert not missing, f"Tools with no bridge handler: {missing}"


def test_tools_json_matches_catalog():
    """The add-in embeds Resources/tools.json generated from this catalog.

    A stale copy would silently hide or mis-describe tools on the HTTP
    transport while the Python server advertises the new ones, so the whole
    entry -- name, description, inputSchema, returnsImage -- must match what
    scripts/export_tool_schemas.py would generate today.
    """
    import importlib.util

    spec = importlib.util.spec_from_file_location(
        "export_tool_schemas", ROOT / "scripts" / "export_tool_schemas.py")
    exporter = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(exporter)
    expected = {
        tool.name: {
            "name": tool.name,
            "description": tool.description,
            "inputSchema": exporter.schema_for(tool),
            "returnsImage": tool.returns_image,
        }
        for tool in CATALOG
    }
    tools_file = ROOT / "addin" / "ArcGISProMCP" / "Resources" / "tools.json"
    data = json.loads(tools_file.read_text(encoding="utf-8-sig"))
    tools = data["tools"] if isinstance(data, dict) else data
    actual = {entry["name"]: entry for entry in tools}
    drifted = sorted(name for name in expected
                     if actual.get(name) != expected[name])
    assert actual == expected, (
        "tools.json differs from what export_tool_schemas.py generates: "
        f"drifted={drifted} missing={sorted(set(expected) - set(actual))} "
        f"extra={sorted(set(actual) - set(expected))}")


def test_every_handler_is_exposed():
    unexposed = sorted(bridge_commands() - set(BY_NAME) - INTERNAL_ONLY)
    assert not unexposed, f"Bridge commands missing from the catalog: {unexposed}"


if __name__ == "__main__":
    test_every_tool_has_a_handler()
    test_every_handler_is_exposed()
    print(f"OK -- {len(BY_NAME)} tools, {len(bridge_commands())} bridge commands")


# Tuning knobs read by a handler but deliberately not exposed as tool inputs.
INTERNAL_PARAMS = {"max_workspaces"}


def handler_param_keys():
    """Every params[...] / params.get(...) key, per command."""
    used = {}
    for path in (ROOT / "arcgis_pro_plugin" / "arcgis_mcp").glob("*.py"):
        tree = ast.parse(path.read_text(encoding="utf-8"))
        for node in tree.body:
            if not isinstance(node, ast.FunctionDef):
                continue
            command = None
            for decorator in node.decorator_list:
                if isinstance(decorator, ast.Call) and \
                        getattr(decorator.func, "id", None) == "command":
                    command = decorator.args[0].value
            if command is None:
                continue
            keys = set()
            for sub in ast.walk(node):
                if isinstance(sub, ast.Subscript) and \
                        getattr(sub.value, "id", None) == "params" and \
                        isinstance(sub.slice, ast.Constant):
                    keys.add(sub.slice.value)
                if isinstance(sub, ast.Call) and \
                        isinstance(sub.func, ast.Attribute) and \
                        sub.func.attr == "get" and \
                        getattr(sub.func.value, "id", None) == "params" and \
                        sub.args and isinstance(sub.args[0], ast.Constant):
                    keys.add(sub.args[0].value)
            used[command] = keys
    return used


def test_handlers_only_read_declared_params():
    problems = []
    for command, keys in handler_param_keys().items():
        tool = BY_NAME.get(command)
        if tool is None:
            continue
        declared = {p.name for p in tool.params} | INTERNAL_PARAMS
        missing = sorted(keys - declared)
        if missing:
            problems.append(f"{command}: {missing}")
    assert not problems, "Handler reads parameters the catalog does not offer: " \
        + "; ".join(problems)
