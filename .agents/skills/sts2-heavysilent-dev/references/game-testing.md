# Agent 实机测试与可选回归

核验游戏版本见[基线](../../../../docs/development/baseline.json)，执行结果见[项目状态](../../../../docs/development/status.md)。入口是 [dev.ps1](../../../../scripts/dev.ps1)，实现集中在 [tools/game-testing](../../../../tools/game-testing)。

## 日常使用

主工作流是 Agent 针对当前改动制定测试、操作游戏并调查结果。固定回归是可选积累，不是每次提交的默认仪式。打击等样例用于首次验证控制链或在相关公共逻辑变化时复查；实现磨顶后应围绕牌堆顺序、触发链、空堆和界面表现选择新场景。

先关闭已有游戏实例，确保启用 BaseLib、HeavySilent。本机 GodotExplorer 可以保留；其他模组会触发环境检查。首次运行需要联网下载固定版本工具和源码：

```powershell
.\scripts\dev.ps1 test-setup
.\scripts\dev.ps1 test-start
# 接下来由 Agent 通过 MCP 登记测试意图、构造场景、操作和判断。
.\scripts\dev.ps1 test-stop
```

也可以把 Agent 制定的检查保存为 JSON，然后执行 `test -TaskFile <task.json>`：它启动可见游戏并登记任务，交给 Agent 继续操作，不自动执行旧回归。格式见[工具能力验收示例](../../../../tools/game-testing/agent-task.example.json)，每项说明问题、预期和选择理由。任务结束后执行 `test-stop`。默认构建 Debug。

只有显式 `test -Suite smoke|regression` 才运行固定套件，完成后关闭自己启动的游戏；`-KeepOpen` 保留现场。无需每次内容修改都运行完整套件。

| 命令 | 行为 |
| --- | --- |
| `test-setup` | 校验下载、安装仓库内 Python、还原锁定依赖、构建并安装独立测试桥、生成项目 MCP 配置 |
| `test-start` | 重建本模组和测试桥、部署、备份个人存档、启动第 3 档，验证 PID／会话／DLL 哈希 |
| `test -TaskFile <task.json>` | 启动并登记本次改动的测试任务，随后由 Agent 决定如何操作和调查 |
| `test -Suite smoke` | 1 轮：选角开局、升级保存续玩、三张基础牌、当前语言文本与截图、结束回合 |
| `test -Suite regression` | 同一种子重复 2 轮；每轮增加升级牌、抽牌边界、力量／敏捷／虚弱／敌人格挡，共 38 项 |
| `test-stop` | 退出受管局和游戏，恢复原档位选择与显示／语言设置，核对第 1、2 档哈希 |

无游戏脚本测试：`python -m unittest discover -s tests -v`。脚本测试、编译与实机测试是不同证据。

## 工具与配置

沿用 .NET 10.0.402 和 MegaDot 4.5.1-m.14。测试桥目标框架仍为 `net9.0`，直接引用本机 STS2、GodotSharp 与 Harmony，不复制游戏 DLL。

`test-setup` 用现有 Python 引导，把 uv 0.13.5 安装到 `.tools/uv`，把 Python 3.13 放到 `.tools/python`，把测试依赖放到 `tools/game-testing/.venv`。MCP SDK 等 Python 包由 [uv.lock](../../../../tools/game-testing/uv.lock) 锁定。工具不写全局 PATH 或个人 Codex 配置。

项目本机配置 `.codex/config.toml` 使用绝对路径启动 `mcp_server.py`，生成后被 Git 忽略。若 Codex 当前会话尚未加载新 MCP，重新加载项目会话；测试 CLI 始终可用。迁移仓库路径后重新生成此配置。Codex 需要信任项目才能加载项目 MCP；配置方式参考 [Codex MCP 文档](https://learn.chatgpt.com/docs/extend/mcp)。

测试桥基于 [Gennadiyev/STS2MCP](https://github.com/Gennadiyev/STS2MCP)，固定提交 `55e064850a68f3b4cde7e5fd525bf9b2dec4e885`。下载 URL、SHA256 与 uv 版本在 [upstream.json](../../../../tools/game-testing/upstream.json)。源码下载到 `.tools/sts2mcp/src`，补丁集中在 [bootstrap.py](../../../../tools/game-testing/bootstrap.py)，上下文不匹配立即失败。

本机旧 GodotExplorer 1.0.0 是场景检查工具，不承担卡牌回归。选择 STS2MCP 是因为它能读取战斗模型并操作原版菜单和 Action 队列；其他候选仍需相似版本适配，直接使用现有旧 MCP 无法覆盖本任务。

## Agent 如何测试

1. 先根据改动列出风险与问题。`start_session` 启动后调用 `begin_test_task(title, change_summary, checks)`；每个 check 包含 `id/question/expected/reason`。不要从实际结果倒填预期。
2. `get_state` 确认 `profile=3`。`start_combat` 可快速走正常菜单进入第一场战斗；已有战斗默认复用，`fresh=true` 才放弃测试档当前局重开。也可自行逐步操作菜单。
3. `list_models('cards'|'powers', query)` 查询实际注册 ID，`prepare_scenario` 安排本次要验证的牌堆顺序和状态。它接受任意已注册卡牌／Power，不限于样例卡，不需要为每张新卡增加专用工具。
4. `perform_action` 使用状态提供的参数：`play_card(card_index,target)`、`end_turn`、`choose_map_node(index)`、`choose_event_option(index)` 等。敌人目标使用 `ui.battle.enemies[].entity_id`，不是数组下标。等待实际结算后读取状态，必要时继续操作以观察触发链。
5. `inspect_card(index)` 打开放大卡面，`capture_screenshot` 返回真实 PNG。观察后调用 `record_observation(check_id, verdict, observed, reasoning, evidence)`，将预期、实际状态、图像和推理连接起来。判断可为 passed/failed/blocked；未测项目保留 unexecuted。测试不同语言时用 `start_session(language='eng'|'zhs')` 或命令的 `-Language` 参数，从对应语言启动新的会话。
6. 遇到意外，用 `add_test_check` 增加调查问题，再构造更小场景验证原因。`finish_test_task` 输出 JSON 和中文 Markdown Review。`get_report` 同时展示 Agent 任务与可选回归，二者互不替代。
7. `stop_session` 完成清理。需要固定回归时才调用 `prepare_case` 或 `run_suite`。控制锁避免 Agent 与套件同时写入游戏；读取和截图可以并行。

例如，在战斗中构造明确的抽牌顺序：

```json
{
  "hand": [{"id": "HEAVYSILENT_QI", "upgraded": true}],
  "draw": [{"id": "HEAVYSILENT_STRIKE"}, {"id": "HEAVYSILENT_DEFEND", "upgraded": true}, {"id": "HEAVYSILENT_QI"}],
  "discard": [],
  "exhaust": [],
  "player": {"energy": 3, "block": 0, "powers": []},
  "enemies": [{"index": 0, "max_hp": 200, "hp": 200, "block": 0}]
}
```

数组顺序即牌堆顺序，`draw[0]` 是下一张抽到的牌。省略的牌堆会清空；默认清除双方 Power 并恢复能量上限，再应用指定条件。`clear_powers=false` 可以保留当前状态以研究触发链。`player/enemies` 支持 `hp/max_hp/block` 和 `powers:[{id,amount}]`，玩家另有 `energy`。未指定的 HP／格挡保留，永久套牌、遗物和敌人种类保留；检查场景前要看实际状态。

通用场景接口会在变更前检查模型 ID、牌堆容量和字段。若以后机制需要额外可观察数据（如待结算队列、标记来源），在桥中增加只读快照；只有普通工具无法安排必要前提时才增加夹具能力。不要让夹具代替正在验证的机制结算。重复出现且预期稳定的调查，再沉淀到可选回归。

工具代码分别在 [mcp_server.py](../../../../tools/game-testing/mcp_server.py) 与 [client.py](../../../../tools/game-testing/client.py)。动作获得 HTTP 成功响应只表示受理；客户端等待多次状态观察，出牌验收还会确认牌离开手牌、离开执行区并进入正确牌堆。超时保留证据，不自动重发出牌，以免重复操作。

## 保存隔离与测试边界

首次只接受空的 `modded/profile3`；以后通过本机 `profile3-owner.json` 识别本项目登记的测试档。已有未登记数据时停止，不自行覆盖。该档只用于测试，`start_combat(fresh=true)` 或固定回归会放弃这个档的上一局，并调用游戏自身的 `unlock all` 准备解锁环境，避免强制时间线阻止新局。新局难度保持 A0。

游戏的 Steam 云同步会覆盖启动前写入的档位选择，因此桥在 `NGame` 初始化档位之前使用游戏公开的 `SaveManager.MockInstanceForTesting(new SaveManager(localStore))` 注入 `GodotFileIo` 本地后端。序列化、迁移、异步磁盘写入仍走原版；受管进程不进行游戏云存档读写。此设置只存在于该进程，不修改 Steam 全局云开关。

启动前记录并备份原版与 modded 第 1、2 档全部文件，结束时在同一范围比较文件清单和 SHA256。备份位于当前报告目录 `personal-saves-before.zip`。原档位选择器另存字节备份并恢复；个人存档不会被测试脚本自动还原覆盖，出现真实不一致时保留证据并报错。旧会话缺少新检查范围时按启动时的范围验证，不能把未纳入基线的文件误报为新增。

桥只监听 localhost 15526，要求随机会话令牌；无测试环境变量时不启动服务、不替换存档后端。会话检查 PID 和部署 DLL 哈希；写操作限定第 3 档，禁用上游档位管理和多人 API。它不是公网服务，不应转发端口。

`HeavySilentTestBridge` 是单独的开发模组，位于游戏 `mods/HeavySilentTestBridge`；正式 `artifacts/package/HeavySilent` 只包含本角色。平时启动游戏即使桥被启用也不提供控制能力。不要把测试桥发布给玩家作为前置。

## 程序原理：安排条件和验证效果

链路为：MCP／CLI → Python 客户端 → localhost HTTP → 游戏主线程队列 → 原版 UI／Action／Command。Godot 节点及战斗模型必须在主线程访问。桥沿用上游 dispatcher，增加 [HsControl.cs](../../../../tools/game-testing/bridge/HsControl.cs) 的会话、快照、截图、查看和保存接口。

[HsScenarios.cs](../../../../tools/game-testing/bridge/HsScenarios.cs) 是通用场景接口，[HsFixtures.cs](../../../../tools/game-testing/bridge/HsFixtures.cs) 保留固定用例。它们使用 `CombatState.CreateCard` 创建实例，再用 `CardPileCmd` 放入牌堆；Power 通过 `PowerCmd` 设置，永久套牌升级使用 `CardCmd.Upgrade`。移除牌必须执行正常视觉清理，不能仅清除模型留下旧手牌节点。快照的 `visible_hand_count` 可与模型手牌数交叉核对。夹具不会直接调用卡的 `OnPlay`。

[runner.py](../../../../tools/game-testing/runner.py) 独立保存预期数字，通过原版 `PlayCardAction` 出牌后比较敌人 HP、格挡、能量、牌堆和卡实例守恒。断言不从卡牌描述提取“正确答案”。当前覆盖：

- 打击 6／9；防御 5／8；力量 2 时伤害 8；虚弱时伤害向下取整为 4；敏捷 2 时格挡 7；敌人已有 3 格挡时实际扣血 3。
- 气 0 费，抽 1／2 后消耗；洗牌、空堆、满手、近满手和禁止抽牌。满 10 手牌含气+时，打出腾出一格，实际抽 1 张；9 手牌时抽 2 张。
- 房间进入检查点保存 → 返回菜单 → 继续，核对永久卡 ID 和升级状态。不会制造任意时刻的战斗存档，也不把夹具牌写入永久套牌。
- 当前启动语言的卡名断言、真实截图，以及正常结束回合和敌人行动；不同语言单独启动会话。

固定种子为 `HEAVYSILENTTEST01`，通过游戏公开的 `NGame.DebugSeedOverride` 提供给正常开局流程，随后断言 `RunState.Rng.StringSeed`。不要把种子固定当作跨游戏版本结果不变的保证。

## 证据与维护

每次会话在 `artifacts/game-tests/<UTC时间>-<随机后缀>/` 保存 `session.json`、`actions.jsonl`、`game.log` 和截图。Agent 任务在 `tasks/` 下保存 JSON／Markdown 意图与观察记录，每次观察自动保存当时状态。可选回归另写 `report.json` 和夹具前后状态。`latest.json` 指向最近会话。视觉判断必须由查看截图的人或 Agent 给出，不能由脚本自动宣称通过。

发现截图疑似缺图层时，先核对保存的原始 PNG：单独以原始分辨率查看，并按需对比相关区域像素。图片预览可能与源文件不同；不要仅凭预览异常就修改游戏渲染或断言素材缺失。记录后续纠正时追加观察，保留原判断与推翻它的证据。

检查失败时保存状态和截图。先区分夹具、控制桥、游戏 API 漂移和模组效果错误，再做针对性调查。不要为了“通过”把实际结果直接改成预期值。修正后验证受影响路径；是否运行已有套件取决于影响范围，不要求无关内容完整重跑。

升级游戏／上游时核对：`StartRunLobby`／`LoadRunLobby` 字段、种子入口、Action 完成语义、主线程队列、存档生命周期以及截图信号。目前额外修正了上游队列一次处理 10 项时会丢弃第 11 项的问题。修改夹具或 API 时同步更新本页、断言和版本基线。

当前范围不覆盖完整爬塔、空奖励池、联机、尚未实现的磨顶／征召或正式美术。运行中切换 `LocManager` 会涉及已经创建的界面和关键词缓存，因此没有暴露热切换语言工具。使用 `start_session(language='eng')` 或 `test-start -Language eng`，结束时恢复原设置；这样文字和资源都经过游戏正常启动加载。
