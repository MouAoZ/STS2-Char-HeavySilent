# STS2 API 与第一张卡

核验基线：[baseline.json](../../../../docs/development/baseline.json)。以下签名已在该游戏程序集与本项目编译中核对。

## “游戏 API”到底指什么

这里同时使用四层代码，稳定性与职责不同：

1. **游戏自带模组入口**：manifest、程序集加载及 `ModInitializer`。
2. **游戏模型与命令**：位于 `MegaCrit.Sts2.Core.*`，例如 `CardModel`、`ModelDb`、`DamageCmd`。这些是可引用的游戏代码，不意味着官方承诺它们跨版本不变。
3. **BaseLib 扩展**：例如 `CustomCardModel`、`PlaceholderCharacterModel`、`Pool` 和 `CustomID`，减少注册与表现上的重复工作。
4. **本项目代码**：角色、卡牌及未来的磨顶／征召封装。

ILSpy 用于阅读本机已有程序集，不修改原版 DLL。API 不确定时，先找原版类似效果，核对它怎样处理生命周期，再调用合适接口。

## 阅读打击需要的 C# 概念

打开[HeavyStrike.cs](../../../../mod/HeavySilentCode/Cards/HeavyStrike.cs)：

- `class` 定义一种卡牌；每次获得的具体卡牌是该类型的实例。
- `: HeavySilentCard(...)` 表示继承共用基类，同时把费用、类型、稀有度和目标传给它。
- `override` 表示替换游戏预留的行为，例如出牌时运行 `OnPlay`。
- `[CustomID(...)]` 是附在类型上的元数据，供 BaseLib 识别保存和本地化使用的 ID。
- `Task`、`async`、`await` 用于等待结算、选择或动画；它们在这里不表示任意开启后台线程。
- `Owner` 是这张卡所属的玩家；`cardPlay.Target` 是本次出牌的目标。

本项目的调用链是：

```text
玩家选择卡牌与目标
  → 游戏检查使用条件、处理本次出牌
  → HeavyStrike.OnPlay(choiceContext, cardPlay)
  → DamageCmd.Attack(...).FromCard(...).Targeting(...).Execute(...)
  → 游戏处理修正、伤害、事件与表现
  → 游戏完成这次出牌并处理卡牌去向
```

代码中先验证目标非空，再等待攻击命令。`FromCard(this, cardPlay)`保留来源与本次出牌的信息。
直接写“敌人生命减六”会绕开格挡、力量、虚弱、触发效果和其他游戏流程。

## DynamicVar：一个数值连接逻辑与文字

`CanonicalVars` 描述卡牌最初的数值。打击定义 `DamageVar(6, ValueProp.Move)`，升级通过
`DynamicVars.Damage.UpgradeValueBy(3)` 加三。

代码执行时使用对应动态变量，描述则写 `{Damage:diff()}`。`diff()` 是游戏文本格式化指令，负责表达数值及变化，和代码中的 `Damage` 变量对应。
角色力量、目标易伤等最终修正仍由游戏伤害管线处理；不要先把预览修正值再送入会重复修正的命令。

[防御](../../../../mod/HeavySilentCode/Cards/HeavyDefend.cs)使用 `BlockVar` 与 `CreatureCmd.GainBlock`；
[气](../../../../mod/HeavySilentCode/Cards/Qi.cs)使用 `CardsVar` 与 `CardPileCmd.Draw`。
修改基础数值和升级增量后，通常无需改描述中的数字，因为描述没有重复写死它们。

本地化 JSON 也使用 BBCode，例如 `[gold]格挡[/gold]`。格式变量和标签要保持成对，英文与中文应引用相同变量。
ModAnalyzers 检查内容需要的本地化项，项目验证脚本补充双语键和变量一致性检查。

## 气为何能作为状态牌打出

`CardType.Status` 表示分类，并不自动表示不能打出。气使用 0 费、无目标，且没有“不可打出”关键词。
`CardRarity.Status` 避免它成为普通稀有度奖励。

`CanonicalKeywords` 中的 `Exhaust` 告诉原版流程：这次出牌结束后应消耗它。
`OnPlay` 中只负责抽牌；若再次手动调用消耗命令，就可能重复触发效果或改变时序。
原版会把关键词加入卡牌显示，所以气的本地化只写抽牌句子，不再手写一次“消耗”。

普通 `CardPileCmd.Draw` 会在必要时洗牌。这正好符合气，但无法直接保证征召“不洗牌”的语义。
直接将抽牌堆中的对象移入手牌也未必等同于抽牌，因为可能漏掉抽牌 Hook；征召需要单独设计和验证。

## 模型定义与可变实例

`ModelDb.Card<T>()` 等方法取得规范模型，相当于“这个内容的标准定义”。它不是某位玩家手中的那张牌。
游戏为玩家套牌和战斗创建可变实例，实例保存所属玩家、升级、临时费用和所在牌堆。

不要把本回合计数写到静态字段或直接修改规范模型，否则不同玩家、复制卡或下一场战斗可能共用脏状态。
生成卡牌时使用目标版本的 `ICardScope.CreateCard<T>(owner)` 实例创建流程，再通过牌堆命令放入战斗。`RunState`／`CombatState` 实现此接口；`CardScope` 本身是作用域枚举，不是创建卡牌的静态工具类。
永久牌组 `Deck`、战斗抽牌堆 `Draw`、弃牌堆 `Discard`、消耗堆 `Exhaust` 是不同范围，设计中必须说清楚。

## 常用接口地图

| 需求 | 优先查阅 | 要理解的边界 |
| --- | --- | --- |
| 攻击／格挡 | `DamageCmd.Attack`、`CreatureCmd.GainBlock` | 来源、目标、修正和结算结果 |
| 抽牌／牌堆移动 | `CardPileCmd.Draw`、`CardPileCmd.Add` | 移动不一定触发抽牌或弃牌效果 |
| 生成卡牌 | `ICardScope.CreateCard`、`AddGeneratedCardToCombat` | 在对应 RunState／CombatState 创建可变实例并指定 Owner |
| 丢弃／消耗／自动打出 | `CardCmd.Discard`、`Exhaust`、`AutoPlay` | 触发时序、自动目标、X 费与终止条件 |
| Buff／Debuff | `PowerModel`、`PowerCmd.Apply` | 堆叠、持续时间、回调与移除 |
| 选择卡牌 | `CardSelectCmd`、`PlayerChoiceContext` | 等待选择、可取消、归属和同步 |
| 回合／战斗触发 | 目标版本 Model 上的 Hook | 触发对象、顺序、计数与重置 |
| 遗物／药水／事件 | 对应 Model 与 BaseLib 扩展 | 登记、使用流程、页面和保存 |

`PowerModel` 指角色身上的状态效果，不等于“能力牌”。能力牌本身仍是卡牌，通常在打出时施加一个 Power。

## 联机友好的基本做法

沿用传入的 `PlayerChoiceContext`，随机目标与洗牌使用游戏的 RNG；不要使用 `new Random()`、当前时间或本机 UI 状态决定玩法。
需要等待玩家选择时使用游戏选择命令，避免直接读取本机选中的节点。
这些约定保留联机兼容方向，但不构成“已验证联机”的结论。
