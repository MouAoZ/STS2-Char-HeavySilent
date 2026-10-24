# HeavySilent 项目入口

这是 STS2 的独立角色模组；目标分支是最新 public-beta。
人类负责策划与 Review，Agent 负责实现、验证和让人类读懂代码。

## 按任务查阅

- 实现、调试或评审本项目：使用 [sts2-heavysilent-dev](.agents/skills/sts2-heavysilent-dev/SKILL.md)。
- 人类学习与 Review：[docs/README.md](docs/README.md)。
- 内容意图：[设计草稿](docs/design/设计草稿v0.1.md)；逐项状态与代码看[设计跟踪](docs/design/implementation-tracker.md)，总览与看板看[README](README.md)，验证证据看[项目状态](docs/development/status.md)。
- 环境变更：核对[版本基线](docs/development/baseline.json)及本机程序集。
- Agent 实机测试与可选回归：[测试工作流](.agents/skills/sts2-heavysilent-dev/references/game-testing.md)。

## 快速命令

```powershell
.\scripts\dev.ps1 doctor
.\scripts\dev.ps1 verify
.\scripts\dev.ps1 package
.\scripts\dev.ps1 deploy
```

## 项目约定

- 说明和教学用中文，代码标识符用英文；内容 ID 一旦进入存档就保持稳定。
- 设计歧义写入待澄清项，只询问影响当前实现的规则，不静默决定玩法。
- 效果通过游戏 Command／Hook 实现；随机和玩家选择沿用游戏上下文。
- 初始套牌与角色素材目前是开发占位，磨顶与征召尚未实现。
- 构建仅写工作区产物；部署仅操作本模组及独立测试桥目录，不覆盖原版程序集。
- 机制、接口、命令或版本约定改变时，在同一次交付中更新对应参考文档。
- Review 交付说明行为变化、关键实现、验证证据和未完成项；实机未验证就明确标记。
