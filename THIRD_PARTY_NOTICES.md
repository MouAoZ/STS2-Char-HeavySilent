# Third-party notices

## Alchyr.Sts2.Templates 2.5.3

This project was initialized from the **Character Mod Template** in
[Alchyr/ModTemplate-StS2](https://github.com/Alchyr/ModTemplate-StS2).
The NuGet package declares the MIT license. The character/pool structure, Godot
project/export setup and the five placeholder PNGs originate from that template.
The build pipeline and teaching cards have been adapted for this repository.

## BaseLib 3.4.7

[Alchyr/BaseLib-StS2](https://github.com/Alchyr/BaseLib-StS2) is an external runtime
dependency, distributed under MIT. Install it separately; this mod's package
does not contain BaseLib or its resources.

## MIT license notice for the above upstream material

Author: Alchyr and contributors.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Slay the Spire 2

Game assemblies and extracted game files are local development references and
are not distributed here. Placeholder character visuals reference resources
from the user's installed game. No license for this project's original work
is selected by this notice.

## STS2MCP development bridge

The optional local testing bridge builds from
[Gennadiyev/STS2MCP](https://github.com/Gennadiyev/STS2MCP), pinned to
`55e064850a68f3b4cde7e5fd525bf9b2dec4e885`, with compatibility and test-session extensions.
Its upstream MIT notice begins: Copyright 2026 Yikun Ji (Kunologist).
Bootstrap preserves the full upstream LICENSE and deployment includes it as
`mods/HeavySilentTestBridge/LICENSE.txt`. The bridge is separate from the player mod package.

The Python MCP SDK and uv are development tools with their own upstream licenses;
their versions are recorded in `tools/game-testing/uv.lock` and `upstream.json`.
