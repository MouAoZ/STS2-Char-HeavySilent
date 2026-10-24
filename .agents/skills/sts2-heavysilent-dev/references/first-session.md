# 首次接手：从仓库恢复工作上下文

本页供没有历史聊天记录的 Agent 使用。目标是找到当前事实、选择本次任务所需资料，然后开始工作；不需要先读完整个文档目录。

## 最短阅读路径

1. 根目录 [AGENTS.md](../../../../AGENTS.md) → [本 Skill](../SKILL.md)：工作约定与资料路由。
2. [README 总览与看板](../../../../README.md#项目进度)、[设计跟踪](../../../../docs/design/implementation-tracker.md)、[项目状态](../../../../docs/development/status.md)：依次定位开发阶段、逐项实现与代码、实机证据。历史验证不能代替本次核对。
3. [工程结构](architecture.md)：定位本次要改的源码；新接触 STS2 时再读 [API 与第一张卡](api-primer.md)。
4. 按任务补读：新增内容读[填充教程](content-authoring.md)；涉及磨顶／征召读[机制语义](../../../../docs/design/mechanics.md)；运行游戏读[测试工作流](game-testing.md)。

当前是独立角色的开发骨架，使用静默占位表现。[设计跟踪](../../../../docs/design/implementation-tracker.md)提供统计和分类入口，逐项设计、实现状态、稳定 ID 与代码维护在所属设计文档。设计中出现一张卡，并不代表它已实现；正式初始牌、完整奖励卡池、磨顶与征召以逐项状态为准。

## 不同问题去哪里找答案

| 需要确认的事实 | 来源 |
| --- | --- |
| 人类想要的玩法 | 当前明确指令与[三类设计正文](../../../../docs/design/README.md)；共用规则直接读[特殊机制设计](../../../../docs/design/mechanics.md)，归档草稿只供历史追溯；冲突或歧义需在相关实现前澄清 |
| 代码目前实际做什么 | `mod/HeavySilentCode/`、`mod/HeavySilent/` 的双语资源与本次实机观察；状态页提供定位和历史结论 |
| 内容的稳定编号与模型 ID | 所属[设计文档](../../../../docs/design/README.md)与源码 `CustomID`；编号约定见[设计跟踪](../../../../docs/design/implementation-tracker.md)，新增时登记，重命名显示名不换 ID |
| 哪些 API 能用 | [基线](../../../../docs/development/baseline.json)、本机程序集和已编译实例；网上文档提供方向，具体签名核对本机 |
| 下一步应做什么 | 用户当前任务和 [README 看板](../../../../README.md#开发看板)；条目依赖见设计跟踪，机制待确认项不是可以自行补全的规则 |

遗物、药水、事件目前有策划模板和实现路线，尚无本项目已编译的完整样例。首次实现时仍需核对对应原版 Model、Hook、注册与资源要求，完成后再把经验补回教程。

## 同一台机器的新会话

先运行 `git status --short`，识别已有改动，避免覆盖其他人的工作。沿用已有 `mod/local.props`、工具缓存和 MCP 配置；不要因为会话是新的就重新初始化工程或清空测试目录。

```powershell
& 'C:\Program Files\PowerShell\7\pwsh.exe' -NoProfile -File .\scripts\dev.ps1 doctor
python scripts/verify.py
```

`doctor` 核对环境；直接运行 `verify.py` 仅查源码、内容和文档，不编译。需要编译时运行 `dev.ps1 verify`。选择验证范围见[Review 指引](../../../../docs/development/review.md)，不因接手会话就默认开游戏跑全部回归。

要测游戏时先确认 MCP 工具是否实际加载、是否有受管游戏会话。`get_state` 缺少 `active-session.json` 通常意味着尚未启动受管会话；由 `start_session` 启动。`get_report` 在没有历史报告的新克隆中也没有可读内容。工具和游戏状态的排错见[测试工作流](game-testing.md)。

## 全新克隆或换机器

仓库不携带游戏、BaseLib 安装、SDK、引擎、Python 环境或个人存档。按[构建环境](build-debug.md)准备依赖，复制 `mod/local.props.example` 为 `mod/local.props` 后填写本机路径；样例中的路径不能保证在其他机器存在。只需要源码打包时使用该指南的独立入口，无需复制外层脚手架。

先完成 `doctor`、`verify`、`package`。需要实机测试时再部署并启用 BaseLib／HeavySilent，执行 `test-setup` 生成独立测试桥、仓库内 Python 和本机 MCP 配置。项目移动路径后同样需要刷新 MCP 配置。没有游戏安装时只能完成源码／文档检查，明确记录实机未验证。

以下信息不随 Git 提交：

| 本机信息 | 如何得到／如何对待 |
| --- | --- |
| `mod/local.props` | 从样例创建，保存真实依赖路径 |
| `.codex/config.toml`、`.tools/`、测试 `.venv/` | `test-setup` 生成；安装依赖首次需要网络 |
| 根及 `mod/` 内的 `.cache/`、`.godot/`、`artifacts/` | 按需重新生成；不当作源码来源 |
| `artifacts/game-tests/` | 历史日志、截图、个人存档备份及会话记录；新克隆中不会自动出现 |
| `profile3-owner.json`、`active-session.json` | 位于上述测试目录，标记测试档归属和待清理会话；不能当普通缓存随意删除 |

状态页给出的旧证据路径是本机历史索引。另一台机器找不到这些文件不代表测试已失败，也不能据此声称自己已复现。新的验证应生成自己的报告。

## 完成一次任务

实现前记录涉及的设计与歧义；实现后按改动选择构建、资源或实机检查。需要实机时，由 Agent 提出问题、构造场景、实际操作和记录判断；测试夹具只安排前提，不代替被测机制。

将新确认的规则写回特殊机制设计，将条目状态与代码写回所属设计文档，将统计写回设计跟踪，将整体进度与任务写回 README，将验证结果写回状态页。Skill 只索引设计正文。交付给人类的说明包括行为变化、关键源码、验证证据和未测范围；完成本地 Git commit，交付时给出提交号。后续 Agent 应能从这些文件接手，不需要依赖旧聊天或被忽略的临时脚本。
