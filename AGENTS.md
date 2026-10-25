# HeavySilent 项目入口

这是 STS2 的独立角色模组；目标分支是最新 public-beta。
人类负责策划与 Review，Agent 负责实现、验证和让人类读懂代码。

`mod/` 是可独立构建打包的模组源码边界；文档、设计、Skill、开发脚本和测试工具属于外层脚手架。新增构建依赖不得让 `mod/` 反向依赖这些脚手架；本机路径配置为 `mod/local.props`。

## 按任务查阅

- 实现、调试或评审本项目：使用 [sts2-heavysilent-dev](.agents/skills/sts2-heavysilent-dev/SKILL.md)。
- 人类学习与 Review：[docs/README.md](docs/README.md)。
- 人类待办、未解决的问题与 Review 意见：[协作文档](docs/collaboration.md)。接手时检查新回答，并将结论写回正式文档。
- 内容意图与逐项状态：[设计入口](docs/design/README.md)，按特殊机制、卡牌、遗物／事件／药水分别维护；汇总看[设计跟踪](docs/design/implementation-tracker.md)，总览与看板看[README](README.md)，验证证据看[项目状态](docs/development/status.md)。
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
- 设计歧义及其他未解决沟通集中写入协作文档，设计和看板只作索引；只追问影响当前工作的事项，不静默决定玩法。已有明确回答直接记录并处理，不重复求确认。
- 效果通过游戏 Command／Hook 实现；随机和玩家选择沿用游戏上下文。
- 优先查本机原版卡牌与结算代码并复用现有实现；只有原版机制缺失或无法满足明确设计时才添加独有实现，不为已有能力引入新的外部代码。参照原版的调查结果见[效果参考](.agents/skills/sts2-heavysilent-dev/references/vanilla-effects.md)。
- 初始套牌与角色素材目前是开发占位，磨顶与征召尚未实现。
- 构建仅写工作区产物；部署仅操作本模组及独立测试桥目录，不覆盖原版程序集。
- 机制、接口、命令或版本约定改变时，在同一次交付中更新对应参考文档。
- Review 交付说明行为变化、关键实现、验证证据和未完成项；实机未验证就明确标记。
