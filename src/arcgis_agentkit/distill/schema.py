"""A tiny, dependency-free JSON Schema validator for the template schema.

Only the subset used by ``library/schema.json`` is implemented: type,
required, enum, properties, items, minimum/maximum, pattern and anyOf.
Unknown keywords are ignored so the schema can grow without this file
rejecting documents for the wrong reason.
"""

from __future__ import annotations

import re
from typing import Any

TYPES = {
    "object": dict,
    "array": list,
    "string": str,
    "number": (int, float),
    "integer": int,
    "boolean": bool,
    "null": type(None),
}


def _type_ok(value: Any, name: str) -> bool:
    expected = TYPES.get(name)
    if expected is None:
        return True
    if name in ("number", "integer") and isinstance(value, bool):
        return False
    return isinstance(value, expected)


def validate(instance: Any, schema: dict[str, Any], path: str = "$") -> list[dict[str, str]]:
    issues: list[dict[str, str]] = []

    def add(code: str, message: str, where: str = path) -> None:
        issues.append({"path": where, "code": code, "message": message})

    if not isinstance(schema, dict):
        return issues

    if "type" in schema:
        names = schema["type"] if isinstance(schema["type"], list) else [schema["type"]]
        if not any(_type_ok(instance, name) for name in names):
            add("type", f"expected {'/'.join(names)}, got {type(instance).__name__}")
            return issues

    if "enum" in schema and instance not in schema["enum"]:
        add("enum", f"value {instance!r} is not one of {schema['enum']}")

    if isinstance(instance, dict):
        for key in schema.get("required", []):
            if key not in instance:
                add("required", f"missing required property '{key}'", f"{path}.{key}")
        for key, child in schema.get("properties", {}).items():
            if key in instance:
                issues.extend(validate(instance[key], child, f"{path}.{key}"))

    if isinstance(instance, list):
        item_schema = schema.get("items")
        if isinstance(item_schema, dict):
            for index, item in enumerate(instance):
                issues.extend(validate(item, item_schema, f"{path}[{index}]"))

    if isinstance(instance, (int, float)) and not isinstance(instance, bool):
        if "minimum" in schema and instance < schema["minimum"]:
            add("minimum", f"value {instance} is below {schema['minimum']}")
        if "maximum" in schema and instance > schema["maximum"]:
            add("maximum", f"value {instance} is above {schema['maximum']}")

    if isinstance(instance, str) and "pattern" in schema:
        if not re.search(schema["pattern"], instance):
            add("pattern", f"value {instance!r} does not match {schema['pattern']}")

    if "anyOf" in schema:
        if not any(not validate(instance, option, path)
                   for option in schema["anyOf"]):
            add("anyOf", "value does not match any allowed schema")

    return issues
