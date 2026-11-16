from arcgis_agentkit.distill.cli import main as distill_main
from arcgis_agentkit.guard.cli import main as guard_main


def test_guard_doctor():
    assert guard_main(["doctor"]) == 0


def test_guard_planned_command_is_not_implemented_yet():
    assert guard_main(["summon"]) == 3


def test_distill_doctor():
    assert distill_main(["doctor"]) == 0


def test_distill_planned_command_is_not_implemented_yet():
    assert distill_main(["measure"]) == 3
