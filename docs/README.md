# 开发手册与学习路线

本手册的读者是策划、Reviewer 和接手实现的 Agent。无需先学完整 C#；先看一张卡如何从设计进入游戏，再沿需要补充原理。
详细技术正文放在项目 Skill 的 `references` 中，人与 Agent 读取同一份事实，避免两套说明逐渐分叉。
没有历史会话的 Agent 从[首次接手指南](../.agents/skills/sts2-heavysilent-dev/references/first-session.md)开始；它区分同机新会话、全新克隆以及需要补充核对的内容。

## 建议阅读顺序

1. [项目状态](development/status.md)：哪些内容真的实现、哪些验证仍待完成。
2. [工程结构](../.agents/skills/sts2-heavysilent-dev/references/architecture.md)：每种改动应放在哪里。
3. [程序原理与第一张卡](../.agents/skills/sts2-heavysilent-dev/references/api-primer.md)：继承、生命周期、数值和结算。
4. [内容填充教程](../.agents/skills/sts2-heavysilent-dev/references/content-authoring.md)：卡牌、遗物、药水、事件如何设计与落地。
5. [构建、调试与升级](../.agents/skills/sts2-heavysilent-dev/references/build-debug.md)：从命令到游戏，如何定位错误。
6. [Agent 实机测试](../.agents/skills/sts2-heavysilent-dev/references/game-testing.md)：按改动构造场景、操作与观察，以及可选回归、专用档位和证据。

## 日常工作入口

| 任务 | 入口 |
| --- | --- |
| 新增内容 | [策划模板](design/templates/README.md) |
| 理解现有卡牌 | [样例设计及代码对应](design/teaching-samples.md) |
| 设计磨顶／征召 | [机制定义与待澄清项](../.agents/skills/sts2-heavysilent-dev/references/mechanics.md) |
| Review 一次改动 | [Review 指引](development/review.md) |
| 在游戏里验证 | [实机清单](development/smoke-test.md) |
| 查 API 来源 | [上游资料索引](../.agents/skills/sts2-heavysilent-dev/references/sources.md) |

每份技术参考的“核验基线”指向具体版本。发现网上文档与本机代码不同，先核对版本，再修改实现与说明。
