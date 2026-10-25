# Heavy Silent

《杀戮尖塔 2》独立角色模组，当前交付为**开发骨架与教学样例**。
人类通过 Markdown 设计玩法并 Review；Agent 负责代码、验证和技术说明。

## 源码与脚手架

**[mod/](mod) 是可独立打包的模组源码目录。** 复制其中的源码、资源和构建文件，准备好 .NET SDK、MegaDot .NET、游戏程序集引用及锁定的 NuGet 依赖，即可产出模组。它不读取仓库外层的文档、设计、开发脚本或测试工具。

| 范围 | 位置 | 用途 |
| --- | --- | --- |
| 模组源码与构建输入 | `mod/HeavySilentCode/`、`mod/HeavySilent/`、工程／依赖／导出配置、manifest、随包许可、`mod/build.ps1` | 构建玩家安装的 DLL、PCK、manifest；构建入口随源码保留 |
| 开发脚手架 | `docs/`、`.agents/`、`AGENTS.md`、本 README、`scripts/`、`tools/`、`tests/` | 策划、教学、进度、检查、部署及实机测试；不属于独立打包输入 |
| 本机配置与生成产物 | `mod/local.props`、各级 `.godot/`、`.cache/`、`artifacts/`、测试环境 | 按需生成且不提交，不作为源码分发 |

只打包源码时，在仓库根目录运行：

```powershell
Copy-Item .\mod\local.props.example .\mod\local.props
# 填写 mod/local.props 的本机路径后：
.\mod\build.ps1 -Action package -Configuration Release
```

独立入口默认输出 `mod/artifacts/package/HeavySilent/`，不需要 Python。开发流程继续使用 `scripts/dev.ps1`，它复用同一构建入口并增加静态检查、包验证和部署保护，输出仍在根目录 `artifacts/`。完整边界与文件职责见[工程结构](.agents/skills/sts2-heavysilent-dev/references/architecture.md)。

## 项目进度

更新日期：2026-10-26。当前已打通角色注册、内容样例、构建部署与 Agent 实机测试；磨顶／征召基础规则已确认，下一阶段是实现共用机制与正式起始内容。完整角色爬塔、平衡、联机和发布尚未验收。

| 范围 | 当前进度 | 剩余工作／依据 |
| --- | --- | --- |
| 工程与测试工具 | 已具备开发能力 | 版本基线、双语本地化、打包部署、场景构造与证据记录；见[验证记录](docs/development/status.md) |
| 角色与起始配置 | 开发占位 | 当前五打五防两张气、70生命、99金币、蛇之戒及静默外观；见[角色代码](mod/HeavySilentCode/Character/HeavySilentCharacter.cs) |
| 卡牌设计落地 | **3／77 已实现，0 进行中，74 未实现** | 三张为打击、防御、气；待实现包括两张特殊初始牌、70张攻击／技能／能力牌和两张先古相关卡；见[逐项跟踪](docs/design/implementation-tracker.md#内容统计) |
| 磨顶／征召 | **0／2 已实现** | 基础规则已按问答确认，原版对照已核对；结算与术语悬停解释均待实现，见[机制条目](docs/design/mechanics.md#术语与实现状态) |
| 遗物 | **0／4 已实现** | 初始、初始升级及两件专属遗物；见[遗物条目](docs/design/relics-events-potions.md#遗物) |
| 美术与其他内容 | 未完成／未定范围 | 正式角色与卡图仍缺；药水、事件尚无具体设计，不计入内容分母 |
| 发布验收 | 未开展 | 完整奖励池、爬塔、平衡、联机与 Steam Workshop 发布 |

卡牌按不同设计计数，升级和起始重复张数不另计；3／77 不是整个项目的完成率。详细状态以[设计到实现跟踪](docs/design/implementation-tracker.md)为准，构建、历史实机和未测范围分别记录在[项目状态](docs/development/status.md)。

## 人类协作看板

待回答的问题与待 Review 的交付作为 Issue 统一在[协作文档](docs/collaboration.md#协作任务)维护。点击编号填写 A 栏，也可在 CLI 按编号回答；Agent 负责整理结论、同步正式文档，并关闭和归档已解决事项。这里仅列入口，具体状态和处理记录以协作文档及其归档为准。

| 待回答 | 待 Review | 已处理 |
| --- | --- | --- |
| 暂无 | [R1 源码与脚手架拆分](docs/collaboration.md#r1) | **T01 · 共用规则确认**及 Q1～Q8 设计 Issue 均已处理；原回答与补充回答已写回设计，见[已关闭 Issue 归档](docs/collaboration.md#已关闭-issue-归档) |

设计问题关闭表示规则已明确；实现完成和人类 Review 结论分别记录。没有收到回答只影响依赖该答案的任务。

## 开发看板

待办按建议依赖顺序排列；“待确认”是阻塞原因，不等于正在开发。实际开工后才移入进行中，任务完成须写回设计条目和验证证据。

| 待办 | 进行中 | 已完成 |
| --- | --- | --- |
| **T02 · 磨顶与征召**：[T01](#人类协作看板) 已完成；按[已确认规则](docs/design/mechanics.md#术语与实现状态)实现两项机制，验收不足牌、满手牌、插入结算、触发边界及术语悬停解释 | —（当前无已开工的玩法任务） | 工程骨架、独立角色与内容池 |
| **T03 · 正式起始内容**：分步实现[两张特殊初始牌](docs/design/cards.md#初始卡组)和[初始遗物](docs/design/relics-events-potions.md#hs-relic-001)，接入角色并验收；磨顶相关部分依赖T02 | — | 打击／防御／气及升级、双语文本；有基础实机记录 |
| **T04 · 卡池填充**：把[70张攻击／技能／能力牌](docs/design/cards.md#攻击牌)按机制分批实现，先选依赖清楚的组合，再处理自动出牌、保留、费用和奖励干预；每批单独验收 | — | 构建、打包、部署与版本核对 |
| **T05 · 遗物与先古内容**：实现[其余三件遗物](docs/design/relics-events-potions.md#遗物)、[两张先古卡](docs/design/cards.md#先古卡)，按已确认规则接入获取与替换，并验证保存续玩 | — | Agent 实机工具、场景构造、截图与 Review 记录 |
| **T06 · 正式资源与内容定稿**：命名、双语文案、角色／卡牌／遗物美术、起始数值；药水和事件待有设计后再建条目 | — | 设计草稿到代码的跟踪表与进度看板 |
| **T07 · 发布验收**：奖励池、爬塔和平衡、保存续玩、联机、兼容与 Workshop 发布 | — | 模组源码与脚手架拆分、独立打包 |

看板维护方式见[日常维护](docs/design/implementation-tracker.md#日常维护)。完成一个大任务的部分内容时，拆出有编号范围的小任务，避免整批误标完成。

## 从这里开始

- 策划与 Review：[开发手册与学习路线](docs/README.md)
- 回答问题与提交评审意见：[协作问答与任务](docs/collaboration.md)
- 查原版参考牌与复用入口：[原版效果参考](.agents/skills/sts2-heavysilent-dev/references/vanilla-effects.md)
- 分类设计与源码：[特殊机制](docs/design/mechanics.md) · [卡牌](docs/design/cards.md) · [遗物／事件／药水](docs/design/relics-events-potions.md)
- 实现进度汇总：[设计到实现跟踪](docs/design/implementation-tracker.md)
- 验证结果与未测范围：[项目状态](docs/development/status.md)
- 策划入口与文档归属：[设计文档](docs/design/README.md)
- Agent 工作入口：[AGENTS.md](AGENTS.md)
- 无历史上下文接手：[首次接手指南](.agents/skills/sts2-heavysilent-dev/references/first-session.md)

## 本机启动

需要 Windows、正版游戏 `public-beta`、.NET SDK **10.0.402**、**MegaDot .NET 4.5.1-m.14**、BaseLib **3.4.7**、Python **3.11+**。推荐使用 PowerShell 7。
SDK 版本与目标框架不同：本工程用 SDK 10 编译游戏使用的 `net9.0`。
依赖与游戏版本见[版本基线](docs/development/baseline.json)；更新测试版后按[升级流程](.agents/skills/sts2-heavysilent-dev/references/build-debug.md)重新验证。

1. 将 `mod/local.props.example` 复制为 `mod/local.props`，填写游戏、引擎和已安装 BaseLib 的绝对路径；已有本机配置沿用即可。
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

从[所属设计文档](docs/design/README.md)选取条目，使用[对应的策划模板](docs/design/templates/README.md)补齐效果顺序、升级、边界和验收例子，然后让 Agent 实现；新设计先登记编号。
Markdown 是设计输入，运行时效果由 C# 与本地化 JSON 提供。
实现前先查[原版参考及调用链](.agents/skills/sts2-heavysilent-dev/references/vanilla-effects.md)，优先复用原版机制；涉及尚未解决的设计差异时处理[对应协作问题](docs/collaboration.md#协作任务)。

玩家包中的模板／占位资源许可见 [mod/THIRD_PARTY_NOTICES.md](mod/THIRD_PARTY_NOTICES.md)，测试工具等脚手架许可见[根目录第三方说明](THIRD_PARTY_NOTICES.md)。
