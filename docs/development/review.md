# 人类 Review 指引

人类以整个 goal 为 Review 边界，主 Agent 代为完成 goal 内全部更细粒度的 Review。一个 goal 只交付一个 PR，触发时机、直接 main 的例外与清理统一按[执行流程](workflow.md)。

| 责任 | 关注点 |
| --- | --- |
| 主 Agent 内部 Review | 任务、功能批次及实现细节的验收；检查后继续推进，内部安排不需人类批准 |
| 人类 goal Review | 整个目标的成果；硬阻塞／暂停时检查部分成果、剩余范围和恢复条件，代码供按需深入阅读 |

复杂机制或疑点需要独立视角时才增加 Agent 检查；自查不称作独立 Review。技术事实由 Agent 先调查，未明确的玩法问题及时通过 local Issue 澄清。

## 在哪里提出与回答

从主目录 main 的[协作入口](../collaboration/README.md)打开 local PR，无需切换分支。在 Timeline 追加 Comment／Approve／Request changes 并注明所评 head，也可在 CLI 按编号回复。Agent 处理修改并补充验证；状态和归档沿用[协作约定](../collaboration/README.md#工作流)。

Issue 跟踪问题和工作，PR 展示待整合改动。已提交内容的评审也可由 Issue 跟踪，例如 [local #54](../collaboration/issues/54.md)。实现状态、Checks、人类 Review 与实际 Merged 分别记录。

## 一次交付应说明

1. goal 范围及本次交付原因（全部完成／硬阻塞／用户暂停），按功能批次整理变化和试玩方法。
2. 验证结果与证据、未测范围；编译与实机分开报告。
3. 未完成范围、阻塞或恢复条件，以及需要人类判断的问题、临时占位、旧存档影响和相关 Issue。
4. 供按需阅读的关键源码和原版参考；新增机制或补丁说明现有能力不足、目标方法与版本。参照[原版效果参考](../../.agents/skills/sts2-heavysilent-dev/references/vanilla-effects.md)。

## 主 Agent 的卡牌技术检查

- 费用、目标、升级、关键词、效果顺序与失败行为是否符合设计。
- 数值是否通过 DynamicVar 与描述共用；命令、选择上下文和 RNG 是否正确。
- 状态属于哪个玩家、卡牌实例、回合或战斗；何时重置和保存。
- 自动打出、复制、奖励修改等受影响边界是否有相应验证。
- 模型 ID 是否保持稳定；显示名变化不应改变存档 ID。

## 文档与交接

设计和代码映射写回所属[设计文档](../design/README.md)，状态与汇总按[跟踪约定](../design/implementation-tracker.md#日常维护)更新；验证证据及未测范围进[状态页](status.md)，讨论与评审留在 Issue／PR。共用规则只在特殊机制正文维护，Skill 仅索引。

API、命令或版本改变时更新对应技术参考；仅修拼写不要求重写所有文档。原始日志、机器路径和存档留在忽略目录，关键结论必须写入仓库；缺少历史证据时不能声称已复现。
