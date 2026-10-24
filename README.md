# Heavy Silent

《杀戮尖塔 2》独立角色模组，当前交付为**开发骨架与教学样例**。
人类通过 Markdown 设计玩法并 Review；Agent 负责代码、验证和技术说明。

已实现：角色注册、静默占位表现、打击（6／9 伤害）、防御（5／8 格挡）、气（0 费状态牌，抽 1／2 张，消耗）。
测试套牌是五打击、五防御、两张气，70 生命、99 金币及蛇之戒。
磨顶、征召、两张正式特殊初始牌和完整奖励卡池尚未实现；这不是完整可跑塔的角色版本。

## 从这里开始

- 策划与 Review：[开发手册与学习路线](docs/README.md)
- 当前实现和验证结果：[项目状态](docs/development/status.md)
- 原始玩法：[卡牌设计 v0.1](docs/design/卡牌设计v0.1.md)
- Agent 工作入口：[AGENTS.md](AGENTS.md)

## 本机启动

需要 Windows、正版游戏 `public-beta`、.NET SDK **10.0.402**、**MegaDot .NET 4.5.1-m.14**、BaseLib **3.4.7**、Python **3.11+**。推荐使用 PowerShell 7。
SDK 版本与目标框架不同：本工程用 SDK 10 编译游戏使用的 `net9.0`。
依赖与游戏版本见[版本基线](docs/development/baseline.json)；更新测试版后按[升级流程](.agents/skills/sts2-heavysilent-dev/references/build-debug.md)重新验证。

1. 将 `local.props.example` 复制为 `local.props`，填写游戏、引擎和已安装 BaseLib 的绝对路径。
2. 在仓库根目录打开 PowerShell 7，依次执行：

```powershell
.\scripts\dev.ps1 doctor
.\scripts\dev.ps1 verify
.\scripts\dev.ps1 package
.\scripts\dev.ps1 deploy
```

`build` 只编译；`package` 构建并验证 DLL／PCK／manifest；`deploy` 安装已完成且未过期的包。
它们不会替换原版 DLL。若系统限制本地脚本，可在当前会话执行
`Set-ExecutionPolicy -Scope Process Bypass`，无需更改机器级策略。

本机 MegaDot 位于 `D:/Studio/GameDev/_Apps/MegaDot/4.5.1-m.14/`，配置示例已指向其 console 可执行文件。
不支持 MSIX 版 PowerShell 的命令行可显式使用以下安装路径（本机已验证为 7.6.8）：

```powershell
& 'C:\Program Files\PowerShell\7\pwsh.exe' -NoProfile -File .\scripts\dev.ps1 doctor
```

启动游戏，在模组设置中启用 BaseLib 和 HeavySilent，并按游戏要求重启。
之后执行[实机 Review 清单](docs/development/smoke-test.md)。构建通过与实机验收是两项独立结果。

```powershell
.\scripts\dev.ps1 logs                  # 最近游戏日志
.\scripts\dev.ps1 doctor -CheckLatest   # 可选：查询 public-beta 分支元数据
.\scripts\dev.ps1 package -Configuration Release
python -m unittest discover -s tests -v
```

产物位于 `artifacts/package/HeavySilent/`，构建日志位于 `artifacts/logs/`。
自动实机测试使用仓库内 Python 3.13 和独立测试桥，首次准备后即可运行：

```powershell
.\scripts\dev.ps1 test-setup
.\scripts\dev.ps1 test-start
# Agent 通过 MCP 按当前改动构造场景、操作和记录判断。
.\scripts\dev.ps1 test-stop
```

主入口是 Agent 针对当前改动开展测试。也可用 `test -TaskFile <task.json>` 启动并登记测试意图，随后通过 MCP 自由构造有序牌堆、状态和敌人条件，实际出牌、截图并记录判断。
工作流复用现有打包部署，使用专用第 3 档和 `unlock all` 测试环境；第 1、2 档有备份和哈希检查。
已有固定检查保留为显式 `test -Suite smoke|regression`，按改动相关性选用，不默认每次运行。详细工具和存档边界见[Agent 实机测试](.agents/skills/sts2-heavysilent-dev/references/game-testing.md)。

## 填充下一项内容

复制[对应的策划模板](docs/design/templates/README.md)，填写效果顺序、升级、边界和验收例子，然后让 Agent 实现。
Markdown 是设计输入，运行时效果由 C# 与本地化 JSON 提供。
新增机制前先处理[机制待澄清项](.agents/skills/sts2-heavysilent-dev/references/mechanics.md)。

模板与占位图来源及许可见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
