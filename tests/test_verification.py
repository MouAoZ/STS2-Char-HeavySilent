"""Regression checks for asset/package failures that compilation cannot detect."""
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import struct
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]
spec = importlib.util.spec_from_file_location("verify", ROOT / "scripts/verify.py")
verify = importlib.util.module_from_spec(spec)
spec.loader.exec_module(verify)


def make_pack(path, entries, engine=(4, 5, 1)):
    """Small real PCK v3 fixture, not a mock of the reader."""
    content, directory = bytearray(), bytearray(struct.pack("<I", len(entries)))
    for name, data in entries.items():
        encoded = name.encode()
        directory += struct.pack("<I", len(encoded)) + encoded
        directory += struct.pack("<QQ", len(content), len(data))
        directory += hashlib.md5(data, usedforsecurity=False).digest() + struct.pack("<I", 0)
        content += data
    header = struct.pack("<6IQQ", 0x43504447, 3, *engine, 2, 112, 112 + len(content))
    path.write_bytes(header.ljust(112, b"\0") + content + directory)


class VerificationTests(unittest.TestCase):
    def setUp(self):
        self.cache = (ROOT / ".cache/verification-tests").resolve()
        self.cache.mkdir(parents=True, exist_ok=True)
        self.root = Path(tempfile.mkdtemp(dir=self.cache)).resolve()

    def tearDown(self):
        # All recursive cleanup stays inside this test's workspace cache.
        self.assertTrue(self.root.is_relative_to(self.cache))
        shutil.rmtree(self.root)

    def test_duplicate_json_key_is_an_error(self):
        path = self.root / "cards.json"
        path.write_text('{"CARD.title":"A","CARD.title":"B"}')
        with self.assertRaisesRegex(ValueError, "duplicate JSON key"):
            verify.read_json(path)

    def test_translations_must_reference_the_same_dynamic_values(self):
        for language, text in (("eng", "Draw {Cards:diff()} cards."), ("zhs", "造成{Damage:diff()}点伤害。")):
            folder = self.root / "HeavySilent/localization" / language
            folder.mkdir(parents=True)
            (folder / "cards.json").write_text(json.dumps({"QI.description": text}), encoding="utf-8")
        self.assertTrue(any("dynamic variables differ" in error for error in verify.check_localization(self.root)))

    def test_pack_detects_stale_localization(self):
        source = self.root / "HeavySilent/localization/zhs/cards.json"
        source.parent.mkdir(parents=True)
        source.write_text('{"QI.title":"New"}', encoding="utf-8")
        pack = self.root / "mod.pck"
        make_pack(pack, {"HeavySilent/localization/zhs/cards.json": b'{"QI.title":"Old"}'})
        self.assertTrue(any("stale localization" in error for error in verify.check_pack(self.root, pack)))

    def test_pack_rejects_unrelated_assembly(self):
        pack = self.root / "mod.pck"
        make_pack(pack, {"sts2.dll": b"must not ship the game"})
        self.assertTrue(any("Unexpected content" in error for error in verify.check_pack(self.root, pack)))

    def test_pack_detects_corruption(self):
        pack = self.root / "mod.pck"
        make_pack(pack, {"HeavySilent/sample.json": b"{}"})
        data = bytearray(pack.read_bytes())
        data[112] ^= 1
        pack.write_bytes(data)
        with self.assertRaisesRegex(ValueError, "checksum mismatch"):
            verify.read_pack(pack)

    def test_truncated_pack_fails(self):
        pack = self.root / "mod.pck"
        pack.write_bytes(b"GDPC")
        with self.assertRaisesRegex(ValueError, "truncated"):
            verify.read_pack(pack)

    def test_newer_engine_pack_fails(self):
        pack = self.root / "mod.pck"
        make_pack(pack, {}, engine=(4, 6, 2))
        with self.assertRaisesRegex(ValueError, "unexpected PCK engine"):
            verify.read_pack(pack)

    def test_valid_pack_is_read(self):
        pack = self.root / "mod.pck"
        entries = {"HeavySilent/sample.json": '{"text":"气"}'.encode()}
        make_pack(pack, entries)
        self.assertEqual(entries, verify.read_pack(pack))


if __name__ == "__main__":
    unittest.main()
