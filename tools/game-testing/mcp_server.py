"""Project-scoped MCP surface, backed by exactly the same client as regression tests."""
from pathlib import Path
import json
import subprocess
import sys
from mcp.server.fastmcp import FastMCP, Image
from client import HERE, ROOT, SESSION_FILE, GameClient, control_lock, read_json
import session
import tasks

mcp = FastMCP("HeavySilent game testing", instructions=(
    "Operate only the managed profile-3 HeavySilent session. Read state after actions; accepted is not completed. "
    "Choose checks from the current change, record intent with begin_test_task, arrange ordered piles and powers with "
    "prepare_scenario, play through normal actions, inspect screenshots, and record_observation with reasoning. "
    "Add investigation checks when evidence surprises you. Fixed regression suites are optional, not a default ritual."))


@mcp.tool()
def begin_test_task(title: str, change_summary: str, checks: list[dict]) -> dict:
    """Record the current change and chosen checks. Each check has id, question, expected, reason (all strings)."""
    with control_lock():
        return tasks.begin(GameClient(), title, change_summary, checks)


@mcp.tool()
def add_test_check(check: dict) -> dict:
    """Add a new investigation to the active task: id, question, expected, reason."""
    with control_lock():
        return tasks.add_check(GameClient(), check)


@mcp.tool()
def record_observation(check_id: str, verdict: str, observed: str, reasoning: str, evidence: list[str] | None = None) -> dict:
    """Record passed/failed/blocked with actual observations and reasoning; captures state and links session evidence files."""
    with control_lock():
        return tasks.observe(GameClient(), check_id, verdict, observed, reasoning, evidence or [])


@mcp.tool()
def finish_test_task(summary: str) -> dict:
    """Produce JSON and Chinese Markdown review with findings, evidence, and explicitly untested checks."""
    with control_lock():
        return tasks.finish(GameClient(), summary)


@mcp.tool()
def start_combat(seed: str = "HEAVYSILENTTEST01", fresh: bool = False) -> dict:
    """Reach a playable combat through normal menus. Reuse active combat unless fresh=True; resets only this test run."""
    from runner import RegressionClient, enter_run
    with control_lock():
        client = RegressionClient()
        state = client.wait()
        if state['in_combat'] and not fresh:
            return client.wait(lambda s: s['play_phase'] and not s['busy'])
        return enter_run(client, 'agent', seed=seed, upgrade_deck=False)


@mcp.tool()
def list_models(kind: str, query: str = "") -> dict:
    """Find registered card/power IDs: kind='cards' or 'powers', optional ID substring (up to 100 results)."""
    return GameClient().request('/hs/catalog', {'kind': kind, 'query': query})


@mcp.tool()
def prepare_scenario(scenario: dict) -> dict:
    """Replace combat piles. hand/draw/discard/exhaust: lists of {id, upgraded?}; draw is TOP FIRST.
    player: {energy?, hp?, max_hp?, block?, powers?:[{id,amount}]}.
    enemies: [{index, hp?, max_hp?, block?, powers?:[{id,amount}]}].
    Unspecified piles become empty. Clears powers unless clear_powers=false, resets energy to max.
    Keeps permanent deck, relics, enemy identities and unspecified HP/block. Does NOT play cards.
    """
    with control_lock():
        client = GameClient()
        client.request('/hs/scenario', scenario)
        return client.wait(lambda s: not s['busy'] and s['play_phase'] and s['visible_hand_count'] == len(s['piles']['hand']))


@mcp.tool()
def inspect_card(index: int = 0, close: bool = False) -> dict:
    """Open/close the real enlarged card view for screenshot review; index refers to current hand."""
    with control_lock():
        return GameClient().request('/hs/inspect-card', {'index': index, 'close': close})


@mcp.tool()
def start_session(configuration: str = "Debug", language: str | None = None) -> dict:
    """Build, deploy and open a visible session; optional language eng/zhs is set before startup. Fails if a game runs."""
    if configuration not in ("Debug", "Release"):
        raise ValueError("Expected Debug or Release")
    with control_lock():
        data = session.start(configuration, language=language)
    return {"id": data["id"], "status": data["status"], "directory": data["directory"]}


@mcp.tool()
def get_state() -> dict:
    """Read live game/UI state and the actual combat piles; does not advance gameplay."""
    return GameClient().state()


@mcp.tool()
def perform_action(action: str, arguments: dict | None = None) -> dict:
    """Use normal game actions: menu_select, play_card, end_turn, choose_map_node, event choices, etc."""
    with control_lock():
        return GameClient().action(action, **(arguments or {}))


@mcp.tool()
def prepare_case(case_id: str) -> dict:
    """Optional existing regression fixture; for change-specific tests prefer prepare_scenario."""
    with control_lock():
        return GameClient().prepare(case_id)


@mcp.tool()
def capture_screenshot(name: str = "agent-screenshot") -> Image:
    """Capture the real game viewport, save the original PNG, and return it for visual inspection."""
    path = GameClient().screenshot(name)
    return Image(path=str(path))


@mcp.tool()
def run_suite(suite: str = "regression") -> dict:
    """Start a smoke/regression job on the active session; poll get_report for completion."""
    if suite not in ("smoke", "regression"):
        raise ValueError("Expected smoke or regression")
    data = read_json(SESSION_FILE)
    log = Path(data["directory"]) / "runner.log"
    with log.open("a", encoding="utf-8") as output:
        process = subprocess.Popen([sys.executable, str(HERE / "runner.py"), "--suite", suite, "--attach", "--keep-open"],
            cwd=ROOT, stdout=output, stderr=subprocess.STDOUT, creationflags=subprocess.CREATE_NO_WINDOW)
    return {"status": "started", "pid": process.pid, "run_id": data["id"], "log": str(log)}


@mcp.tool()
def get_report() -> dict:
    """Read the active session's report, or the last completed report after cleanup."""
    from client import ARTIFACTS
    pointer = SESSION_FILE if SESSION_FILE.exists() else ARTIFACTS / "latest.json"
    directory = Path(read_json(pointer)["directory"])
    path = directory / "report.json"
    result = {'directory': str(directory)}
    if (directory / 'active-task.json').exists():
        result['agent_task'] = read_json(directory / read_json(directory / 'active-task.json')['path'])
    if path.exists():
        result['optional_regression'] = read_json(path)
    return result


@mcp.tool()
def stop_session() -> dict:
    """Close this workflow's game and restore the original active profile and window settings."""
    with control_lock():
        return session.stop()


if __name__ == "__main__":
    mcp.run(transport="stdio")
