---
name: sts2-heavysilent-dev
description: Implement or review HeavySilent mod content, shared mechanics, build tooling, and project teaching documentation.
---

# HeavySilent 开发

服务于这个仓库的 STS2 角色模组。人类给出策划与 Review，Agent 完成程序实现、验证和解释。
优先复用本机目标版本的游戏接口与 BaseLib；网上示例需要核对签名和行为。
首次接手且没有会话上下文时，先看[首次接手指南](references/first-session.md)，再按本次任务读取下表中的正文。

## 按任务读取

| 当前任务 | 参考 |
| --- | --- |
| 理解代码结构、注册与资源归属 | [工程结构](references/architecture.md) |
| 解释代码、检查命令与生命周期 | [API 与教学](references/api-primer.md) |
| 增加卡牌、遗物、药水或事件 | [内容填充](references/content-authoring.md) |
| 磨顶、征召、气及相关触发链 | [机制语义](references/mechanics.md) |
| 编译、打包、部署、排错、版本升级 | [构建与调试](references/build-debug.md) |
| Agent 实机测试、场景构造、截图与可选回归 | [测试工作流](references/game-testing.md) |
| 查询上游文档 | [来源索引](references/sources.md) |

这些正文同时供人类阅读。仅加载当前任务所需章节；小型文本修正不需要全量阅读。

## 落实内容

- 通过[设计跟踪](../../../docs/design/implementation-tracker.md)定位意图、逐项状态、稳定 ID 与代码，通过[项目状态](../../../docs/development/status.md)核对验证证据；[教学样例](../../../docs/design/teaching-samples.md)解释现有结算。
- 将已有命令组合与新增机制分开。只询问影响当前实现的设计歧义；其余条目标记待确认。
- 保持卡牌效果顺序可读，使用游戏 Command、玩家选择上下文和游戏 RNG。
- 新机制先核对原版类似实现；必要补丁单独说明目标方法和版本。
- 教学解释连接策划语句、源码和可观察行为，不只描述语法。

## 交付与维护

- 按变更范围运行相关验证。构建、资源包验证、实际游戏验证分别报告。
- 实机测试由当前改动驱动：先列问题、预期和理由，再构造场景、实际操作、观察和记录。固定回归按相关性选用，不默认重跑整套打击／防御／气。
- 内容变更更新设计跟踪、README 进度与看板、状态页验证证据；维护步骤见跟踪表。机制变更更新本 Skill 的机制参考；命令或环境变更更新构建参考。
- 同一事实只保留一份正文，其他入口链接它；不要把会话日志写进 Skill。
- 适用版本看[基线](../../../docs/development/baseline.json)，不是凭 Skill 的更新时间推断兼容。
- 人类 Review 需要行为变化、关键源码、验证证据和未完成项；沿用[Review 指引](../../../docs/development/review.md)。
