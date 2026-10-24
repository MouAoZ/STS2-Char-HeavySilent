"""Check authoring contracts and the actual Godot pack without a game installation."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re
import struct
import sys
from urllib.parse import unquote
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
MOD_ROOT = ROOT / "mod"


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"duplicate JSON key: {key}")
        result[key] = value
    return result


def read_json(path: Path):
    return json.loads(path.read_text(encoding="utf-8-sig"), object_pairs_hook=unique_object)


def variables(text: str) -> set[str]:
    return set(re.findall(r"\{([A-Za-z][A-Za-z0-9_]*)(?=[:.}])", text))


def check_localization(root: Path) -> list[str]:
    errors = []
    loc = root / "HeavySilent/localization"
    tables = {}
    for language in ("eng", "zhs"):
        tables[language] = {}
        for path in sorted((loc / language).glob("*.json")):
            try:
                data = read_json(path)
                if not isinstance(data, dict) or not data:
                    raise ValueError("localization must be a nonempty object")
                if any(not isinstance(v, str) or not v.strip() for v in data.values()):
                    raise ValueError("localization values must be nonempty strings")
                tables[language][path.name] = data
            except (ValueError, OSError) as error:
                errors.append(f"{path.relative_to(root)}: {error}")
    if not tables["eng"] or tables["eng"].keys() != tables["zhs"].keys():
        errors.append("eng/zhs must contain the same nonempty set of localization tables")
    for name in tables["eng"].keys() & tables["zhs"].keys():
        english, chinese = tables["eng"][name], tables["zhs"][name]
        if english.keys() != chinese.keys():
            errors.append(f"{name}: eng/zhs keys differ: {sorted(english.keys() ^ chinese.keys())}")
        for key in english.keys() & chinese.keys():
            if variables(english[key]) != variables(chinese[key]):
                errors.append(f"{name}:{key}: eng/zhs dynamic variables differ")
    ids = {}
    for path in sorted((root / "HeavySilentCode").rglob("*.cs")):
        for model_id in re.findall(r'\[CustomID\("([^"]+)"\)\]', path.read_text(encoding="utf-8-sig")):
            if model_id in ids:
                errors.append(f"Duplicate model ID {model_id}: {path} and {ids[model_id]}")
            ids[model_id] = path
            if path.parent.name == "Cards":
                for language in ("eng", "zhs"):
                    cards = tables[language].get("cards.json", {})
                    for suffix in ("title", "description"):
                        if f"{model_id}.{suffix}" not in cards:
                            errors.append(f"Missing {language} card localization: {model_id}.{suffix}")
    return errors


def check_markdown_links(root: Path) -> list[str]:
    errors = []
    paths = [root / "README.md", root / "AGENTS.md"]
    paths += list((root / "docs").rglob("*.md"))
    paths += list((root / ".agents/skills").rglob("*.md"))
    for path in paths:
        if not path.exists():
            errors.append(f"Missing documentation entry: {path.relative_to(root)}")
            continue
        text = re.sub(r"```.*?```", "", path.read_text(encoding="utf-8-sig"), flags=re.S)
        for target in re.findall(r"\[[^\]]*\]\(([^)]+)\)", text):
            if re.match(r"https?://|mailto:|#", target):
                continue
            target = unquote(target.split("#", 1)[0].strip("<>"))
            if target and not (path.parent / target).exists():
                errors.append(f"{path.relative_to(root)}: broken local link {target}")
    return errors


def check_versions(root: Path) -> list[str]:
    errors = []
    manifest = read_json(root / "HeavySilent.json")
    project = ET.parse(root / "HeavySilent.csproj").getroot()
    props = ET.parse(root / "Directory.Build.props").getroot()
    framework = project.findtext(".//TargetFramework")
    if manifest["id"] != "HeavySilent" or not manifest["has_dll"] or not manifest["has_pck"]:
        errors.append("Manifest must declare the HeavySilent DLL and PCK")
    if manifest["version"] != project.findtext(".//Version"):
        errors.append("Manifest and assembly versions differ")
    base = props.findtext(".//BaseLibVersion")
    dependencies = {d["id"]: d["min_version"] for d in manifest["dependencies"]}
    if dependencies.get("BaseLib") != base:
        errors.append("Manifest BaseLib requirement differs from the compile dependency")
    for configuration in ("Debug", "Release"):
        lock = read_json(root / f"packages.{configuration}.lock.json")
        for package, prop in (("Alchyr.Sts2.BaseLib", "BaseLibVersion"), ("Alchyr.Sts2.ModAnalyzers", "ModAnalyzersVersion")):
            expected = props.findtext(f".//{prop}")
            if lock["dependencies"][framework][package]["resolved"] != expected:
                errors.append(f"{configuration} lock file disagrees with {package}={expected}")
    return errors


def read_pack(path: Path) -> dict[str, bytes]:
    """Read unencrypted Godot 4.5 PCK v3; reject truncated or ambiguous packs."""
    with path.open("rb") as stream:
        length = path.stat().st_size

        def take(count):
            data = stream.read(count)
            if len(data) != count:
                raise ValueError("truncated PCK")
            return data

        magic, version, major, minor, patch, flags = struct.unpack("<6I", take(24))
        if magic != 0x43504447 or version != 3:
            raise ValueError("expected a Godot PCK v3")
        if (major, minor, patch) != (4, 5, 1):
            raise ValueError(f"unexpected PCK engine {major}.{minor}.{patch}; update the baseline and verifier together")
        if flags & 1:
            raise ValueError("encrypted PCK directories are unsupported")
        file_base, directory_offset = struct.unpack("<QQ", take(16))
        if directory_offset >= length:
            raise ValueError("PCK directory is outside the file")
        stream.seek(directory_offset)
        count = struct.unpack("<I", take(4))[0]
        if count > 100000:
            raise ValueError("unexpected PCK entry count")
        entries = []
        seen = set()
        for _ in range(count):
            name_length = struct.unpack("<I", take(4))[0]
            if name_length > 16384:
                raise ValueError("invalid PCK path length")
            name = take(name_length).rstrip(b"\0").decode("utf-8").removeprefix("res://")
            offset, size = struct.unpack("<QQ", take(16))
            digest = take(16)
            entry_flags = struct.unpack("<I", take(4))[0]
            if name in seen or ".." in name.split("/") or name.startswith("/") or "\\" in name:
                raise ValueError(f"duplicate or unsafe PCK path: {name}")
            if entry_flags != 0:
                raise ValueError(f"unsupported PCK flags for {name}")
            start = file_base + offset
            if start + size > length:
                raise ValueError(f"PCK entry outside file: {name}")
            seen.add(name)
            entries.append((name, start, size, digest))
        result = {}
        for name, start, size, digest in entries:
            stream.seek(start)
            data = take(size)
            if hashlib.md5(data, usedforsecurity=False).digest() != digest:
                raise ValueError(f"PCK checksum mismatch: {name}")
            result[name] = data
        return result


def check_pack(root: Path, pack: Path) -> list[str]:
    entries = read_pack(pack)
    errors = []
    for name in entries:
        allowed = (name.startswith("HeavySilent/") or name.startswith(".godot/imported/")
                   or name in ("project.binary", ".godot/uid_cache.bin", ".godot/global_script_class_cache.cfg"))
        if not allowed or name.lower().endswith((".dll", ".cs", ".pdb")):
            errors.append(f"Unexpected content in mod PCK: {name}")
    for source in (root / "HeavySilent/localization").rglob("*.json"):
        name = source.relative_to(root).as_posix()
        if entries.get(name) != source.read_bytes():
            errors.append(f"Missing or stale localization in PCK: {name}")
    for source in (root / "HeavySilent").rglob("*.png"):
        name = source.relative_to(root).as_posix()
        imported = entries.get(name + ".import")
        if imported:
            for resource in re.findall(r'"res://([^"\n]+\.ctex)"', imported.decode("utf-8")):
                if resource not in entries:
                    errors.append(f"Missing imported texture in PCK: {resource}")
        elif name not in entries:
            errors.append(f"Missing image in PCK: {name}")
    return errors


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--pack", type=Path, help="also verify this generated PCK")
    args = parser.parse_args()
    try:
        errors = check_localization(MOD_ROOT) + check_versions(MOD_ROOT) + check_markdown_links(ROOT)
        if args.pack:
            errors += check_pack(MOD_ROOT, args.pack)
    except (OSError, ValueError, KeyError, ET.ParseError) as error:
        errors = [str(error)]
    for error in errors:
        print(f"ERROR: {error}", file=sys.stderr)
    if errors:
        return 1
    print("Verification passed: localization, IDs, dependency versions, documentation links" + (", PCK inventory/checksums/content." if args.pack else "."))
    return 0


if __name__ == "__main__":
    sys.exit(main())
