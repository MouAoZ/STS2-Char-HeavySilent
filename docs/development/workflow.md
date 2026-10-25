# 目标执行与 worktree

本页维护项目执行约定。简短目标即可启动，例如 `/goal 实现 implementation-tracker.md 中的未实现项`；Agent 从[设计跟踪](../design/implementation-tracker.md)进入当前正文，接续已有进行中项，按依赖推进。不自动添加尚无设计的内容、美术或发布任务。

## 开工与续做

- 先检查 `git status --short`、`git worktree list --porcelain` 和相关 local Issue／PR。已有目标、Review 修订或未完成分支优先在原 worktree 续做；纯阅读与评估不必新建。
- 修改默认使用独立任务分支与 worktree，基线为目标分支的当前提交，通常是 `main`。主工作目录保留为整合入口；不夹带其已有改动。
- 新任务目录放在主工作目录的 `.cache/worktrees/<task>`（已忽略），分支用 `agent/<task>`。从主目录执行 `git worktree add .cache/worktrees/<task> -b agent/<task> <base>`；此后的编辑、验证和提交均在该 worktree。
- Issue／PR 正文随任务分支提交。主目录只显示已合入的内容；新会话扫描已登记 worktree 的协作入口找到待 Review 工作。编号由主 Agent 在已登记 worktree 和待合入任务分支中核对后统一分配，避免重复。
- 本机配置、MCP 及实机状态的准备见[接手指南](../../.agents/skills/sts2-heavysilent-dev/references/first-session.md#本机配置与-worktree)。不把整个缓存、虚拟环境或存档复制进新 worktree。

## 拆分与执行

主 Agent 读取相关设计、依赖和已确认评论，选择可独立验证的批次。大型目标用 local Issue 记录范围与完成条件，必要时拆 sub-issue；小改动可直接交付 PR，不为每张卡或每次工具调用创建条目。

实际开工后更新条目为“进行中”；实现先核对原版效果与可复用入口。依赖该批次的后续内容在共用接口明确后推进。每批记录设计编号、实际代码、相关验证、剩余工作及阻塞链接；状态与统计遵守[跟踪约定](../design/implementation-tracker.md#日常维护)。

主 Agent 可按需委派 subagent，用于隔离会产生大量细节的调查、实现或检查；能顺手完成且不会挤占主上下文的工作直接完成。没有固定数量或必须委派的阶段。委派时只给任务所需背景，要求返回结论、文件位置、证据与未决事项。

subagent 默认共用任务 worktree，不单开分支；主 Agent 划分文件写入范围，协调共用接口、本地化、看板和提交。同一文件的写入与实机操作串行安排，调查和互不重叠的改动可并行。subagent 完成不等于验收，主 Agent 核对结果并负责整合；subagent 线程不必与 sub-issue 一一对应。

遇到技术疑问先查证；玩法歧义沿用[local Issue 流程](../collaboration/README.md#工作流)，继续无阻塞内容。等待答案或 Review 时保留可续做记录；全部剩余工作被阻塞时如实报告，不为维持运行制造新任务。

## 提交与 local PR

完成相关检查后在任务分支提交，提供 [local PR](../collaboration/templates/pull-request.md)，默认不创建远端 GitHub PR。PR 记录 Base、Head 分支、worktree、实际 diff、Checks、未测范围与 Review 记录；交付消息提供当前 head 的固定提交号。PR 文档用分支定位最新 head，避免把自己的元数据提交写成过期的 head。

大目标可在同一 worktree 分批提交、累积到一条 PR；需要独立 Review 与整合时再拆 PR／worktree。已有 PR 的修订继续使用原编号。暂停或交接时在原 Issue／PR 留下剩余范围与下一步，不另建聊天流水账。

默认“实现”目标的 Agent 交付条件是：目标范围的代码、配套内容及相关验证完成，状态回写且 PR 可 Review。未执行的必要验证仍属未完成工作；人类 Review 与实际 Merged 独立记录。目标明确要求合入时，未获 Review 通过不能宣称目标完成。

## Review、整合与清理

- 人类在 PR Timeline 或 CLI 给出 Comment／Approve／Request changes，并注明所评 head。未明确结论时保持 Open；Request changes 后在原 worktree 修改、验证、更新 head 并请求复审。
- 本项目默认以人类对所评 head 的 Approve 作为整合及后续清理的授权。新增实质改动需再次 Review；仅追加评审记录与归档索引不使批准失效。
- 整合前核对目标分支变化和相关检查。发生冲突时在任务 worktree 解决并验证；解决导致实质改动时重新提交 Review。随后将批准的改动合入目标分支，记录实际整合提交，更新 Merged、关联 Issue 和 Archive。
- 仅在改动已整合、记录已保留且任务 worktree 没有未保存工作时清理。从主目录确认 `git worktree list` 中的绝对目标仍位于本仓库的任务目录下，检查未跟踪／忽略文件、测试会话和需要保留的证据，再执行 `git worktree remove <path>`。分支用 `git branch -d <branch>` 删除；不使用强制删除掩盖未合入改动。
- 测试会话未结束、证据未保存或 PR 未通过时保留 worktree。放弃任务沿用 Closed 流程，保留原因与提交；不把未获批准的工作视作可清理。

Review 关注内容及文档职责见[Review 指引](review.md)，Issue／PR 的术语、评论和归档见[协作入口](../collaboration/README.md#工作流)。
