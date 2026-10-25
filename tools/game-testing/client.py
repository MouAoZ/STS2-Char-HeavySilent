"""Shared local transport and observable action completion for MCP and regression runs."""
from __future__ import annotations
import base64
from contextlib import contextmanager
from datetime import datetime, timezone
import hashlib
import json
import msvcrt
from pathlib import Path
import subprocess
import time
import urllib.error
import urllib.request

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]


def shared_artifacts(root: Path) -> Path:
    """Keep the one game session, ownership record and lock outside disposable worktrees."""
    if not (root / ".git").exists():
        return root / "artifacts/game-tests"
    result = subprocess.run(
        ["git", "-C", str(root), "worktree", "list", "--porcelain", "-z"],
        capture_output=True, check=True, encoding="utf-8",
    )
    primary = result.stdout.split("\0", 1)[0]
    if not primary.startswith("worktree "):
        raise RuntimeError("Cannot locate the primary worktree for shared game-test state.")
    return Path(primary.removeprefix("worktree ")) / "artifacts/game-tests"


ARTIFACTS = shared_artifacts(ROOT)
SESSION_FILE = ARTIFACTS / "active-session.json"


def read_json(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def write_json(path, data):
    path = Path(path)
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = path.with_suffix(path.suffix + ".tmp")
    temporary.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")
    temporary.replace(path)


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


@contextmanager
def control_lock():
    """An OS lock releases after crashes; reads and screenshots need no ownership."""
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    with (ARTIFACTS / "controller.lock").open("a+b") as stream:
        if stream.tell() == 0:
            stream.write(b"0"); stream.flush()
        stream.seek(0)
        try:
            msvcrt.locking(stream.fileno(), msvcrt.LK_NBLCK, 1)
        except OSError as error:
            raise RuntimeError("Another regression run or Agent action owns the game controls.") from error
        try:
            yield
        finally:
            stream.seek(0)
            msvcrt.locking(stream.fileno(), msvcrt.LK_UNLCK, 1)


class GameClient:
    def __init__(self, session=None):
        self.session = session or read_json(SESSION_FILE)
        self.directory = Path(self.session["directory"])
        self.opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))

    def request(self, path, body=None, timeout=30):
        request = urllib.request.Request("http://127.0.0.1:15526" + path,
            data=None if body is None else json.dumps(body).encode(),
            headers={"Content-Type": "application/json", "X-HeavySilent-Session": self.session["token"]})
        record = {"time": datetime.now(timezone.utc).isoformat(), "path": path, "request": body}
        try:
            with self.opener.open(request, timeout=timeout) as response:
                result = json.load(response)
            record["response"] = {k: v for k, v in result.items() if k != "png_base64"}
            if result.get("status") == "error" or "error" in result:
                raise RuntimeError(str(result))
            return result
        except urllib.error.HTTPError as error:
            detail = error.read().decode(errors="replace")
            record["error"] = detail
            raise RuntimeError(f"{path}: HTTP {error.code}: {detail}") from error
        except Exception as error:
            record["error"] = str(error)
            raise
        finally:
            self.directory.mkdir(parents=True, exist_ok=True)
            with (self.directory / "actions.jsonl").open("a", encoding="utf-8") as stream:
                stream.write(json.dumps(record, ensure_ascii=False) + "\n")

    def state(self):
        return self.request("/hs/state") | {
            "source_root": self.session.get("source_root"),
            "controller_root": str(ROOT),
        }

    def wait(self, predicate=lambda state: not state["busy"], timeout=30):
        deadline = time.monotonic() + timeout
        stable = 0
        last = None
        while time.monotonic() < deadline:
            last = self.state()
            stable = stable + 1 if predicate(last) else 0
            if stable >= 3:
                return last
            time.sleep(.2)
        raise TimeoutError(f"Game did not settle within {timeout}s; last UI={last.get('ui') if last else None}")

    def action(self, action, **arguments):
        self.request("/api/v1/singleplayer", {"action": action, **arguments})
        time.sleep(.3)  # Give an enqueued UI action its first frame; then observe actual state.
        # A live action can deliberately await a player choice. Returning that stable
        # screen lets the Agent answer it; it is not a claim that the effect completed.
        return self.wait(lambda state: not state['busy'] or state.get('ui', {}).get('state_type') in
                         {'card_select', 'hand_select', 'bundle_select', 'relic_select', 'card_reward', 'rewards'})

    def prepare(self, case_id):
        self.request("/hs/prepare", {"case_id": case_id})
        return self.wait(lambda s: not s["busy"] and s["play_phase"])

    def screenshot(self, name="screenshot"):
        if not name.replace("-", "").replace("_", "").isalnum():
            raise ValueError("Screenshot name must contain only letters, digits, '-' and '_'.")
        result = self.request('/hs/screenshot')
        png = base64.b64decode(result['png_base64'], validate=True)
        path = self.directory / f"{name}.png"
        path.write_bytes(png)
        with (self.directory / 'actions.jsonl').open('a', encoding='utf-8') as stream:
            stream.write(json.dumps({'time': datetime.now(timezone.utc).isoformat(),
                'screenshot': path.name, 'backend': 'godot_viewport',
                'width': result['width'], 'height': result['height']}) + '\n')
        return path
