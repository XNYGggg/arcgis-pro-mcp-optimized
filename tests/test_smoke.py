from arcgis_agentkit.distill.cli import main as distill_main
from arcgis_agentkit.guard.cli import main as guard_main


def test_guard_doctor():
    assert guard_main(["doctor", "--json"]) == 0


def test_distill_doctor():
    assert distill_main(["doctor", "--json"]) == 0


def test_guard_engine_and_rules_are_packaged():
    from arcgis_agentkit.guard.config import PACKAGE_ENGINE, PACKAGE_RULES

    assert PACKAGE_ENGINE.is_file()
    assert PACKAGE_RULES.is_file()
