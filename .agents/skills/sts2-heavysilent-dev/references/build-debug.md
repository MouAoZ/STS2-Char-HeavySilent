# 构建、调试与版本升级

核验基线：[baseline.json](../../../../docs/development/baseline.json)。
统一入口：[scripts/dev.ps1](../../../../scripts/dev.ps1)。当前支持 Windows，PowerShell 5.1+、Python 3.11+；本机优先使用 PowerShell 7。

## 环境和配置

1. Steam 游戏使用 public-beta 并完成更新。
2. 安装 `global.json` 指定的 .NET 10 SDK；当前固定为本机的 10.0.402。游戏运行时仍是 .NET 9，因此 `TargetFramework` 保持 `net9.0`。
3. 使用与游戏匹配的 MegaDot .NET；本机已安装到 `D:/Studio/GameDev/_Apps/MegaDot/4.5.1-m.14/`，`local.props` 的 `GodotPath` 指向其中的 `MegaDot_v4.5.1-stable_mono_win64_console.exe`。配置字段继续叫 `GodotPath`；引擎基础版本与 Godot.NET.Sdk 仍为 4.5.1。不要使用现有的 4.6.2 导出给 4.5.1 游戏。
4. 安装项目声明的 BaseLib。优先用已有 Workshop 版本；不要另复制一个不同版本进本地 mods。
5. 将 `local.props.example` 复制为 `local.props` 并填写三个绝对路径。

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
`packages.Debug.lock.json` 与 `packages.Release.lock.json` 分别锁定两种配置。Godot SDK 只在 Debug 引用编辑器程序集，因此不能共用一个依赖锁文件。
`package` 生成的完整记录在 `artifacts/package/package-info.json`。部署时检查源文件指纹和游戏／BaseLib 哈希，防止把更新前或改代码前的旧包装入游戏。
打包失败会移除完成记录；历史文件即使仍在输出目录也不能被当成新包部署。

PCK 在 `.cache` 下的独立资源工程导出，避免 Godot 再次发布一套 C# 产物，也防止把本地设计、缓存或游戏 DLL 打包进去。
`scripts/verify.py --pack <路径>` 检查 PCK 内容、校验和和文本是否与源码一致；`verify_pack.gd` 在空项目中挂载该包并实际加载纹理和 JSON。

## 如何定位问题

- **SDK 找不到**：先看 `dotnet --list-sdks` 和 `global.json`。Agent 沙箱若无法读取用户 NuGet 配置，应让对应构建动作正常提权执行；不要把它误诊为项目依赖不存在。
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
4. 编译并用 ILSpy 检查变化的类型；修复受影响 API。新增依赖或修改版本时，分别执行 `dotnet restore -p:Configuration=Debug --configfile NuGet.Config` 与 `dotnet restore -p:Configuration=Release --configfile NuGet.Config`，Review 两份锁文件差异。
5. 重新打包与实机验证后，更新基线、技术参考和状态记录。引擎升级时也更新 PCK 验证器支持的版本。

初始化的版本核对是一次有日期的观察，不是今后永远兼容 public-beta 的承诺。

## MCP 与额外工具

编译与打包只需文件工具、终端和 `ilspycmd`。游戏自动测试已提供项目级 MCP，CLI 与 MCP 共用实现，见[测试工作流](game-testing.md)；没有修改个人 Codex 全局配置。
OpenAI Docs MCP 可辅助查询 Codex 配置；GitHub 工具在需要 PR／Issue 协作时再接入；Godot 编辑器 MCP 在有实际场景自动化需求时再评估。
本机反编译与资源提取结果只放忽略的 `.cache`，不提交完整游戏源码或资源。

## 验证边界与回退

自动构建、资源验证不能证明玩家实际交互正确。实机步骤见[清单](../../../../docs/development/smoke-test.md)，结果写入[状态](../../../../docs/development/status.md)。
回退本模组时，在游戏关闭状态部署先前已验证的包，或在游戏模组设置中禁用 HeavySilent；无需恢复或修改原版 DLL。
发布到 Workshop 不是本次初始化内容，未来使用 Mega Crit 的官方上传工具并单独整理发布信息。
