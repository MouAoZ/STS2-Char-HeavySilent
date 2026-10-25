# local Issues / Pull requests

人类与 Agent 在本仓库文档中使用 **local Issue / PR** 协作。`local #31` 表示本地记录；GitHub Issue／PR 使用实际仓库链接，两个编号空间独立。

采用 GitHub 的 [Issue](https://docs.github.com/en/issues/tracking-your-work-with-issues/learning-about-issues/quickstart)、[sub-issue](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/adding-sub-issues)、[PR](https://docs.github.com/en/pull-requests/reference/pull-requests) 和 [Review](https://docs.github.com/en/pull-requests/reference/pull-request-reviews) 术语。Issue、sub-issue 和 PR 共用一条递增整数序列；当前最后使用 **local #55**，下一个为 **local #56**。

## Issues

| Number | Title | State | Assignees |
| --- | --- | --- | --- |
| [local #54](issues/54.md) | 评审模组源码与脚手架边界 | Open | 人类 |

## Pull requests

| Number | Title | State | Reviewers |
| --- | --- | --- | --- |
| [local #55](pull-requests/55.md) | 精简 Skill 并接入 worktree 目标执行流程 | Open | 人类 |

## Archive

53个 Closed Issue，0个已终结 PR。完整记录见[归档索引](archive.md)。

## 参与讨论

打开对应 Issue／PR，在 Timeline 末尾按“日期 · 作者 · 内容”追加评论，也可以在 CLI 中回复“local #31：……”。Agent 负责记录 CLI 评论、处理结论、更新设计和看板。技术事实先由 Agent 调查，需要人类决定的内容才分配给人类。

人类可以直接回复、提出修改或说明暂缓原因；同一条目支持多轮讨论。PR Review 使用 Comment、Approve 或 Request changes，并注明所评 head 提交。暂缓属于工作安排，记录原因即可。

## 工作流

- **Issue：** 描述问题或任务及完成条件；可填写 Assignees、Labels、关联提交或 PR。决定得到落实后 Closed (Completed)，明确不再开展时 Closed (Not planned) 并说明原因。设计决策完成与游戏内容实现分别跟踪。
- **Sub-issue：** 作为一个 Issue 分配自己的整数编号，通过 Parent issue 和父项的 Sub-issues 列表关联；独立记录状态和评论。普通追问追加在原 Timeline 中。
- **Comments：** 记录作者、日期和内容；调查、回复、修正继续追加。需要继续处理已关闭目标时 Reopen 原 Issue，保留之前的关闭记录；新的独立目标另建 Issue。
- **PR：** 提出一组待整合改动，记录 base／head 分支及提交、关联 Issue、diff、Checks 和 Review。修订后更新 head 和验证证据，在同一 PR 内继续评审。Approve 与 Merged 分别记录；实际整合后记 Merged 和结果提交，放弃时记 Closed 及原因。
- **Archive：** 同次交付中将已关闭 Issue、Merged／Closed PR 从活动列表移入归档索引；正文文件保持原位置。Reopen 后恢复活动索引。同一记录只有一份正文。
- **维护：** 收到评论后落实到正式设计或技术文档，再同步状态、关联条目和 README 看板。目标拆分、worktree、按需委派、Review 后整合与清理见[执行流程](../development/workflow.md)。

## 文件与模板

| 内容 | 位置 |
| --- | --- |
| Issue 及 sub-issue | `docs/collaboration/issues/<number>.md` |
| PR | `docs/collaboration/pull-requests/<number>.md` |
| 已关闭记录索引 | [Archive](archive.md) |
| Issue 模板 | [Issue](templates/issue.md) |
| PR 模板 | [Pull request](templates/pull-request.md) |

新建前核对已登记 worktree 及待合入分支中的 Issue／PR 最大编号，再取下一个整数并同步本页。正式玩法见[设计入口](../design/README.md)，Review 检查方法见[Review 指引](../development/review.md)，开发任务见 [README 看板](../../README.md#开发看板)。
