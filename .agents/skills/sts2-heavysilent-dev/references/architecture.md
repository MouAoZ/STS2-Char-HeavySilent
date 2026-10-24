# 工程结构与数据流

核验基线：[baseline.json](../../../../docs/development/baseline.json)。

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
| [ModEntry](../../../../HeavySilentCode/ModEntry.cs) | 模组常量、入口和日志 | 初始化行为；当前没有玩法补丁 |
| [角色](../../../../HeavySilentCode/Character/HeavySilentCharacter.cs) | 生命、起始套牌、遗物、外观引用 | 正式起始配置和角色表现 |
| [内容池](../../../../HeavySilentCode/Character/HeavySilentPools.cs) | 卡牌、遗物、药水归属及颜色 | 新内容归属和池的展示 |
| [卡牌基类](../../../../HeavySilentCode/Cards/HeavySilentCard.cs) | 共同卡池、占位图片路径 | 为具体卡牌提供独立图片时调整 |
| [具体卡牌](../../../../HeavySilentCode/Cards) | 费用、动态数值、关键词、效果与升级 | 每张卡的规则 |
| [资源](../../../../HeavySilent) | eng／zhs 文本、图片与导入设置 | 文案、翻译、美术 |
| [脚本](../../../../scripts) | 环境核对、构建、包装、部署与检查 | 开发流程 |

## 测试工具与正式模组的分界

主工程 `HeavySilent.csproj` 只编译 `HeavySilentCode/**/*.cs`。下列测试代码有独立入口，不会因为位于仓库内就被编进玩家 DLL：

| 位置 | 职责 |
| --- | --- |
| [session.py](../../../../tools/game-testing/session.py) | 构建部署、进程与专用档管理、备份和退出恢复 |
| [mcp_server.py](../../../../tools/game-testing/mcp_server.py)、[client.py](../../../../tools/game-testing/client.py) | Agent 工具入口、通信、等待状态与截图 |
| [测试桥](../../../../tools/game-testing/bridge/HeavySilentTestBridge.csproj) | 独立 C# 工程；HsControl 管会话／快照，HsScenarios 构造通用场景，HsFixtures 保留固定夹具 |
| [tasks.py](../../../../tools/game-testing/tasks.py)、[runner.py](../../../../tools/game-testing/runner.py) | 前者记录 Agent 的测试与判断；后者执行显式选择的固定回归 |
| [bootstrap.py](../../../../tools/game-testing/bootstrap.py)、[upstream.json](../../../../tools/game-testing/upstream.json) | 固定上游下载和版本适配；下载源码在忽略的 `.tools/sts2mcp/src` |

需要调整上游桥时修改 `bootstrap.py` 的补丁，不能只编辑 `.tools` 内生成的文件，否则下次准备会丢失修改。新增测试能力优先放本地桥扩展；玩法实现放正式模组，二者不可互相替代。

## 新增程序目录

首次新增共用机制时再引入 `Mechanics`，首次新增状态效果时引入 `Powers`，确需补丁时引入 `Patches`。
普通内容调用共用机制，补丁处理游戏现有入口无法表达的行为；避免每张卡复制一套磨顶代码。

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
初始化只有基础牌和气，因此没有完整奖励卡池。不要在缺乏设计时把气改成普通稀有度来填补奖励。

## 构建边界

- SDK／NuGet 版本在仓库中固定；机器路径放忽略的 `local.props`。
- 游戏 DLL 通过本地路径引用，`Private=false`，不进入模组产物。
- 普通构建输出在 `.godot/mono/temp/bin/`；`package` 使用独立资源工程导出 PCK。
- 包信息和哈希在模组目录外的 `package-info.json`；游戏扫描模组 JSON 时不会把它当成另一个 manifest。
- 部署只复制本模组的已验证文件；BaseLib 由玩家单独安装。

当前未引入对外公共 API。未来共用机制属于本模组内部接口，语义明确后再决定参数、返回结果与事件。
