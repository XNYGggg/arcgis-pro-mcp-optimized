# -*- coding: utf-8 -*-
"""Generic code runner for the ArcGIS Pro MCP bridge.

Run a .py file inside ArcGIS Pro's geoprocessing Python context -- the same
context in which arcpy.mp.ArcGISProject("CURRENT") resolves -- and write the
captured stdout/stderr plus the final expression to a .result.json file next
to it.

Call through the add-in's run_python_toolbox_tool:

    toolbox_path = ...\\RunCode.pyt
    tool_name    = RunCode
    args         = [r"E:\\path\\to\\script.py"]
"""

import json
import os
import sys
import traceback

import arcpy

RESULT_SUFFIX = ".result.json"


class Toolbox(object):
    def __init__(self):
        self.label = "ArcGIS MCP Runtime"
        self.alias = "arcgismcpruntime"
        self.tools = [RunCode]


class RunCode(object):
    def __init__(self):
        self.label = "RunCode"
        self.description = "Execute a Python file in the ArcGIS Pro Python context."
        self.canRunInBackground = False

    def getParameterInfo(self):
        code_file = arcpy.Parameter(
            displayName="Python file",
            name="code_file",
            datatype="DEFile",
            parameterType="Required",
            direction="Input",
        )
        return [code_file]

    def isLicensed(self):
        return True

    def updateParameters(self, parameters):
        return

    def updateMessages(self, parameters):
        return

    def execute(self, parameters, messages):
        code_file = parameters[0].valueAsText
        if not code_file:
            raise ValueError("code_file is required")
        code_file = os.path.abspath(code_file)
        result_path = code_file + RESULT_SUFFIX

        result = {"code_file": code_file, "pid": os.getpid(), "ok": False}
        namespace = {"arcpy": arcpy, "__name__": "__mcprun__", "__file__": code_file}

        import io
        import contextlib
        buffer = io.StringIO()
        try:
            with open(code_file, "r", encoding="utf-8-sig") as handle:
                source = handle.read()
            tree = compile(source, code_file, "exec")
            with contextlib.redirect_stdout(buffer), contextlib.redirect_stderr(buffer):
                exec(tree, namespace)
            result["ok"] = True
        except Exception as exc:
            result["error"] = "{}: {}".format(type(exc).__name__, exc)
            result["traceback"] = traceback.format_exc()
        finally:
            result["output"] = buffer.getvalue()
            try:
                with open(result_path, "w", encoding="utf-8") as handle:
                    json.dump(result, handle, ensure_ascii=False, indent=2)
            except Exception:
                pass
            try:
                if result["output"]:
                    messages.addMessage("output:\n" + result["output"][-4000:])
            except Exception:
                pass
            if not result["ok"]:
                try:
                    messages.addWarningMessage(result.get("error", "RunCode failed"))
                    trace = result.get("traceback") or ""
                    if trace:
                        messages.addMessage("traceback:\n" + trace[-3000:])
                except Exception:
                    pass
            messages.addMessage("RunCode: " + ("ok" if result["ok"] else "failed")
                                + " -- " + result_path)
