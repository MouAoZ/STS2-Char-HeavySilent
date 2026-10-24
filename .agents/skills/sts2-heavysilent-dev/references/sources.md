# 上游资料索引

核验日期：2026-10-24。具体本机版本见[基线](../../../../docs/development/baseline.json)。
下列资料供按需查询；项目参考正文是中文解释与经本机编译核验的实例，不是整站镜像。

| 来源 | 用途与局限 |
| --- | --- |
| [ModTemplate-StS2](https://github.com/Alchyr/ModTemplate-StS2) | 本工程的角色模板来源；所用 NuGet 版本 2.5.3 |
| [Setup](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup) | MegaDot、.NET、模板和 BaseLib 的安装说明 |
| [Modding Basics](https://github.com/Alchyr/ModTemplate-StS2/wiki/Modding-Basics) | DLL／PCK／manifest、加载目录与基础工作方式 |
| [BaseLib](https://github.com/Alchyr/BaseLib-StS2) | 社区库源码、版本与实现；本工程使用 3.4.7 |
| [BaseLib Wiki](https://alchyr.github.io/BaseLib-Wiki/) | Custom Models、Hook、资源、本地化；有些页面仍未完成 |
| [CustomCardModel](https://alchyr.github.io/BaseLib-Wiki/docs/models/custom-card.html) | 卡池和卡牌扩展；示例签名须结合本机 API 核对 |
| [Common Commands Cookbook](https://github.com/Alchyr/ModTemplate-StS2/wiki/Common-Commands-Cookbook) | 查常见效果的实现方向，不直接假设所有片段在当前版本可编译 |
| [Testing and Debugging](https://github.com/Alchyr/ModTemplate-StS2/wiki/Testing-and-Debugging) | 控制台、日志、调试器和后续联机测试 |
| [MegaDot](https://megadot.megacrit.com/) | Mega Crit 的引擎发行入口 |
| [官方模组上传器](https://github.com/megacrit/sts2-mod-uploader) | 后续 Workshop 发布流程，初始化未发布 |
| [OpenAI 指令与 Skill 建议](https://developers.openai.com/blog/rethinking-skills-and-prompts-for-gpt-6-astra) | 精简 AGENTS、明确触发条件和按需加载参考 |
| [AGENTS 文档](https://learn.chatgpt.com/docs/agent-configuration/agents-md) | 仓库指令的发现与作用域 |
| [Skill 文档](https://learn.chatgpt.com/docs/build-skills) | `.agents/skills`、渐进加载和元数据 |
| [STS2MCP](https://github.com/Gennadiyev/STS2MCP) | 游戏状态与操作桥上游；固定提交及本机兼容补丁见[测试工作流](game-testing.md) |
| [Codex MCP 配置](https://learn.chatgpt.com/docs/extend/mcp) | 项目级 stdio MCP 配置；不写个人全局配置 |

## 本机核验依据

- `sts2.dll`：`CharacterModel`、`CardModel`、原版 `StrikeSilent`、`DefendSilent`、`Slimed`、`CardPileCmd` 等。
- `BaseLib.dll`：`CustomCardModel`、`PlaceholderCharacterModel`、`CustomIDAttribute`、`PrefixIdPatch` 等。
- 本机游戏 JSON：核对字段名、文字格式与文本分组；提取副本仅保存在忽略缓存。
- Steam manifest 与 SteamCMD 元数据镜像：核对 public-beta Build ID；镜像不是 Steam 官方更新器。

查阅或引用上游时记录其来源和适用版本。可分发的模板／占位资源许可保留在[模组第三方说明](../../../../mod/THIRD_PARTY_NOTICES.md)中；测试桥与开发工具许可见[脚手架第三方说明](../../../../THIRD_PARTY_NOTICES.md)。
游戏完整程序集和提取资源不随仓库或模组包分发。
