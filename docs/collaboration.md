# 人类与 Agent 协作

这里是本地 Issue／PR 协作入口，属于开发脚手架。沿用 GitHub 的语义：[Issue](https://docs.github.com/en/issues/tracking-your-work-with-issues/learning-about-issues/quickstart) 跟踪问题与工作，[sub-issue](https://docs.github.com/en/issues/tracking-your-work-with-issues/using-issues/adding-sub-issues) 拆分可独立跟踪的子任务，[PR](https://docs.github.com/en/pull-requests/reference/pull-requests) 提出具体改动并承载评审。Q&A 只是评论的表达方式，不是一问一答就新建一个 Issue。

本地 Markdown 代替 GitHub 页面，Git 提交与 diff 提供改动证据；归档目录代替关闭记录的列表视图。保留既有 Q／R 编号和锚点作为本地标识，不冒充 GitHub 的 `#编号`。Issue 的讨论、PR 的评审、代码是否已合入和内容是否已实现分别记录。

在 Codex CLI 中尚未得到答案、需要保留到后续会话的问题都可以登记，包括玩法、需求、命名与素材、开发环境、任务范围和验收反馈。技术上能由 Agent 查证的事项先由 Agent 调查，确实需要人类选择或提供信息时才进入“待人类”。

## 如何回答

1. 从下方任务表打开条目，在同一 Issue／PR 的讨论区按“日期 · 作者 · 内容”追加评论。已有空白 A 栏可以直接填写；之后的追问、回答、调查与修正继续追加，不覆盖旧回答，也不因多一轮交流新增编号。
2. 也可以在 CLI 中回复“Q4.5：……”，或“R1：通过／需要修改／暂缓，理由是……”。Agent 将回答和来源日期写回对应讨论。PR 的“通过／需要修改／普通意见”分别记录为 Approve／Request changes／Comment；暂缓只是工作安排，不表示关闭。
3. 不确定时写“暂缓，等……再决定”；需要分析时写“请 Agent 比较……”。无需为了填表立即作决定。
4. 告诉 Agent“处理协作文档中的新回答”。你不需要维护状态、设计正文或看板；Agent 接手时也应主动检查新回答，而不只看表内状态。

活动条目的正文与讨论在本页维护；关闭后完整迁入 `docs/collaboration/archive/`，本页保留索引。确认后的当前玩法写入[设计文档](design/README.md)，工程约定写入相应技术参考；协作记录保留讨论过程与落点，不作为另一份规则正文。

## 协作任务

2026-10-26：Q1～Q8 及其已有子项已解决并归档，四处补充交流均属于原条目的评论。当前没有 Open Issue 或新建 PR；待处理的是历史提交评审 R1。

| 编号 | 类型／主题 | Issue／PR 状态 | Review | 下一步由谁处理 |
| --- | --- | --- | --- | --- |
| [R1](#r1) | 历史提交 Review · 模组源码与脚手架拆分 | 不适用（未创建 PR） | Pending | 人类核对目录边界，给出评审意见 |

### 状态与维护

| 对象 | 生命周期 | 另行记录的字段 |
| --- | --- | --- |
| Issue／sub-issue | Open／Closed；关闭时注明 Completed 或 Not planned 及原因 | 负责人、标签、父项、关联 PR，按需填写 |
| PR | Open／Closed（未合并）／Merged（已合并） | Draft／Ready for review；base／head 与实际整合证据 |
| Review | Pending 表示尚无结论；提交意见用 Comment／Approve／Request changes | 对应的 head 提交、评论和后续修订 |

“待人类／待 Agent／暂缓”属于负责人或看板安排，不替代上述生命周期；Approved 不等于 Merged，评论已解决也不等于 PR 已通过。[GitHub 的 Review 类型](https://docs.github.com/en/pull-requests/reference/pull-request-reviews)区分普通意见、批准与要求修改。

- **Issue 与评论：** 同一目标内的追问、补充证据与修正持续追加到同一讨论。旧结论需要重新处理时[重新打开原 Issue](https://cli.github.com/manual/gh_issue_reopen)，记录原因；仅补充说明也可以在 Closed 条目下评论。只有新的独立目标才新建 Issue，必要时关联原条目。
- **父项与子项：** sub-issue 本身也是 Issue，有自己的状态、目标和评论，并链接父项。Q4 是父项，Q4.5 是其已有子项，“Q4.5 后续确认”是 Q4.5 的下一轮评论；`q4-followup` 只是历史定位锚点。已有 Qn.m 编号继续使用，不再为每轮回复加层级。普通问题清单不自动变成 sub-issue；只有需要独立跟踪时才拆分。父项按目标及子项完成情况关闭，不把子项的一次回答当作整个父项完成。
- **PR 与评审：** 一组待整合的具体改动使用 PR 记录，注明关联 Issue、base／head、diff 范围、行为变化与验证。人类在同一 PR 内多轮评论，Agent 修订并追加新提交和检查结果，再请求复审。每次 Review 绑定当时的 head，不把旧批准直接当作新改动已获批准。超出改动范围的新工作才另建 Issue 并回链。
- **关闭与实现：** [Issue 关闭](https://docs.github.com/en/issues/tracking-your-work-with-issues/administering-issues/closing-an-issue)以该 Issue 的目标为准：设计决策得到答案且写回正式文档即可完成；实现任务需完成对应实现和验证。PR 创建、关联 Issue、自动检查通过或一次回答均不自动关闭未完成工作。本地文档中的关联不会自动执行 GitHub 的关闭逻辑。
- **合并事实与授权：** PR 只有在改动实际整合到目标分支后才标 Merged，并记录结果提交；放弃时 Closed，说明原因。仅做提交后的回顾评审时明确标为“提交 Review”，不虚构 PR 或合并事件。执行已授权的实现、验证和本地提交继续推进；实际整合按会话授权与仓库约定处理，不因登记 PR 额外阻塞已授权工作。
- **归档与重开：** 同次交付中将已关闭 Issue、终结的 PR 或已完成的历史提交评审连同评论完整归档，记录关闭／整合及归档日期、原因和文档落点。归档是本地整理方式，不是禁止继续讨论的状态。重开时把原条目及完整线程迁回活动区，保留原编号、关闭历史和归档处的跳转入口；同一正文只保留一份。若子项重开使父项目标未完成，同步重开父项。
- **编号与入口：** Q 为既有 Issue 标识，R 为 PR／Review 标识；检查活动与归档后分配下一个未用编号，目前为 Q9／R2。归档文件按日期与编号范围命名，迁移时修正相对链接，保留旧锚点。CLI 已有明确答案直接记录并处理；README 只索引活动与归档，不复制评论正文。

<a id="r1"></a>

## R1 · 模组源码与脚手架拆分

**类型与当前状态：** 历史提交 Review；Pending，等待人类。此改动已在 `main` 的历史中，未创建 PR；下方评审范围是 `2513f3b..183d63e`。按 PR 的评论与 Review 方式继续沟通，不补记未经发生的批准或合并。

**评审对象：** 已交付提交 `183d63e`。边界说明见 [README](../README.md#源码与脚手架)，独立入口见 [mod/build.ps1](../mod/build.ps1)，完整[验证记录](development/status.md#模组源码与脚手架拆分2026-10-25)可供复核。

**Q／请核对：** `mod/` 已包含代码、资源和独立构建输入；文档、设计、Skill、验证部署与测试工具位于外层。这个边界是否符合“只保留源码与要求的工具链就能打包”的预期？如需调整，请指出目录或职责。

**已有证据与限制：** 单独复制源码的 Release 打包、常规 Debug／Release 打包、引擎资源加载、20项脚本测试与测试桥构建均通过；玩法源码与资源内容保留。本次未部署、未重新进行游戏实机验证。这里评审目录与构建边界，不代表正式角色玩法验收。

**A／人类结论：** 待填写（通过／需要修改／暂缓，并附意见）。

**Agent 处理记录：** 待评审；有修改意见时在本条记录处理结果与新证据。

后续意见追加在本条下方，注明日期、作者与所评提交；不为每轮反馈新建 R 编号。

## 已关闭 Issue 归档

Q1～Q8 及其45个已有子项已关闭并归档于2026-10-26，关闭原因为 Completed。完整问答与处理记录只保存在归档文件；本页保留编号入口，让既有设计和看板链接继续有效。下方四处讨论链接是原条目的评论，不计作额外 Issue。归档记录不代表相关内容已实现。

| 编号 | 主题 | 归档记录 |
| --- | --- | --- |
| <a id="q1"></a>Q1 | 磨顶 | [完整记录](collaboration/archive/2026-10-26-q1-q8.md#q1) |
| <a id="q2"></a>Q2 | 征召 | [完整记录](collaboration/archive/2026-10-26-q1-q8.md#q2) |
| <a id="q3"></a>Q3 | 生成、复制与转化 | [完整记录](collaboration/archive/2026-10-26-q1-q8.md#q3) |
| <a id="q4"></a>Q4 | 伤害与牌堆范围 | [完整记录](collaboration/archive/2026-10-26-q1-q8.md#q4) · <a id="q4-followup"></a>[Q4.5 评论](collaboration/archive/2026-10-26-q1-q8.md#q4-followup) |
| <a id="q5"></a>Q5 | 奖励、获取与保存 | [完整记录](collaboration/archive/2026-10-26-q1-q8.md#q5) · <a id="q5-followup"></a>[Q5.4 评论](collaboration/archive/2026-10-26-q1-q8.md#q5-followup) |
| <a id="q6"></a>Q6 | 计数、费用与回合时序 | [完整记录](collaboration/archive/2026-10-26-q1-q8.md#q6) · <a id="q6-followup"></a>[Q6.3 评论](collaboration/archive/2026-10-26-q1-q8.md#q6-followup) |
| <a id="q7"></a>Q7 | 替代支付与机制修正 | [完整记录](collaboration/archive/2026-10-26-q1-q8.md#q7) · <a id="q7-followup"></a>[Q7 评论](collaboration/archive/2026-10-26-q1-q8.md#q7-followup) |
| <a id="q8"></a>Q8 | 升级与可打出性 | [完整记录](collaboration/archive/2026-10-26-q1-q8.md#q8) |

## 新增条目模板

按实际需要填写，不为一条回复新建条目。Issue 与 PR 使用各自模板；子 Issue 使用 Issue 模板并填写父项。新增时检查编号，历史表格中的 A 栏视为首轮回复，之后按评论时间线追加。

### Issue／sub-issue

```markdown
<a id="q9"></a>

## Q9 · 简短主题

- 状态：Open
- 负责人／下一步：人类或 Agent；标签按需填写
- 父 Issue：无，或父项链接；子项列表按需填写
- 关联：设计条目、开发任务、PR 或提交

**描述与完成条件：** 上下文、待解决目标、影响范围；必要时给出方案及取舍。

### 评论

**日期 · 作者：** 内容。后续评论按时间追加；CLI 回复由 Agent 注明来源。

**生命周期记录：** 关闭时记录日期、Completed／Not planned、理由及正式文档落点；重开时追加原因，保留旧记录。
```

### PR

```markdown
<a id="r2"></a>

## R2 · 改动标题

- 状态：Open；阶段：Draft／Ready for review
- 作者：Agent；Reviewer：人类；Review：Pending
- 关联 Issue：链接，或说明直接来自哪项已授权任务
- base：目标分支与提交；head：工作分支与提交；diff：比较命令或文件链接

**改动与验证：** 解决的问题、实际行为、关键实现、验证证据和未测范围。

### 评论与 Review

**日期 · 人类 · head 提交 · Comment／Approve／Request changes：** 意见。

**日期 · Agent · 新 head 提交：** 对应修改、验证与待复核点。

**生命周期记录：** 实际整合后记 Merged、目标分支与结果提交；放弃记 Closed 及原因；终结后归档。
```

Review 的具体检查方法见[Review 指引](development/review.md)，开发排期见 [README 看板](../README.md#开发看板)。
