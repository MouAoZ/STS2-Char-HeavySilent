# 原版效果与接入参考

核对日期：2026-10-26。依据本机 `v0.111.0`／public-beta、Steam build `24724944`，程序集指纹见[版本基线](../../../../docs/development/baseline.json)；BaseLib 为3.4.7。本页记录已接入内容的原版入口、共用封装和补丁边界。玩法以[设计正文](../../../../docs/design/README.md)为准，实际验证与未测范围见[项目状态](../../../../docs/development/status.md)，代码对照本身不代表实机验收。

## 查找与复用顺序

1. 用本机中文本地化找到原版卡名对应的 ID，再用英文名和类型列表定位实现；本地化只证明展示文案，规则以对应版本的调用链核对。
2. 从卡牌的 `OnPlay` 或 Power 的 Hook 向下追到游戏 Command、选择、RNG、伤害结果和保存流程。确认哪些事件会触发，哪些只改变牌堆位置。
3. 能直接调用现有 Command／Hook／Power／Enchantment 就复用。需要组合时只编排这些原版操作；角色注册等扩展优先使用项目已经依赖的 BaseLib 接口。
4. 只有原版确实缺少能力或不能满足明确设计时才添加本项目实现，说明已检查的原版入口及差异。不为已有机制引入新的外部代码或依赖，也不整份复制原版类来维护。
5. 策划的明确规则与参考卡不一致时保留策划答案，记录具体差异；技术事实由 Agent 查证，剩下的玩法选择进入协作页。不要把“参照原版”变成让人类填写 API 名称的任务。

基本查询命令见[构建参考](build-debug.md#查询一个不确定的-api)。本次选取的反编译类型与本地化证据保存在忽略目录 `.cache/collaboration-review-20261025/`；目录日期为本次调查开始日。`evidence.json` 记录实际读取的程序集及类型哈希，`localization-evidence.json` 记录从游戏 PCK 提取且校验通过的10张文本表。缓存不进入玩家包或 Git；新机器应按类型名重新查询。

## 已核对的效果入口

下表游戏类型均位于 `MegaCrit.Sts2.Core`；`Models.Cards`、`Models.Powers` 等为实际命名空间。BaseLib 类型另列完整名称。

| 参考 | 已读取的入口 | 可以复用或必须保留的行为 |
| --- | --- | --- |
| 地狱狂徒／Hellraiser | `Models.Powers.HellraiserPower.AfterCardDrawnEarly`；`Hooks.Hook.AfterCardDrawn` | 早期抽牌 Hook 中 `await CardCmd.AutoPlay`，插入结算完成后才继续；适合参考磨顶中途触发的时序 |
| 乱战／Mayhem | `Models.Powers.MayhemPower` → `Commands.CardPileCmd.AutoPlayFromDrawPile` → `Commands.CardCmd.AutoPlay` | `AutoPlay` 对无指定目标的 `AnyEnemy` 使用 `RunState.Rng.CombatTargets` 选择合法敌人；沿用自动出牌与战斗结束保护 |
| 愤怒／Anger | `Models.Cards.Anger.OnPlay` → `Models.CardModel.CreateClone`、`DeepCloneFields` | 复制当前战斗实例，再经 `AddGeneratedCardToCombat` 入堆；保留原版复制规则，包括升级、动态变量、本地关键词、费用修正、附魔／负面附魔，不手写拷贝清单 |
| 守墓人／Grave Warden | `Models.Cards.GraveWarden.OnPlay` | 使用 `AddGeneratedCardsToCombat(..., PileType.Draw, ..., CardPilePosition.Random)`；随机插入沿用游戏 Shuffle RNG |
| 刀刃之舞／Blade Dance | `Models.Cards.BladeDance.OnPlay`；`Commands.CardPileCmd.Add` | 生成牌走原版入手流程；手牌满时 Add 将溢出牌转入弃牌堆 |
| 下去！／Begone | `Models.Cards.Begone.OnPlay` → `Commands.CardCmd.Transform` | 原版战斗内转化会记录 `History.CardGenerated` 并调用 `AfterCardGeneratedForCombat`；不再额外派发一次生成事件 |
| 普通抽牌 | `Commands.CardPileCmd.DrawInternal`；`Hooks.Hook.AfterCardDrawn` | 检查禁抽与手牌上限、逐张读取当前顶牌、记录抽牌并调用早期／普通抽牌 Hook；该方法内部会按需洗牌，没有“不洗牌”参数 |
| 拳斗／Fisticuffs | `Models.Cards.Fisticuffs.OnPlay`；`Entities.Creatures.DamageResult` | 从攻击结果汇总 `TotalDamage + OverkillDamage`；`TotalDamage = BlockedDamage + UnblockedDamage`，口径包含格挡吸收与过量伤害 |
| 狩猎／The Hunt | `Models.Cards.TheHunt.OnPlay` | 攻击前检查目标 Power 的 `ShouldOwnerDeathTriggerFatal()`，攻击后检查结果 `WasTargetKilled`；提示使用 `StaticHoverTip.Fatal`，不要只用剩余 HP 判斩杀 |
| 散射炮／Flak Cannon | `Models.Cards.FlakCannon.GetStatuses`、`OnPlay` | 枚举战斗 `AllCards`，明确排除 `PileType.Exhaust`，消耗后按次数攻击随机敌人；本项目的范围与目标必须另按设计处理 |
| 侧步／Sidestep | `Models.Cards.Sidestep.OnPlay`；`Models.Powers.EnergyNextTurnPower.AfterEnergyReset` | 现在记录固定 Amount，下个己方回合能量重置后发放并移除 Power；这张牌没有读取所选卡牌费用的逻辑 |
| 汇流／Convergence | `Models.Cards.Convergence.OnPlay`；`Models.Powers.RetainHandPower` | 一回合全手牌保留通过阻止手牌清理实现，回合结束后递减；不永久给每张牌添加关键词 |
| 响指／Snap | `Models.Cards.Snap.OnPlay` → `CardCmd.ApplyKeyword` | 给战斗中的选中牌实例添加 `CardKeyword.Retain`；持久牌组修改需要作用于 Deck 实例并走保存流程 |
| 当前费用与 X | `Entities.Cards.CardEnergyCost.GetWithModifiers`、`GetAmountToSpend`、`GetResolved`；`Models.CardModel.ResolveEnergyXValue` | 普通实际支付费用下限为0；X 区分当前可支付能量与本次出牌捕获值；效果修正沿用游戏 Hook |
| 回合与结算顺序 | `Combat.CombatState.RoundNumber`、`IterateHookListeners`；`Hooks.Hook.AfterSideTurnEnd`；`Combat.CombatManager` | RoundNumber 从1开始；同一角色的 Power 在遗物之前枚举，Hook 还区分常规／Late 阶段和选择上下文，不能压成自建全局排序器 |
| 吹哨／Whistle：击晕 | `Models.Cards.Whistle.OnPlay` → `Commands.CreatureCmd.Stun` → `Entities.Creatures.Creature.StunInternal` | 使用原版怪物状态机的 STUNNED 行动，下一回合无法行动；死亡目标不处理、不能击晕玩家，提示用 `StunIntent.GetStaticHoverTip()` |
| 完美契合／Perfect Fit | `Models.Enchantments.PerfectFit.ModifyShuffleOrder` | 原版附魔；在非初始洗牌时将该牌放到抽牌堆顶。本地化键位于 `enchantments`，不是新造的卡牌关键词 |

`HellraiserPower` 中还存在针对无限生命目标的每回合自动打出上限；这是该原版 Power 的特殊保护，不据此给所有磨顶或重打卡暗加统一次数上限。“预见”在本次 STS2 类型与中文卡表中未定位到对应实现，仅作为策划的弃牌语义类比，不能称作本机已验证的 Command。

## 先古内容沿用原版流程

| 内容 | 原版路径 | 现有 BaseLib 接入点 |
| --- | --- | --- |
| 初始遗物升级 | `Models.Events.Orobas` → `Models.Relics.TouchOfOrobas.SetupForPlayer/AfterObtained` → `RelicCmd.Replace` | 初始遗物继承现有 `BaseLib.Abstracts.CustomRelicModel`，覆盖 `GetUpgradeReplacement()` |
| 特殊初始牌替换 | `Orobas` → `Models.Relics.ArchaicTooth`：从永久 Deck 找映射牌，转化时保留升级及已有附魔 | 初始牌实现 `BaseLib.Abstracts.ITranscendenceCard.GetTranscendenceTransformedCard()` |
| 达弗专属先古卡 | `Models.Events.Darv` → `Models.Relics.DustyTome`：选择职业先古卡，升级后加入永久 Deck | 先古卡实现 `BaseLib.Abstracts.ITomeCard`，`TomeCharacter` 指向本角色 |

三条路径分别接入砥石→玉砥、偏锋→合流、易脉+，对应 BaseLib 的 `StarterUpgradePatches`、`ArchaicToothTranscendenceUpgradesPatch`、`DustyTomePatch`。获取及永久 Deck 保存仍走原版流程，不另建先古事件。合流在 `ModifyShuffleOrder` 中委托原版 `PerfectFit`，同时保留偏锋继承来的附魔槽；提示也取自原版附魔。

铭刻与赠印的永久关键词另由 BaseLib `CardModifier` 扩展保存。`PermanentKeywordModifier` 使用稳定位标记 `Retain=1`、`Innate=2`，在载入、升级和降级后重新应用，并随 BaseLib 的克隆／序列化流程传递。直接 `AddKeyword` 只修改实例，不能独自证明保存续玩后仍存在；普通战斗保留不使用这层永久修正。

## 设计差异与实现边界

- [MillCmd](../../../../mod/HeavySilentCode/Mechanics/MillCmd.cs)复用 `CardPileCmd.Add`，不派发普通弃牌事件；调用者检查 `Success` 以截断余下效果。`IMilledCard` 每移动一张后等待插入结算。`MillResult.Amount` 为锁定的最终数值，`Cards` 为实际移动集合，`OrdinalThisTurn` 让首次遗物不受嵌套计数干扰；先觉及中途空堆规则见[机制正文](../../../../docs/design/mechanics.md#磨顶-x实现约束)。
- [RecruitCmd](../../../../mod/HeavySilentCode/Mechanics/RecruitCmd.cs)逐次检查非空抽牌堆，再执行原版单张 Draw。每次等待早期／普通抽牌 Hook 后重读牌顶，不一次调用 `Draw(min(X, 初始牌数))`，也不拦截插入效果自身的洗牌。
- [涤尘](../../../../mod/HeavySilentCode/Cards/Attacks/Purge.cs)排除消耗堆并按实际消耗数量攻击同一目标；仅复用散射炮的范围，不沿用其随机目标。[伏笔](../../../../mod/HeavySilentCode/Cards/Skills/Foresight.cs)在选牌完成后采样费用，交给 `EnergyNextTurnPower` 延迟发放。
- [MillPayment](../../../../mod/HeavySilentCode/Mechanics/MillPayment.cs)在出牌资源阶段捕获差额与 X，保留实际能量支出，进入原版出牌包装前执行磨顶。若插入触发导致支付磨顶失败，按[替代支付规则](../../../../docs/design/mechanics.md#替代支付修正与原版术语)继续原版打出和归堆，只跳过源卡 `OnPlay` 效果；原先已发生的抽牌与费用不回滚。
- [RewardContent](../../../../mod/HeavySilentCode/Mechanics/RewardContent.cs)分开处理赠印的奖励出现、舍离／兼收的实际奖励选择与最终入 Deck。异步作用域隔离并行选择，兼收附赠不会再次触发“选择舍离”；若原版入组 Hook 替换实例，永久关键词转移到最终获得实例。

## 当前补丁边界

以下均由 `ModEntry.Initialize` 的 `Harmony.PatchAll` 安装，目标为上方本机基线。升级游戏后须重新反编译目标并验证，不能只凭编译通过判断运行时补丁仍有效。永久关键词复用现有 BaseLib `CardModifier`，没有为保存格式另加本项目补丁。

| 实现 | 目标方法 | 原版入口不足与适配范围 |
| --- | --- | --- |
| [MillTurnState](../../../../mod/HeavySilentCode/Mechanics/MillTurnState.cs) | `Hook.BeforeSideTurnStart`、`AfterCardPlayed`、`AfterCardChangedPiles`、`AfterShuffle` 的 Prefix | 按 `PlayerCombatState` 记录回合计数，在原版监听前更新；重置先于己方 `BeforeHandDraw`、起手 Draw 与 `AfterPlayerTurnStart`，牌堆变化刷新动态费用。避免依赖某张手牌或可移除 Power 承担全局计数 |
| [MillPayment](../../../../mod/HeavySilentCode/Mechanics/MillPayment.cs) | `PlayerCombatState.HasEnoughResourcesFor`、`CardModel.SpendResources`、`CardEnergyCost.GetAmountToSpend`、`CardModel.OnPlayWrapper` | 原版只提供不足能量转星星，没有可等待抽牌／选择的磨顶支付入口；适配资格、实际支付、锁定 X 与待支付状态，不改普通卡费用 |
| 同上 | `CardModel.OnPlayWrapper` 的异步状态机 `MoveNext` 中唯一 `CardModel.OnPlay` 调用 | 支付失败时跳过源卡效果，同时让原版历史、打出 Hook、消耗／弃置、费用清理继续运行；Transpiler 要求恰好匹配一次，否则显式报错，避免静默套用变更后的 IL |
| [RewardPatches](../../../../mod/HeavySilentCode/Patches/RewardPatches.cs) | `CardReward.Populate`、`CardReward.OnSelect`、单张 `CardPileCmd.Add(CardModel, PileType, CardPilePosition, AbstractModel, bool)` | 奖励候选不是原版 Hook 监听者，也没有所选卡回调；仅在相应奖励作用域识别候选入 Deck，保留原版选择、同步、UI 和获得 Hook，不将普通生成或购买当作选择 |
