# local PR #<number> · 改动标题

- **State:** Open
- **Draft:** Yes / No
- **Author:** Agent
- **Reviewers:** 人类
- **Base:** 目标分支及提交
- **Head:** 工作分支及本次交付的固定提交号；修订后同步 main 中此记录
- **Worktree:** 相对主工作目录的任务路径
- **Linked issues:** Issue 链接

## Description

整个 goal 的范围、交付原因（全部完成／硬阻塞／用户明确暂停）及成果，可按功能批次组织内容，但同一 goal 不拆 PR。说明试玩方法和需要人类判断的问题；部分交付须列出剩余范围、阻塞证据与恢复条件，不标记 goal 完成。关键实现供按需深入阅读。

## Files changed

`git diff <base>...<head>`。待合入文件用 `<head>:<path>` 和 `git show <head>:<path>` 定位，避免链接到 main 尚不存在的文件或临时 worktree。

## Checks

主 Agent 技术 Review 结论、已运行检查及证据；注明未测范围。

## Timeline

### YYYY-MM-DD · Reviewer · head 提交

Comment / Approve / Request changes：评审意见。

### YYYY-MM-DD · Author · 新 head 提交

对应修改、验证结果与待复核点。实际整合后记录 Merged 和结果提交；放弃则记录 Closed 及原因。worktree 保留与清理按[执行流程](../../development/workflow.md#review整合与清理)。
