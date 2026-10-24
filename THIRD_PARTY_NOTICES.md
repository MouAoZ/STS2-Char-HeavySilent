# Development scaffold notices

Player-mod licenses and required distribution notices are maintained in
[mod/THIRD_PARTY_NOTICES.md](mod/THIRD_PARTY_NOTICES.md) and are included in the player package.
This document covers the development scaffold, which is not required to build the standalone mod.

## STS2MCP development bridge

The optional local testing bridge builds from
[Gennadiyev/STS2MCP](https://github.com/Gennadiyev/STS2MCP), pinned to
`55e064850a68f3b4cde7e5fd525bf9b2dec4e885`, with compatibility and test-session extensions.
Its upstream MIT notice begins: Copyright 2026 Yikun Ji (Kunologist).
Bootstrap preserves the full upstream LICENSE and deployment includes it as
`mods/HeavySilentTestBridge/LICENSE.txt`. The bridge is separate from the player mod package.

The Python MCP SDK and uv are development tools with their own upstream licenses;
their versions are recorded in `tools/game-testing/uv.lock` and `upstream.json`.
