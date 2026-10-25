---
name: sts2-heavysilent-dev
description: Implement or review HeavySilent mod content, shared mechanics, build tooling, and project teaching documentation.
---

# HeavySilent 开发

服务于这个仓库的 STS2 角色模组。人类给出策划与 Review，Agent 完成程序实现、验证和解释。
模组源码集中在 `mod/`，包含独立构建所需配置和入口；本 Skill、设计文档、开发脚本及测试工具属于脚手架，只能由外层调用源码构建入口。文件边界见下方工程结构。
优先复用本机原版卡牌和 Command／Hook／Power／附魔；不确定时追查实际代码及本地化，不能凭牌名猜行为。原版机制缺失或不能满足明确设计时才添加本项目实现，并说明差异；注册等扩展使用既有 BaseLib，不为已有能力引入新的外部代码。网上示例需要核对签名和行为。
首次接手且没有会话上下文时，先看[首次接手指南](references/first-session.md)，再按本次任务读取下表中的正文。

## 按任务读取

| 当前任务 | 参考 |
| --- | --- |
| 接手评论、Issue／PR 与人类 Review | [local Issues / Pull requests](../../../docs/collaboration.md)；[Review 方法](../../../docs/development/review.md) |
| 理解代码结构、注册与资源归属 | [工程结构](references/architecture.md) |
| 解释代码、检查命令与生命周期 | [API 与教学](references/api-primer.md) |
| 查原版参考牌、调用链与可复用边界 | [原版效果参考](references/vanilla-effects.md) |
| 增加卡牌、遗物、药水或事件 | [内容填充](references/content-authoring.md) |
| 特殊机制、术语解释与共用规则 | [特殊机制设计](../../../docs/design/mechanics.md) |
| 初始卡组、角色卡牌、状态牌、诅咒牌与特殊来源卡牌 | [卡牌设计](../../../docs/design/cards.md) |
| 遗物、事件与药水的玩法 | [遗物／事件／药水设计](../../../docs/design/relics-events-potions.md) |
| 编译、打包、部署、排错、版本升级 | [构建与调试](references/build-debug.md) |
| Agent 实机测试、场景构造、截图与可选回归 | [测试工作流](references/game-testing.md) |
| 查询上游文档 | [来源索引](references/sources.md) |

设计正文统一在 [docs/design](../../../docs/design/README.md)，本 Skill 仅索引设计与维护技术工作流，不另存玩法规则。上述正文供人类与 Agent 共用；仅加载当前任务所需章节，小型文本修正不需要全量阅读。

## 落实内容

- 从[设计入口](../../../docs/design/README.md)进入对应类别，读取玩法、逐项状态、稳定 ID 与代码；[设计跟踪](../../../docs/design/implementation-tracker.md)汇总进度，[项目状态](../../../docs/development/status.md)记录验证证据，[教学样例](../../../docs/design/teaching-samples.md)解释现有结算。
- 将已有命令组合与新增机制分开。未在 CLI 解决的问题集中登记到[协作页](../../../docs/collaboration.md)，不限于玩法；设计条目只留编号链接。只追问影响当前工作的事项，其他工作继续推进。
- 接手时检查 local Issue／PR 的 Timeline 和当前对话，已有明确答案直接记录并处理；技术事实先自行查证。确认后的玩法写回设计，工程约定写回技术参考，再同步状态、活动与归档索引、人类看板。编号、Parent issue、Review 和实际合并沿用协作入口约定，不以测试通过代替人类 Review。
- 保持卡牌效果顺序可读，使用游戏 Command、玩家选择上下文和游戏 RNG。
- 新机制先核对原版类似实现及其底层调用，区分可直接复用的操作与本项目真正缺少的规则；必要补丁单独说明目标方法、版本与现有入口不足之处。原版参考与明确设计有差异时，记录差异并保留人类答案，不静默改玩法。
- 教学解释连接策划语句、源码和可观察行为，不只描述语法。

## 交付与维护

- 按变更范围运行相关验证。构建、资源包验证、实际游戏验证分别报告。
- 实机测试由当前改动驱动：先列问题、预期和理由，再构造场景、实际操作、观察和记录。固定回归按相关性选用，不默认重跑整套打击／防御／气。
- 内容变更更新所属设计文档中的条目，再同步设计跟踪、README 看板与状态页验证证据。机制规则直接更新设计目录中的特殊机制正文；Skill 维护索引，命令或环境变更更新构建参考。
- 同一事实只保留一份正文，其他入口链接它；不要把会话日志写进 Skill。
- 适用版本看[基线](../../../docs/development/baseline.json)，不是凭 Skill 的更新时间推断兼容。
- 人类 Review 需要行为变化、关键源码、验证证据和未完成项；沿用[Review 指引](../../../docs/development/review.md)。
