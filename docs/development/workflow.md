# 目标执行与 worktree

本页维护项目执行约定。简短目标即可启动，例如 `/goal 实现 implementation-tracker.md 中的未实现项`；Agent 从[设计跟踪](../design/implementation-tracker.md)进入当前正文，接续已有进行中项，按依赖推进。不自动添加尚无设计的内容、美术或发布任务。

## 开工与续做

- 先检查 `git status --short`、`git worktree list --porcelain` 和相关 local Issue／PR。已有目标、Review 修订或未完成分支优先在原 worktree 续做；纯阅读与评估不必新建。
- 修改默认使用独立任务分支与 worktree，基线为目标分支的当前提交，通常是 `main`。用户明确说“本次直接在 main 工作，不走 PR”时按该指令执行，仍需验证、提交和回写状态；该例外不改变以后任务的默认方式。
- 新任务目录放在主工作目录的 `.cache/worktrees/<task>`（已忽略），分支用 `agent/<task>`。从主目录执行 `git worktree add .cache/worktrees/<task> -b agent/<task> <base>`；实现及相关验证在该 worktree 进行。
- 协作记录在主工作目录 main 维护，属于隔离开发的固定例外；同步与编号规则见“main 中的协作记录”。保留各目录已有改动，只提交本次任务范围。
- 本机配置、MCP 及实机状态的准备见[接手指南](../../.agents/skills/sts2-heavysilent-dev/references/first-session.md#本机配置与-worktree)。不把整个缓存、虚拟环境或存档复制进新 worktree。

## 拆分与执行

主 Agent 按依赖自行划分任务和功能批次；大型目标用 local Issue 记录范围与完成条件，必要时拆 sub-issue。这些都是 goal 内部安排，包含功能批次在内的所有更细粒度 Review 均授权主 Agent 代为完成，不请求人类逐项或逐批批准。

实际开工后更新条目为“进行中”；实现先核对原版效果与可复用入口。依赖该批次的后续内容在共用接口明确后推进。每批记录设计编号、实际代码、相关验证、剩余工作及阻塞链接；状态与统计遵守[跟踪约定](../design/implementation-tracker.md#日常维护)。

主 Agent 可按需委派 subagent，用于隔离会产生大量细节的调查、实现或检查；能顺手完成且不会挤占主上下文的工作直接完成。没有固定数量或必须委派的阶段。委派时只给任务所需背景，要求返回结论、文件位置、证据与未决事项。

goal 内的任务、分支、worktree 与委派方式由主 Agent 自主管理，最终汇总到一个交付 head；实现代码在最终 PR 获批后合入 main。subagent 默认共用工作目录，确有需要时再隔离；协调重叠文件和实机操作。主 Agent 核对实际 diff、边界及验证证据，不能把 subagent 完成当作通过；需要第二视角时才增加检查，自查不记为独立审查。

遇到技术疑问先查证；玩法歧义沿用[local Issue 流程](../collaboration/README.md#工作流)，继续无阻塞内容。只有无法自主排除、确需外部输入或环境变化，且没有其他可推进工作的情况才作为整个 goal 的硬阻塞。暂停必须来自用户明确指令，不能因完成一个批次而自行暂停。

## main 中的协作记录

`docs/collaboration/` 的 Issue／PR 正文、活动及归档索引与 README 人类看板在主工作目录 main 维护并纳入 Git。分配编号、新建请求、收到评论或改变状态时，由主 Agent 及时直接更新 main，以独立协作提交保存；这不代表实现代码已合入，也不要求为协作更新再开 PR。编号仅从 main 的记录分配。

任务 worktree 以 main 的记录为准，不另写一套 Timeline。需要引用新增记录或同步进展时，将 main 合入任务分支，再处理可能的冲突；保留 main 最新评论和状态。实现进度、源码与技术文档仍随任务分支评审，main 人类看板只展示待办和 PR，不提前宣称功能已合入。

交付前确认 main 的入口已经列出该 PR，并在回复中链接主目录的文件。待合入文件在 PR 中用 `<head>:<path>` 和 `git show <head>:<path>` 定位；不创建指向 main 尚不存在文件或待清理 worktree 的持久链接。模板和工作流本身的改动仍随功能分支交付。

## 提交与 local PR

**一个 `/goal` 只交付一个 [local PR](../collaboration/templates/pull-request.md)**，默认不创建远端 GitHub PR。仅在整个目标完成、遇到硬阻塞或用户明确暂停时，在 main 提交该 PR；即使包含多个功能批次，也一起交付。工作中以 Issue、commit 和状态记录进展，不用中间 PR 要求人类验收。

PR 记录目标、交付原因、Base／Head 实际提交、汇总 diff、Agent Review 与 Checks。用功能批次组织变化和证据，便于阅读，但不拆 PR。列出试玩方法、未测范围及需要人类判断的问题；硬阻塞／暂停交付还须明确剩余范围、阻塞证据与恢复条件，保留未完成状态。

完整实现须覆盖整个 goal 的代码、配套内容和必要验证；部分批次完成、提交 PR 或部分成果获批均不等于 goal 完成。会话中断或预算限制记录续做状态，不冒充完成或用户暂停。修订或恢复工作时沿用尚未合并的同一 PR；部分成果已合并后，剩余项仍未完成，按后续用户指令续做，不自动为同一 goal 再开第二条 PR。

明确指定直接 main 且不走 PR 的任务，按该次指令交付提交号与验证结果。

## Review、整合与清理

以下适用于默认 PR 流程；明确指定直接 main、不走 PR 的任务按该次指令交付。

- 人类在 PR Timeline 或 CLI 给出 Comment／Approve／Request changes，并注明所评 head。未明确结论时保持 Open；Request changes 后在原 worktree 修改、验证、更新 head 并请求复审。
- 本项目默认以人类对所评 head 的 Approve 作为整合及后续清理的授权。新增实质改动需再次 Review；仅追加评审记录与归档索引不使批准失效。
- 整合前核对目标分支变化和相关检查。发生冲突时在任务 worktree 解决并验证；解决导致实质改动时重新提交 Review。随后将批准的改动合入目标分支，记录实际整合提交，更新 Merged、关联 Issue 和 Archive。
- PR 通过并整合后，由主 Agent 清理这个 goal 所属且已无用途的分支／worktree，包含内部试验目录；保留交付提交、记录和所需证据。先核对归属、绝对路径、未保存工作及测试会话，再用 Git 清理，不影响其他任务。未合入但确定弃用的试验内容在目标记录中说明，不使用强制删除掩盖未处理成果。
- 测试会话未结束、证据未保存或 PR 未通过时保留 worktree。放弃任务沿用 Closed 流程，保留原因与提交；不把未获批准的工作视作可清理。

Review 关注内容及文档职责见[Review 指引](review.md)，Issue／PR 的术语、评论和归档见[协作入口](../collaboration/README.md#工作流)。
