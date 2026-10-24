"""Own one visible game process and profile 3; keep personal save files unchanged."""
from __future__ import annotations
import argparse
import base64
import copy
from datetime import datetime, timezone
import json
import os
from pathlib import Path
import secrets
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET
import zipfile
from client import ARTIFACTS, HERE, ROOT, SESSION_FILE, GameClient, control_lock, digest, read_json, write_json

PWSH = Path(r"C:\Program Files\PowerShell\7\pwsh.exe")
ALLOWED_MODS = {"BaseLib", "HeavySilent", "godotexplorer", "HeavySilentTestBridge"}


def settings(config=None):
    root = ET.parse(config or ROOT / "mod/local.props").getroot()
    return {name: Path(root.findtext(f".//{name}")) for name in ("Sts2Path", "GodotPath", "BaseLibPath")}


def run_dev(action, configuration="Debug", config=None):
    args = [str(PWSH), "-NoProfile", "-File", str(ROOT / "scripts/dev.ps1"), action, "-Configuration", configuration]
    if config:
        args += ["-LocalConfig", str(config)]
    subprocess.run(args, cwd=ROOT, check=True, stdout=sys.stderr)


def game_pids():
    result = subprocess.run([str(PWSH), "-NoProfile", "-Command",
        "@(Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue).Id | ConvertTo-Json -Compress"],
        capture_output=True, text=True, check=False)
    value = json.loads(result.stdout) if result.stdout.strip() else []
    return [] if value is None else value if isinstance(value, list) else [value]


def personal_hashes(account, include_unmodded=True):
    prefixes = (account, account / "modded") if include_unmodded else (account / "modded",)
    paths = [p for prefix in prefixes for slot in (1, 2)
             for p in (prefix / f"profile{slot}").rglob("*") if p.is_file()]
    return {str(p.relative_to(account)): digest(p) for p in sorted(paths)}


def find_account():
    root = Path(os.environ["APPDATA"]) / "SlayTheSpire2/steam"
    candidates = [p for p in root.iterdir() if (p / "settings.save").is_file()]
    if len(candidates) != 1:
        raise RuntimeError("Expected one local Steam account save directory; select the account explicitly before testing.")
    return candidates[0]


def install_bridge(config=None):
    if game_pids():
        raise RuntimeError("Close the existing game before installing the test bridge.")
    game = settings(config)["Sts2Path"]
    destination = game / "mods/HeavySilentTestBridge"
    destination.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT / "artifacts/test-bridge/HeavySilentTestBridge.dll", destination)
    shutil.copy2(HERE / "bridge/HeavySilentTestBridge.json", destination)
    shutil.copy2(ROOT / ".tools/sts2mcp/src/LICENSE", destination / "LICENSE.txt")
    write_json(destination / "STS2_MCP.conf", {"port": 15526})


def start(configuration="Debug", config=None, language=None):
    if language not in (None, 'eng', 'zhs'):
        raise ValueError('Test language must be eng or zhs')
    if game_pids():
        raise RuntimeError("A game instance already exists. Use test-stop for an owned session, or close your game first.")
    if SESSION_FILE.exists():
        raise RuntimeError("An unfinished session record exists. Run test-stop to recover it first.")
    account = find_account()
    slot = account / "modded/profile3"
    ownership = ARTIFACTS / "profile3-owner.json"
    has_data = any(p.is_file() for p in slot.rglob("*")) if slot.exists() else False
    if has_data and not ownership.exists():
        raise RuntimeError("Profile 3 contains unregistered data; it will not be overwritten.")
    if ownership.exists() and read_json(ownership)["account"] != str(account):
        raise RuntimeError("The registered test profile belongs to a different Steam account.")
    original_settings = read_json(account / "settings.save")
    enabled = {m["id"] for m in original_settings["mod_settings"]["mod_list"] if m["is_enabled"]}
    if enabled - ALLOWED_MODS:
        raise RuntimeError(f"Unexpected enabled mods: {sorted(enabled - ALLOWED_MODS)}")
    if not {"BaseLib", "HeavySilent"}.issubset(enabled):
        raise RuntimeError("Enable BaseLib and HeavySilent before testing.")
    # Build/deploy already provide hash and game-version gates; reuse that workflow.
    run_dev("package", configuration, config)
    run_dev("deploy", configuration, config)
    bridge_args = [sys.executable, str(HERE / "bootstrap.py"), "--bridge-only"]
    if config:
        bridge_args += ["--local-config", str(config)]
    subprocess.run(bridge_args, cwd=ROOT, check=True, stdout=sys.stderr)
    install_bridge(config)
    run_id = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + secrets.token_hex(3)
    directory = ARTIFACTS / run_id
    directory.mkdir(parents=True)
    selector = account / "modded/profile.save"
    selector_bytes = selector.read_bytes() if selector.exists() else None
    original_profile = read_json(selector)["last_profile_id"] if selector_bytes else 1
    data = {
        "id": run_id, "directory": str(directory), "token": secrets.token_hex(24), "pid": None,
        "account": str(account), "original_profile": original_profile,
        "selector_backup": base64.b64encode(selector_bytes).decode() if selector_bytes else None,
        "original_settings": copy.deepcopy(original_settings), "personal_hashes": personal_hashes(account),
        "hash_scope": "modded_and_unmodded",
        "package": read_json(ROOT / "artifacts/package/package-info.json"),
        "bridge_sha256": digest(ROOT / "artifacts/test-bridge/HeavySilentTestBridge.dll"),
        "configuration": configuration, "config": str(config) if config else None,
        "visual_review": "pending", "status": "starting"
    }
    with zipfile.ZipFile(directory / 'personal-saves-before.zip', 'w', zipfile.ZIP_DEFLATED) as backup:
        for relative in data['personal_hashes']:
            backup.write(account / relative, relative)
    data['personal_backup'] = str(directory / 'personal-saves-before.zip')
    write_json(SESSION_FILE, data)
    write_json(directory / "session.json", data)
    write_json(ownership, {"account": str(account), "created_by": "HeavySilent game testing"})
    # Select before startup: loading a personal profile can itself update its progress file.
    selected = read_json(selector) if selector_bytes else {"schema_version": 2}
    selected["last_profile_id"] = 3
    write_json(selector, selected)
    mods = original_settings["mod_settings"]["mod_list"]
    bridge = next((m for m in mods if m["id"] == "HeavySilentTestBridge"), None)
    if bridge:
        bridge["is_enabled"] = True
    else:
        mods.append({"id": "HeavySilentTestBridge", "is_enabled": True, "source": "mods_directory"})
    # The game's settings override the CLI window flag during startup.
    original_settings['fullscreen'] = False
    if language:
        original_settings['language'] = language
    write_json(account / "settings.save", original_settings)
    game = settings(config)["Sts2Path"]
    env = os.environ | {"SteamAppId": "2868840", "HEAVYSILENT_TEST_SESSION": data["token"],
                        "HEAVYSILENT_ORIGINAL_PROFILE": str(original_profile)}
    process = subprocess.Popen([str(game / "SlayTheSpire2.exe"), "--windowed", "--resolution", "1280x720",
                                "--log-file", str(directory / "game.log")], cwd=game, env=env)
    data["pid"] = process.pid
    write_json(SESSION_FILE, data)
    write_json(directory / "session.json", data)
    client = GameClient(data)
    deadline = time.monotonic() + 120
    while time.monotonic() < deadline:
        if process.poll() is not None:
            raise RuntimeError(f"Game exited during startup ({process.returncode}); see {directory / 'game.log'}")
        try:
            health = client.request("/hs/health", timeout=3)
        except (OSError, RuntimeError):
            time.sleep(1); continue
        if health["pid"] != process.pid or health["session"] != data["token"]:
            raise RuntimeError("The bridge belongs to a different game session.")
        if health["ready"]:
            if not health.get('local_saves') or health["profile"] != 3 or health["bridge_sha256"] != data["bridge_sha256"] or health["heavy_silent_sha256"] != data["package"]["files"]["HeavySilent.dll"]:
                raise RuntimeError(f"Unexpected profile or installed assembly hashes: {health}")
            data["status"] = "ready"
            write_json(SESSION_FILE, data); write_json(directory / "session.json", data)
            print(f"Test session ready: {run_id}", flush=True, file=sys.stderr)
            return data
        time.sleep(.5)
    raise TimeoutError(f"Game startup timed out; see {directory / 'game.log'}")


def stop():
    if not SESSION_FILE.exists():
        return {"status": "no_active_session"}
    data = read_json(SESSION_FILE)
    client = GameClient(data)
    if data["pid"] in game_pids():
        health = client.request("/hs/health")
        if health["session"] != data["token"] or health["pid"] != data["pid"]:
            raise RuntimeError("Refusing to stop a different game process.")
        state = client.state()
        if state.get("character_id"):
            client.request("/hs/save-quit", {})
            client.wait(lambda s: not s.get("character_id"))
        client.request("/hs/quit", {})
        deadline = time.monotonic() + 30
        while data["pid"] in game_pids() and time.monotonic() < deadline:
            time.sleep(.5)
        if data["pid"] in game_pids():
            raise TimeoutError("Owned game did not close; saved evidence remains available.")
    account = Path(data["account"])
    selector = account / "modded/profile.save"
    if data["selector_backup"] is not None:
        selector.write_bytes(base64.b64decode(data["selector_backup"]))
    elif selector.exists():
        selector.unlink()
    # Restore only settings that the visible test launch can change.
    current = read_json(account / "settings.save")
    for key in ("language", "fullscreen", "window_position", "window_size"):
        if key in data["original_settings"]:
            current[key] = data["original_settings"][key]
    write_json(account / "settings.save", current)
    after = personal_hashes(account, data.get('hash_scope') == 'modded_and_unmodded')
    changed = sorted(k for k in set(after) | set(data["personal_hashes"]) if after.get(k) != data["personal_hashes"].get(k))
    data.update(status="closed", personal_saves_unchanged=not changed, changed_personal_files=changed)
    write_json(Path(data["directory"]) / "session.json", data)
    SESSION_FILE.unlink()
    if changed:
        raise RuntimeError(f"Personal save hash mismatch: {changed}")
    print("Test session closed; profiles 1 and 2 match their original hashes.", flush=True, file=sys.stderr)
    return {"status": "closed", "personal_saves_unchanged": True}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("action", choices=("setup", "start", "stop"))
    parser.add_argument("--configuration", default="Debug", choices=("Debug", "Release"))
    parser.add_argument("--local-config")
    parser.add_argument("--language", choices=('eng', 'zhs'))
    args = parser.parse_args()
    with control_lock():
        if args.action == "setup":
            setup_args = [os.sys.executable, str(HERE / "bootstrap.py")]
            if args.local_config:
                setup_args += ['--local-config', args.local_config]
            subprocess.run(setup_args, cwd=ROOT, check=True)
            install_bridge(args.local_config)
        elif args.action == "start":
            start(args.configuration, args.local_config, args.language)
        else:
            stop()


if __name__ == "__main__":
    main()
