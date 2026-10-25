# local PR #<number> · 改动标题

- **State:** Open
- **Draft:** Yes / No
- **Author:** Agent
- **Reviewers:** 人类
- **Base:** 目标分支及提交
- **Head:** 工作分支；当前提交用 `git rev-parse <branch>` 查询，交付消息及 Review 注明实际提交号
- **Worktree:** 相对主工作目录的任务路径
- **Linked issues:** Issue 链接

## Description

解决的问题、具体行为变化和关键实现。

## Files changed

`git diff <base>...<head>`，以及需要重点阅读的文件链接。

## Checks

已运行的检查、结果与证据；注明未测范围。

## Timeline

### YYYY-MM-DD · Reviewer · head 提交

Comment / Approve / Request changes：评审意见。

### YYYY-MM-DD · Author · 新 head 提交

对应修改、验证结果与待复核点。实际整合后记录 Merged 和结果提交；放弃则记录 Closed 及原因。worktree 保留与清理按[执行流程](../../development/workflow.md#review整合与清理)。
