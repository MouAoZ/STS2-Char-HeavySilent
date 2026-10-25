# 工程结构与数据流

核验基线：[baseline.json](../../../../docs/development/baseline.json)。

## 模组源码与开发脚手架

以能否独立产出玩家模组为边界：只复制 [mod/](../../../../mod) 的可提交内容，提供所需工具链和游戏引用，即可由 [build.ps1](../../../../mod/build.ps1) 生成 DLL、PCK 与 manifest。独立构建不读取外层 README、设计、Skill、版本基线、验证脚本或测试桥。

| 范围 | 位置与内容 | 依赖方向 |
| --- | --- | --- |
| 模组源码 | `mod/HeavySilentCode/` 的 C#，`mod/HeavySilent/` 的文本、图片和导入设置 | 依赖游戏 API 与 BaseLib |
| 独立构建输入 | `mod/` 内的 csproj／sln、Directory.Build.props、global.json、NuGet.Config、两份锁文件、project.godot、export_presets.cfg、manifest 和 build.ps1 | 只使用同目录输入、.NET／MegaDot／PowerShell 与外部游戏引用 |
| 随包许可 | `mod/THIRD_PARTY_NOTICES.md` | 属于必须随玩家包分发的输入，不是开发教学文档 |
| 开发脚手架 | `docs/`、`.agents/`、AGENTS.md、根 README、`scripts/`、`tools/`、`tests/` 和编辑器配置 | 可以读取源码、调用构建入口；源码构建不反向读取它们 |
| 本机状态与产物 | `mod/local.props`、`.godot/`、`.cache/`、`artifacts/`、测试环境等 | 按需生成，均不作为可提交源码 |

独立入口默认输出 `mod/artifacts/`；外层 `scripts/dev.ps1` 显式指定根 `artifacts/`，增加验证、完成记录与部署保护。两个入口共用构建实现。复制源码时排除 local.props、缓存与产物；可复制 local.props.example 后填写路径，也可通过参数传入游戏和引擎路径。

## 三种输入与三个产物

策划 Markdown 是人类与 Agent 的需求约定；C# 是游戏执行的逻辑；本地化 JSON 和图片是游戏显示的资源。
Agent 把设计翻译为后两者，并将设计编号、模型 ID 与源码对应起来。

```text
设计 Markdown ──Agent 实现──> C# 内容模型 ──dotnet build──> HeavySilent.dll
                         └─> 本地化／图片 ──Godot 导出──> HeavySilent.pck
                             模组信息 ─────────────────> HeavySilent.json
```

游戏根据 manifest 找到 DLL 与 PCK，调用 DLL 的初始化入口，并由 BaseLib 注册内容类型。
PCK 以 `res://HeavySilent/` 为资源根，避免覆盖原版同名资源。

## 代码分工

| 位置 | 职责 | 常见修改 |
| --- | --- | --- |
| [ModEntry](../../../../mod/HeavySilentCode/ModEntry.cs) | 模组常量、入口和日志 | 通过 Harmony 注册当前程序集的机制与奖励补丁 |
| [角色](../../../../mod/HeavySilentCode/Character/HeavySilentCharacter.cs) | 生命、起始套牌、遗物、外观引用 | 正式起始配置和角色表现 |
| [内容池](../../../../mod/HeavySilentCode/Character/HeavySilentPools.cs) | 卡牌、遗物、药水归属及颜色 | 新内容归属和池的展示 |
| [卡牌基类](../../../../mod/HeavySilentCode/Cards/HeavySilentCard.cs) | 共同卡池、占位图片路径 | 为具体卡牌提供独立图片时调整 |
| [具体卡牌](../../../../mod/HeavySilentCode/Cards) | 费用、动态数值、关键词、效果与升级 | 起始／先古卡及 Attacks、Skills、Powers 子目录 |
| [共用机制](../../../../mod/HeavySilentCode/Mechanics) | 磨顶／征召、计数、支付、奖励作用域及永久关键词 | 普通卡牌复用的内部接口与必要补丁 |
| [状态](../../../../mod/HeavySilentCode/Powers)／[遗物](../../../../mod/HeavySilentCode/Relics) | 原版 Hook 与磨顶监听、状态堆叠、遗物归属 | 能力持续效果与遗物触发 |
| [奖励补丁](../../../../mod/HeavySilentCode/Patches/RewardPatches.cs) | 原版奖励显示／选择／入组的窄入口 | 连接奖励生命周期与 `RewardContent`；目标方法见[接入参考](vanilla-effects.md#当前补丁边界) |
| [资源](../../../../mod/HeavySilent) | eng／zhs 文本、图片与导入设置 | 文案、翻译、美术 |
| [独立构建](../../../../mod/build.ps1) | 从源码生成 DLL 与资源包 | 模组打包输入与命令 |
| [开发脚本](../../../../scripts) | 环境核对、验证、部署及调用独立构建 | 脚手架流程 |

## 测试工具与正式模组的分界

主工程 `mod/HeavySilent.csproj` 只编译其目录内的 `HeavySilentCode/**/*.cs`。下列测试代码有独立入口，不会因为位于仓库内就被编进玩家 DLL：

| 位置 | 职责 |
| --- | --- |
| [session.py](../../../../tools/game-testing/session.py) | 构建部署、进程与专用档管理、备份和退出恢复 |
| [mcp_server.py](../../../../tools/game-testing/mcp_server.py)、[client.py](../../../../tools/game-testing/client.py) | Agent 工具入口、通信、等待状态与截图 |
| [测试桥](../../../../tools/game-testing/bridge/HeavySilentTestBridge.csproj) | 独立 C# 工程；HsControl 管会话／快照，HsScenarios 构造通用场景，HsFixtures 保留固定夹具 |
| [tasks.py](../../../../tools/game-testing/tasks.py)、[runner.py](../../../../tools/game-testing/runner.py) | 前者记录 Agent 的测试与判断；后者执行显式选择的固定回归 |
| [bootstrap.py](../../../../tools/game-testing/bootstrap.py)、[upstream.json](../../../../tools/game-testing/upstream.json) | 固定上游下载和版本适配；下载源码在忽略的 `.tools/sts2mcp/src` |

需要调整上游桥时修改 `bootstrap.py` 的补丁，不能只编辑 `.tools` 内生成的文件，否则下次准备会丢失修改。新增测试能力优先放本地桥扩展；玩法实现放正式模组，二者不可互相替代。

测试桥显式导入 `mod/Directory.Build.props` 以复用本机游戏路径；bootstrap 在 `mod/` 下执行 dotnet，使用该目录的 SDK 版本和 NuGet.Config。这个依赖只从测试脚手架指向模组构建配置。

## 共用机制与补丁

`Mechanics` 已提供 `MillCmd`、`RecruitCmd`、`MillTurnState`、`MillPayment`、`PermanentKeywords` 和 `RewardContent`。普通内容调用这些入口，具体伤害、格挡、生成、选择与出牌继续走原版 Command／Hook。`MillVar` 负责磨顶卡面修正预览，`MechanicHoverTips` 提供双语术语。

`MillTurnState` 和 `MillPayment` 的窄补丁与所属机制放在同一文件；奖励入口位于 `Patches/RewardPatches.cs`。新增补丁前核对原版入口不足之处，并更新[补丁目标与版本](vanilla-effects.md#当前补丁边界)，不为每张卡复制通用流程。

## 为什么一张卡一个类

游戏通过类型和模型 ID 识别卡牌，再为每个玩家创建独立实例。
这个项目采用常规 C# 内容类，便于直接对照原版卡牌阅读；没有另造效果语言或 Markdown 运行时解释器。
基类只处理共享归属和表现，具体结算顺序保留在每张卡中。

`CustomID` 明确指定稳定的模型 Entry，例如 `HEAVYSILENT_QI`。
显示名来自 `HEAVYSILENT_QI.title`，所以卡名可以翻译或修改，而保存标识保持不变。
完整 `ModelId` 还包含卡牌、角色等类别；控制台究竟接受 Entry 还是完整形式，以当前游戏 `help` 为准。

## 内容注册与池

角色继承 `PlaceholderCharacterModel`，进而获得 BaseLib 的角色注册支持。
卡牌继承 `CustomCardModel`，并通过基类上的 `[Pool(typeof(HeavySilentCardPool))]` 关联角色卡池。
角色通过 `CardPool`、`RelicPool`、`PotionPool` 属性暴露这些池。

“在池里”不等于“会作为普通奖励出现”：基础牌和状态牌有不同稀有度筛选。
当前角色池包含70张普通／罕见／稀有角色牌；4种初始牌、气、2张先古卡按自身稀有度排除常规奖励。微调的2个 Token 模型仅用于选择界面，隐藏于卡库且不能在战斗中随机生成，不计为新增设计卡。遗物通过 `HeavySilentRelic` 关联角色遗物池；砥石为 Starter、玉砥为 Ancient，其余两件按稀有度获取。

## 构建边界

- SDK／NuGet 版本在 `mod/` 中固定；机器路径放忽略的 `mod/local.props`。
- 游戏 DLL 通过本地路径引用，`Private=false`，不进入模组产物。
- 普通构建输出在 `mod/.godot/mono/temp/bin/`；PCK 使用 `mod/.cache/` 下的独立资源工程导出，资源路径仍为 `res://HeavySilent/`。
- 包信息和哈希在模组目录外的 `package-info.json`；游戏扫描模组 JSON 时不会把它当成另一个 manifest。
- 部署只复制本模组的已验证文件；BaseLib 由玩家单独安装。

当前共用机制均为本模组内部接口，不承诺对外 API 稳定性；玩法规则仍以设计正文为准。
