"""Reproduce the pinned development bridge; never copy game assemblies."""
from __future__ import annotations
import hashlib
import io
import json
import os
import re
from pathlib import Path
import subprocess
import urllib.request
import zipfile
import xml.etree.ElementTree as ET

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
LOCK = json.loads((HERE / "upstream.json").read_text())


def download(url: str, digest: str) -> bytes:
    cache = ROOT / ".cache/game-testing-downloads" / digest
    if cache.exists():
        data = cache.read_bytes()
    else:
        request = urllib.request.Request(url, headers={"User-Agent": "HeavySilent-testing"})
        with urllib.request.urlopen(request, timeout=60) as response:
            data = response.read()
    if hashlib.sha256(data).hexdigest() != digest:
        raise RuntimeError(f"Download checksum mismatch: {url}")
    cache.parent.mkdir(parents=True, exist_ok=True)
    cache.write_bytes(data)
    return data


def replace_exact(text: str, old: str, new: str, count: int = 1) -> str:
    if text.count(old) != count:
        raise RuntimeError(f"Upstream patch context changed: {old!r}")
    return text.replace(old, new)


def patch_source(name: str, text: str) -> str:
    if name == "McpMod.Actions.cs":
        # v0.111 standard lobby's UI listener rejects SetSeed. The game provides
        # an explicit debug override consumed by the normal embark pipeline.
        text = replace_exact(text, 'charSelect.Lobby.SetSeed(seed);', 'MegaCrit.Sts2.Core.Nodes.NGame.Instance!.DebugSeedOverride = seed;')
    # v0.111: lobby players are structs in Players; MaxPlayers became private.
    if name == "McpMod.StateBuilder.cs":
        for old, new in [
            ('lobby.MaxPlayers', 'GetInstanceFieldValue(lobby, "_maxPlayers")'),
            ('lobby.ConnectedPlayerIds?.Count ?? 0', 'lobby.Players.Count'),
            ('lobby.ConnectedPlayerIds;', 'lobby.Players.Select(p => p.id).ToHashSet();'),
            ('lobby.ConnectedPlayerIds?.Contains(sp.NetId) ?? false', 'lobby.Players.Any(p => p.id == sp.NetId)'),
        ]:
            text = replace_exact(text, old, new)
    if name == "McpMod.cs":
        text = replace_exact(text, 'public static void Initialize()\n    {',
                             'public static void Initialize()\n    {\n        if (!HsEnabled) return;\n        HsUseLocalSaves();')
        text = replace_exact(text, 'while (_mainThreadQueue.TryDequeue(out var action) && processed < 10)',
                             'while (processed < 10 && _mainThreadQueue.TryDequeue(out var action))')
        text = replace_exact(text, 'var request = context.Request;\n            var response = context.Response;',
                             'var request = context.Request;\n            var response = context.Response;\n'
                             '            if (!AuthorizeHsRequest(request, response)) return;\n'
                             '            if (TryHandleHsRequest(context)) return;')
    return text


def prepare_sources() -> Path:
    source = ROOT / ".tools/sts2mcp/src"
    source.mkdir(parents=True, exist_ok=True)
    data = download(LOCK["archive_url"], LOCK["archive_sha256"])
    with zipfile.ZipFile(io.BytesIO(data)) as archive:
        for entry in archive.infolist():
            relative = Path(*Path(entry.filename).parts[1:])
            if len(relative.parts) != 1:
                continue
            if relative.name == "LICENSE" or relative.name.startswith("McpMod") and relative.suffix == ".cs":
                original = archive.read(entry).decode("utf-8-sig").replace("\r\n", "\n")
                (source / relative.name).write_text(patch_source(relative.name, original), encoding="utf-8", newline="\n")
    return source


def prepare_python() -> Path:
    uv = ROOT / ".tools/uv/uv.exe"
    if not uv.exists():
        data = download(LOCK["uv_wheel_url"], LOCK["uv_wheel_sha256"])
        with zipfile.ZipFile(io.BytesIO(data)) as archive:
            executable = next(n for n in archive.namelist() if n.endswith("/uv.exe"))
            uv.parent.mkdir(parents=True, exist_ok=True)
            uv.write_bytes(archive.read(executable))
    env = os.environ | {"UV_CACHE_DIR": str(ROOT / ".cache/uv"), "UV_PYTHON_INSTALL_DIR": str(ROOT / ".tools/python")}
    args = [str(uv), "sync", "--project", str(HERE), "--python", "3.13"]
    if (HERE / "uv.lock").exists():
        args.append("--locked")
    subprocess.run(args, env=env, cwd=ROOT, check=True)
    return HERE / ".venv/Scripts/python.exe"


def main() -> None:
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--sources-only", action="store_true")
    parser.add_argument("--bridge-only", action="store_true")
    parser.add_argument("--local-config")
    args = parser.parse_args()
    prepare_sources()
    if args.sources_only:
        return
    python = HERE / ".venv/Scripts/python.exe" if args.bridge_only else prepare_python()
    build = ["dotnet", "build", str(HERE / "bridge/HeavySilentTestBridge.csproj"), "-c", "Release",
             "--configfile", str(ROOT / "mod/NuGet.Config"), "-o", str(ROOT / "artifacts/test-bridge")]
    if args.local_config:
        game = ET.parse(args.local_config).getroot().findtext('.//Sts2Path')
        build.append(f"-p:Sts2Path={game}")
    subprocess.run(build, cwd=ROOT / "mod", check=True)
    if args.bridge_only:
        return
    # Local machine paths are generated, not placed in the shared template.
    config = ROOT / ".codex/config.toml"
    stanza = ('[mcp_servers.heavysilent_game]\ncommand = ' + json.dumps(str(python)) +
              '\nargs = [' + json.dumps(str(HERE / "mcp_server.py")) + ']\nstartup_timeout_sec = 30\ntool_timeout_sec = 180\n')
    existing = config.read_text(encoding='utf-8') if config.exists() else ''
    pattern = r'(?ms)^\[mcp_servers\.heavysilent_game\]\s*\n.*?(?=^\[|\Z)'
    updated = re.sub(pattern, lambda _: stanza + '\n', existing) if re.search(pattern, existing) else existing.rstrip() + '\n\n' + stanza
    config.parent.mkdir(parents=True, exist_ok=True)
    config.write_text(updated.lstrip('\n'), encoding='utf-8')
    print("Pinned bridge and Python environment ready.")


if __name__ == "__main__":
    main()
