"""Connection retry policy: only read-only commands reconnect automatically.

A write may have reached ArcGIS Pro before the socket dropped, so resending it
could duplicate the edit. The client must fail closed for writes.
"""
import pathlib
import sys
import unittest

ROOT = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "src"))

from arcgis_pro_mcp.connection import ArcGISConnection, ArcGISProNotAvailable


class FlakyConnection(ArcGISConnection):
    """First call raises a stale-socket error, later calls succeed."""

    def __init__(self):
        super().__init__(timeout=1)
        self.calls = 0

    def _send_once(self, payload):
        self.calls += 1
        if self.calls == 1:
            raise ConnectionError("stale socket")
        return {"success": True, "command": payload["command"]}


class RetryPolicyTests(unittest.TestCase):
    def test_read_command_retries_once(self):
        conn = FlakyConnection()
        result = conn.send_command("get_project_info")
        self.assertEqual(2, conn.calls)
        self.assertTrue(result["success"])

    def test_write_command_not_retried(self):
        conn = FlakyConnection()
        with self.assertRaises(ArcGISProNotAvailable):
            conn.send_command("set_layer_renderer")
        self.assertEqual(1, conn.calls)

    def test_diagnose_is_treated_as_read(self):
        conn = FlakyConnection()
        result = conn.send_command("diagnose")
        self.assertEqual(2, conn.calls)
        self.assertTrue(result["success"])


if __name__ == "__main__":
    unittest.main()
