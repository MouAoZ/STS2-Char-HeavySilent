# HeavySilent 项目入口

这是 STS2 的独立角色模组；目标分支是最新 public-beta。
人类负责策划与 goal 级 Review；主 Agent 负责 goal 内所有更细粒度的 Review、实现、验证和必要的代码解释。

`mod/` 是可独立构建打包的模组源码边界；文档、设计、Skill、开发脚本和测试工具属于外层脚手架。新增构建依赖不得让 `mod/` 反向依赖这些脚手架；本机路径配置为 `mod/local.props`。

## 按任务查阅

- 实现、调试或评审本项目：使用 [sts2-heavysilent-dev](.agents/skills/sts2-heavysilent-dev/SKILL.md)。
- 执行目标、worktree 与 PR：[执行流程](docs/development/workflow.md)。一个 `/goal` 只交付一个 PR，在完成整个目标、硬阻塞或用户明确暂停时提交；内部管理由主 Agent 决定。用户可指定本次直接 main、不走 PR。
- 人类学习与 Review：[docs/README.md](docs/README.md)。
- 问题、回答与评审：统一读取主工作目录 main 的 [local Issues / Pull requests](docs/collaboration/README.md)，不把 worktree 内旧副本当作最新记录。协作更新及时写入 main；已有答案直接落实，完成后关闭并归档。
- 内容意图与逐项状态：[设计入口](docs/design/README.md)；汇总看[设计跟踪](docs/design/implementation-tracker.md)，总览与看板看 [README](README.md)，验证证据看[项目状态](docs/development/status.md)。
- 环境变更：[版本基线](docs/development/baseline.json)及本机程序集；实机测试见[测试工作流](.agents/skills/sts2-heavysilent-dev/references/game-testing.md)。

## 快速命令

```powershell
.\scripts\dev.ps1 doctor
.\scripts\dev.ps1 verify
.\scripts\dev.ps1 package
.\scripts\dev.ps1 deploy
```

## 项目约定

- 说明和教学用中文，代码标识符用英文；内容 ID 一旦进入存档就保持稳定。
- 玩法歧义集中登记 local Issue，只阻塞依赖该答案的工作；不静默决定玩法。
- 优先查本机原版效果并复用 Command／Hook、玩家选择与 RNG；只有原版机制无法满足设计时才添加独有实现。参照[原版效果参考](.agents/skills/sts2-heavysilent-dev/references/vanilla-effects.md)，不为已有能力引入新的外部代码。
- 构建仅写工作区产物；部署仅操作本模组及独立测试桥目录，不覆盖原版程序集。
- 机制、接口、命令或版本约定改变时，同次更新对应参考文档；实机未验证就明确标记。
