# 构建、调试与版本升级

核验基线：[baseline.json](../../../../docs/development/baseline.json)。
开发脚手架入口：[scripts/dev.ps1](../../../../scripts/dev.ps1)；独立源码构建入口：[mod/build.ps1](../../../../mod/build.ps1)。当前支持 Windows，PowerShell 5.1+；本机优先使用 PowerShell 7。Python 3.11+ 仅用于外层验证与开发工具，独立打包不需要 Python。

## 环境和配置

1. Steam 游戏使用 public-beta 并完成更新。
2. 安装 `mod/global.json` 指定的 .NET 10 SDK；当前固定为本机的 10.0.402。游戏运行时仍是 .NET 9，因此 `TargetFramework` 保持 `net9.0`。
3. 使用与游戏匹配的 MegaDot .NET；本机已安装到 `D:/Studio/GameDev/_Apps/MegaDot/4.5.1-m.14/`，`mod/local.props` 的 `GodotPath` 指向其中的 `MegaDot_v4.5.1-stable_mono_win64_console.exe`。配置字段继续叫 `GodotPath`；引擎基础版本与 Godot.NET.Sdk 仍为 4.5.1。不要使用现有的 4.6.2 导出给 4.5.1 游戏。
4. 安装项目声明的 BaseLib。优先用已有 Workshop 版本；不要另复制一个不同版本进本地 mods。
5. 将 `mod/local.props.example` 复制为 `mod/local.props` 并填写三个绝对路径。

不需要为本项目重新运行模板生成器。初始化来源是 Alchyr.Sts2.Templates 2.5.3，后续直接维护本仓库。
NuGet 的 BaseLib 负责编译，游戏目录／Workshop 的 BaseLib 负责运行，两个版本需要一致。
`ModAnalyzers` 是编译期依赖，不是玩家要安装的前置模组。

本机独立安装的 PowerShell 位于 `C:\Program Files\PowerShell\7\pwsh.exe`，已核验为 7.6.8。
Agent 和不支持 MSIX 版本的命令行优先使用这个绝对路径，避免依赖可能指向 MSIX 应用的 `pwsh` 别名；无需修改全局 PATH。
从 PowerShell 启动脚本的例子：

```powershell
& 'C:\Program Files\PowerShell\7\pwsh.exe' -NoProfile -File .\scripts\dev.ps1 doctor
& 'C:\Program Files\PowerShell\7\pwsh.exe' -NoProfile -File .\scripts\dev.ps1 package
```

使用 `cmd.exe` 时去掉开头的 `&`，并用双引号包住可执行文件路径。`doctor` 会显示实际运行脚本的 PowerShell 路径和版本，打包记录也保存该版本。

## 仅有源码时独立打包

将 `mod/` 的可提交内容复制到独立目录，保留其中的工程、资源、SDK／NuGet 配置、锁文件、manifest、随包许可和 build.ps1。外层文档、设计、Skill、Python、验证脚本与测试桥均不是打包输入。不要复制本机 local.props、`.godot/`、`.cache/` 或 `artifacts/`；`local.props.example` 可作为路径配置样例。

在复制后的目录运行（替换游戏与引擎路径）：

```powershell
.\build.ps1 -Action package -Configuration Release `
    -Sts2Path 'D:/SteamLibrary/steamapps/common/Slay the Spire 2' `
    -GodotPath 'D:/Studio/GameDev/_Apps/MegaDot/4.5.1-m.14/MegaDot_v4.5.1-stable_mono_win64_console.exe'
```

也可填写该目录的 local.props 后省略路径参数。独立构建使用 Sts2Path 和 GodotPath；BaseLib 从 NuGet 锁定依赖还原，BaseLibPath 用于外层 doctor 和实机工具核对运行前置。首次还原需要可访问 NuGet 或已有依赖缓存。

默认 Action 为 package、Configuration 为 Release，输出为该目录下的 `artifacts/package/HeavySilent/`：DLL、PCK、manifest、第三方许可，Debug 额外含 PDB。`-Action build` 只编译，程序集在 `.godot/mono/temp/bin/<配置>/`。`-OutputRoot` 可指定产物根目录；开发脚手架使用它将日志与包放在仓库根 `artifacts/`。

独立入口只负责构建产物；外层 `dev.ps1 package` 复用这个入口后执行本地化／ID／依赖／文档检查、PCK 清单与哈希校验、引擎实际挂载，并写入部署完成记录。独立输出没有脚手架的部署记录；使用 `dev.ps1 deploy` 前仍需先运行 `dev.ps1 package`。

## VS Code 的 GDScript 语言服务器

Godot Tools 需要运行中的 Godot 编辑器实例提供语言服务；只配置可执行文件路径不会自动启动该实例。若希望所有 Godot 项目都自动启动后台语言服务，通过 `Preferences: Open User Settings (JSON)` 在 VS Code 用户设置中添加 `"godotTools.lsp.headless": true`，并将 `godotTools.editorPath.godot4` 设为常用 Godot 主程序。本机通用默认版本为已安装的 Godot 4.6.2 .NET。

本项目在本机 `.vscode/settings.json` 中单独将 `godotTools.editorPath.godot4` 覆盖为 `D:/Studio/GameDev/_Apps/MegaDot/4.5.1-m.14/MegaDot_v4.5.1-stable_mono_win64.exe`，不重复设置 Headless 开关，继承用户配置。工作区同名设置优先于用户设置，打开本项目时仍使用匹配游戏的 MegaDot。换机器时调整实际路径；若该机器未配置用户级 Headless，也可仅在工作区启用。

这些配置由 VS Code 和 Godot Tools 自行读取，不依赖 Agent 或 Codex。VS Code 不读取 `mod/local.props`；构建脚本的 `GodotPath` 继续使用同版本 `_console.exe`，便于命令行日志和退出状态处理，VS Code 使用主程序。修改设置后执行 `Developer: Reload Window`。引擎工程入口现为 `mod/project.godot`，C# 解决方案为 `mod/HeavySilent.sln`；在引擎或 IDE 中使用这些入口。

本机 Godot Tools 2.7.1 在 Headless 模式下会自动分配空闲端口，无需固定 `serverPort`。如果关闭 Headless 模式，则需要先用 MegaDot 打开本项目，再连接编辑器的 LSP 端口（Godot 4 默认 `6005`）。`输出 → Godot LSP` 可查看连接日志；只看到反复尝试 `6005`／`6008` 且没有引擎进程时，应先检查服务是否启动。

该配置用于 `.gd` 脚本；C# 代码补全由 C# 扩展提供。官方说明见 [Godot Tools 配置](https://github.com/godotengine/godot-vscode-plugin#configuration)与 [Godot 4.5 外部编辑器](https://docs.godotengine.org/en/4.5/tutorials/editor/external_editor.html#lsp-dap-support)。

## 命令的区别

| 命令 | 作用 | 是否修改游戏目录 |
| --- | --- | --- |
| `doctor` | 检查路径、SDK、引擎、运行前置和版本漂移 | 否 |
| `doctor -CheckLatest` | 额外查询 SteamCMD 元数据镜像，对比 public-beta Build ID | 否 |
| `build` | 锁文件还原与编译 | 否 |
| `verify` | JSON／双语／ID／文档链接检查，加编译 | 否 |
| `package` | 构建、独立资源导出、包内容与真实资源加载检查 | 否 |
| `deploy` | 验证包未过期及哈希后安装本模组 | 是，仅 `mods/HeavySilent` |
| `logs` | 输出最近游戏日志 | 否 |
| `test-setup`／`test-start`／`test`／`test-stop` | 准备与运行可见游戏自动测试，见[测试工作流](game-testing.md) | 安装本模组及独立测试桥，使用专用第 3 档 |

加 `-Configuration Release` 可构建发布配置；默认 Debug 包含 PDB 用于调试。
只检查源码与文档、不依赖游戏安装时可运行 `python scripts/verify.py`；它不等于上述包含 C# 编译的 `dev.ps1 verify`。
`mod/packages.Debug.lock.json` 与 `mod/packages.Release.lock.json` 分别锁定两种配置。Godot SDK 只在 Debug 引用编辑器程序集，因此不能共用一个依赖锁文件。构建入口先切换到 `mod/`，确保 dotnet 使用其中的 global.json 和 NuGet.Config。
`package` 生成的完整记录在 `artifacts/package/package-info.json`。部署时检查源文件指纹和游戏／BaseLib 哈希，防止把更新前或改代码前的旧包装入游戏。
打包失败会移除完成记录；历史文件即使仍在输出目录也不能被当成新包部署。

PCK 在 `mod/.cache` 下的独立资源工程导出，避免 Godot 再次发布一套 C# 产物，也防止把本机配置或游戏 DLL 打包进去。
`scripts/verify.py --pack <路径>` 以 `mod/` 为源码根核对 PCK 内容、校验和和文本，同时检查外层文档链接；`verify_pack.gd` 在空项目中挂载该包并实际加载纹理和 JSON。资源内部路径保持 `res://HeavySilent/`，不添加磁盘上的 `mod/` 前缀。

## 如何定位问题

- **SDK 找不到**：先看 `dotnet --list-sdks`，在 `mod/` 中运行 `dotnet --version` 核对 global.json。Agent 沙箱若无法读取用户 NuGet 配置，应让对应构建动作正常提权执行；不要把它误诊为项目依赖不存在。
- **STS001 本地化缺项**：读取诊断中完整键名，补充 eng 与 zhs 对应表。角色还需要古代人对白和语法相关项。
- **DLL 更新了，文案没变**：执行 `package` 和 `deploy`，确认 PCK 同步更新，并重启游戏。
- **图片存在但加载失败**：检查 `.png.import` 与 PCK 中的导入纹理；使用 `package` 的实际加载检查。
- **模组加载失败**：先看 manifest、BaseLib 版本和游戏日志，再查本模组初始化标记。
- **运行时异常**：从日志栈中找到第一个本项目方法，再看调用它的原版命令和实际游戏版本。

游戏日志通常在 `%APPDATA%/SlayTheSpire2/logs/godot.log`。控制台 `open logs` 可定位，BaseLib 的 `showlog` 可显示日志窗口。
控制台命令参数用当前版本 `help` 确认。断点调试可让 Rider／支持 .NET 的调试器附加游戏进程，PDB 应与当前 DLL 一起部署。

## 游戏更新后

1. Steam 完成 public-beta 更新，再运行 `doctor -CheckLatest`。在线镜像不可用时使用 Steam 客户端确认，不把网络失败当作“没有新版本”。
2. 读取游戏 `release_info.json`、`sts2.runtimeconfig.json`、`sts2.deps.json`，核对目标框架与 Godot 版本。
3. 查看 BaseLib 的兼容更新；如需更新，显式调整版本、manifest、锁文件，不能改成通配符。
4. 编译并用 ILSpy 检查变化的类型；修复受影响 API。新增依赖或修改版本时，在 `mod/` 中分别执行 `dotnet restore -p:Configuration=Debug --configfile NuGet.Config` 与 `dotnet restore -p:Configuration=Release --configfile NuGet.Config`，Review 两份锁文件差异。
5. 重新打包与实机验证后，更新基线、技术参考和状态记录。引擎升级时也更新 PCK 验证器支持的版本。

初始化的版本核对是一次有日期的观察，不是今后永远兼容 public-beta 的承诺。

## MCP 与额外工具

编译与打包使用前述 SDK、引擎、Python 和终端；`ilspycmd` 仅用于查询原版 API，不是构建前置。游戏自动测试已提供项目级 MCP，CLI 与 MCP 共用实现，见[测试工作流](game-testing.md)；没有修改个人 Codex 全局配置。
OpenAI Docs MCP 可辅助查询 Codex 配置；GitHub 工具在需要 PR／Issue 协作时再接入；Godot 编辑器 MCP 在有实际场景自动化需求时再评估。
本机反编译与资源提取结果只放忽略的 `.cache`，不提交完整游戏源码或资源。

## 查询一个不确定的 API

先读本项目类似内容，再读原版同类效果及其调用的 Command／Hook。不要只凭方法名判断它是否触发抽牌、弃牌或保存流程。
策划给出中文参考牌名时，先从本机中文本地化定位 ID，再沿对应类型核对调用链；已查证的入口见[原版效果参考](vanilla-effects.md)，原版足以表达效果时直接复用。
本机 `ilspycmd --version` 为 10.0.1.8346；以下 PowerShell 命令已实际读取当前基线的 `StrikeSilent`：

```powershell
[xml]$apiConfig = Get-Content mod/local.props -Raw
$gameAssembly = Join-Path $apiConfig.Project.PropertyGroup.Sts2Path 'data_sts2_windows_x86_64/sts2.dll'
New-Item -ItemType Directory -Force .cache/game-api | Out-Null
ilspycmd -t MegaCrit.Sts2.Core.Models.Cards.StrikeSilent $gameAssembly > .cache/game-api/StrikeSilent.cs
```

更换 `-t` 后可查询 `MegaCrit.Sts2.Core.Commands.CardPileCmd` 等完整类型名。不知道完整名称时，用 `ilspycmd -l c $gameAssembly` 列出类再用 `rg` 筛选。查询 BaseLib 时改为 `mod/local.props` 中 `BaseLibPath` 下的 `BaseLib.dll`。
如果新机器没有该工具，可按需执行 `dotnet tool install --tool-path .tools/ilspy ilspycmd --version 10.0.1`，之后使用 `.tools/ilspy/ilspycmd.exe`；安装需要网络及相应 .NET 运行时。
只保存本次需要的类型，核对参数、异步时序和调用者，再以项目编译和实机行为确认。把必要的原理与签名解释写回技术参考，不把反编译缓存当成需提交的源码。

## 验证边界与回退

自动构建、资源验证不能证明玩家实际交互正确。实机步骤见[清单](../../../../docs/development/smoke-test.md)，结果写入[状态](../../../../docs/development/status.md)。
回退本模组时，在游戏关闭状态部署先前已验证的包，或在游戏模组设置中禁用 HeavySilent；无需恢复或修改原版 DLL。
Workshop 发布尚未开展；发布时使用 Mega Crit 的官方上传工具并单独整理发布信息。
