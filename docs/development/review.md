# 人类 Review 指引

先检查玩法与实际行为，再看实现是否容易维护。任务 worktree、提交、Approve 后整合与清理统一按[执行流程](workflow.md)。

## 在哪里提出与回答

从[协作入口](../collaboration/README.md)打开 local PR，在 Timeline 追加 Comment／Approve／Request changes 并注明所评 head，也可在 CLI 按编号回复。Agent 处理修改并补充验证；状态和归档沿用[协作约定](../collaboration/README.md#工作流)。

Issue 跟踪问题和工作，PR 展示待整合改动。已提交内容的评审也可由 Issue 跟踪，例如 [local #54](../collaboration/issues/54.md)。实现状态、Checks、人类 Review 与实际 Merged 分别记录。

## 一次交付应说明

1. 对应设计编号、玩家可观察的行为变化。
2. 原版参考及复用入口；新增机制或 Harmony 补丁说明现有能力不足、目标方法、版本与影响范围。查阅[原版效果参考](../../.agents/skills/sts2-heavysilent-dev/references/vanilla-effects.md)。
3. 关键源码入口及必要的中文解释，帮助人类连接设计与结算。
4. 验证情形、结果、证据及未测范围；编译与实机分开报告。
5. 临时占位、旧存档影响及未解决的 local Issue／PR。

## 看卡牌时重点检查

- 费用、目标、升级、关键词、效果顺序与失败行为是否符合设计。
- 数值是否通过 DynamicVar 与描述共用；命令、选择上下文和 RNG 是否正确。
- 状态属于哪个玩家、卡牌实例、回合或战斗；何时重置和保存。
- 自动打出、复制、奖励修改等受影响边界是否有相应验证。
- 模型 ID 是否保持稳定；显示名变化不应改变存档 ID。

## 文档与交接

设计和代码映射写回所属[设计文档](../design/README.md)，状态与汇总按[跟踪约定](../design/implementation-tracker.md#日常维护)更新；验证证据及未测范围进[状态页](status.md)，讨论与评审留在 Issue／PR。共用规则只在特殊机制正文维护，Skill 仅索引。

API、命令或版本改变时更新对应技术参考；仅修拼写不要求重写所有文档。原始日志、机器路径和存档留在忽略目录，关键结论必须写入仓库；缺少历史证据时不能声称已复现。
