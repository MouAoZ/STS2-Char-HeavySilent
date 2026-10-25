# 首次接手：从仓库恢复工作上下文

本页供没有历史聊天记录的 Agent 使用，只读与当前任务有关的资料。

## 最短阅读路径

1. 根目录 [AGENTS.md](../../../../AGENTS.md) → [Skill](../SKILL.md) → [执行流程](../../../../docs/development/workflow.md)。
2. `git status --short`、`git worktree list --porcelain`：定位当前分支、已有改动及可续做的任务 worktree；修改前进入正确 worktree。
3. [README 看板](../../../../README.md#项目进度)与[设计跟踪](../../../../docs/design/implementation-tracker.md)：选择范围，读取所属设计条目及代码，不从历史草稿推断当前规则。
4. 检查主工作目录 main 的 [local Issue／PR](../../../../docs/collaboration/README.md)及相关 Timeline，按 PR 的 Worktree／Head 定位实现。处理新回答和 Request changes，再核对[验证证据](../../../../docs/development/status.md)及剩余工作；任务目录中的协作副本可能滞后。
5. 用[工程结构](architecture.md)定位源码，其余资料由 Skill 按任务路由；不必先读完整个目录。

玩法以当前明确指令及设计正文为准，实际行为以源码和本次观察为准；历史报告只能提供线索。跨会话进度保存在 Issue／PR、设计条目与提交中，不依赖旧聊天。

## 本机配置与 worktree

同机续做沿用已核实的配置。新 worktree 不带忽略文件；需要构建时从主目录复制 `mod/local.props` 并核对路径，产物在当前 worktree 生成。仅改文档运行 `python scripts/verify.py`，无需初始化游戏工具。

涉及构建或游戏时按[构建指南](build-debug.md)运行 `dev.ps1 doctor` 并选择相应检查；本机 PowerShell 7 入口为 `C:\Program Files\PowerShell\7\pwsh.exe`。不因新会话默认全量回归。

实机配置含绝对路径：在任务 worktree 执行 `test-setup` 后重新加载该目录的 MCP，确认启动脚本指向当前 worktree；旧会话工具仍指向原目录时，使用当前 worktree 的 CLI／Python 接口。会话、测试档归属和控制锁的共享位置见[测试工作流](game-testing.md#worktree-中的实机测试)。

## 全新克隆或换机器

仓库不携带游戏、BaseLib、SDK、引擎和本机配置。按[构建指南](build-debug.md)准备依赖，从 `mod/local.props.example` 创建路径配置；需要源码打包时不依赖外层脚手架。缺少工具时记录验证限制，不把历史成功当作本次已验证。

| 不随 Git 提交的内容 | 处理方式 |
| --- | --- |
| `mod/local.props` | 核对真实依赖路径 |
| `.codex/config.toml`、`.tools/`、测试 `.venv/` | 按需由 `test-setup` 生成，首次可能需网络；不原样搬用带旧绝对路径的配置 |
| 构建缓存与包 | 在当前 worktree 重新生成 |
| 主目录 `artifacts/game-tests/` | 保留会话、报告、存档备份与测试档归属；worktree 共用，不随任务清理 |
| `.cache/worktrees/` | Git 登记的任务目录；按执行流程清理，不能当普通缓存批量删除 |

交付内容与文档回写沿用[Review 指引](../../../../docs/development/review.md)及[跟踪约定](../../../../docs/design/implementation-tracker.md#日常维护)。
