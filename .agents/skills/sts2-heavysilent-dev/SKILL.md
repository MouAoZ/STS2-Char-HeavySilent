---
name: sts2-heavysilent-dev
description: Implement or review HeavySilent mod content, shared mechanics, build tooling, and project teaching documentation.
---

# HeavySilent 开发

遵守仓库 [AGENTS.md](../../../AGENTS.md)。首次接手读[接手指南](references/first-session.md)；开始修改或续做目标读[执行流程](../../../docs/development/workflow.md)，其中维护 worktree／直接 main、按需委派、技术验收与功能批次 PR 的约定。

只加载当前任务需要的资料。玩法与逐项状态以[设计正文](../../../docs/design/README.md)为准，技术参考不另存玩法或进度副本。

## 按任务读取

| 当前任务 | 参考 |
| --- | --- |
| 领取设计条目、更新进度 | [设计跟踪](../../../docs/design/implementation-tracker.md)；[项目状态](../../../docs/development/status.md) |
| 接手问题、评论与 Review | 主工作目录 main 的 [local Issues / Pull requests](../../../docs/collaboration/README.md)；[Review 指引](../../../docs/development/review.md) |
| 定位源码、注册与资源 | [工程结构](references/architecture.md) |
| 理解调用链、向人类解释代码 | [API 与教学](references/api-primer.md)；[教学样例](../../../docs/design/teaching-samples.md) |
| 查原版效果与可复用边界 | [原版效果参考](references/vanilla-effects.md) |
| 增加卡牌、遗物、药水或事件 | [内容填充](references/content-authoring.md) |
| 特殊机制、术语与共用规则 | [特殊机制设计](../../../docs/design/mechanics.md) |
| 卡牌与初始卡组 | [卡牌设计](../../../docs/design/cards.md) |
| 遗物、事件与药水 | [其他内容设计](../../../docs/design/relics-events-potions.md) |
| 构建、部署、版本与排错 | [构建与调试](references/build-debug.md) |
| 实机测试与可选回归 | [测试工作流](references/game-testing.md) |
| 查询上游资料 | [来源索引](references/sources.md) |

API 不确定时追查本机原版同类效果、底层命令和本地化；网上示例需核对本机签名。原版与设计有差异时说明复用边界，确需补丁则说明目标方法、版本及现有入口不足之处。教学解释连接策划语句、关键源码和可观察行为。

Skill 只维护路由与必要技术约定。规则变化修改所属正文；删去重复、过时说明，不把调查日志或一次性交付记录追加到 Skill。适用版本以[基线](../../../docs/development/baseline.json)为准。
