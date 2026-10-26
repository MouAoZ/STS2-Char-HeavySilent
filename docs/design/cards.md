# 卡牌设计

更新日期：2026-10-26。本页维护卡牌设计、实现状态与源码；共用规则引用[特殊机制设计](mechanics.md)，状态含义与汇总见[设计到实现跟踪](implementation-tracker.md)。原始输入保留在[历史草稿](archive/设计草稿v0.1.md)，后续直接修改本页。

77张设计卡的效果、升级、注册和双语本地化均已落地，源码与构建检查通过。实机覆盖和未测边界见[项目状态](../development/status.md)；“已实现”不表示全部组合、完整爬塔、平衡或联机已验收。

## 分类与阅读约定

- 初始卡组、角色卡牌、状态牌、诅咒牌、先古卡、其他特殊奖励卡分别登记。攻击／技能／能力是牌型；初始或先古卡也可以具有这些牌型。
- 角色卡牌默认进入卡牌奖励，分类名称不加“奖励”前缀；初始牌、状态牌、诅咒牌等例外单独标注。先古卡、事件专属卡等记录各自获取方式。
- 每张卡只保留一份设计正文，初始卡组和获取它的事件／遗物通过编号引用；升级版与起始重复张数不另计。
- 费用独立成列，括号内为升级信息；白／蓝／金对应普通／罕见／稀有。[local #8](../collaboration/issues/8.md) 已确认的升级与可打出性写入下表；Issue 保留讨论与决策来源。
- 中文名和英文名单列。打击／Strike、防御／Defend、气／Qi 沿用当前游戏显示名；其余74张卡的中英文名均为 **Agent 拟定的暂名，待策划定稿**。
- 暂名直接填写在名称栏，不再另设检索短名。后续定名时替换中英文名即可，设计编号、页内锚点与已进入存档的模型 ID 保持稳定；名称不构成玩法规则，也不表示效果已实现。
- 未实现项的模型 ID／代码填“—”；实现时登记真实源码。气的数值来自原始设计；打击／防御数值来自现有教学样例。

## 初始卡组

设计配置为五张打击、五张防御和下列两张特殊初始牌各一张，已接入[角色代码](../../mod/HeavySilentCode/Character/HeavySilentCharacter.cs)。这四种初始牌不进入常规卡牌奖励，起始遗物见[遗物设计](relics-events-potions.md#hs-relic-001)。

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-base-001"></a>HS-BASE-001 | 打击 | Strike | 1 | 基础攻击，造成6（9）点伤害；打击标签。 | 已实现 | `HEAVYSILENT_STRIKE`<br>[HeavyStrike](../../mod/HeavySilentCode/Cards/HeavyStrike.cs) | 起始5张；数值沿用教学样例，非草稿新增约定。 |
| <a id="hs-base-002"></a>HS-BASE-002 | 防御 | Defend | 1 | 基础技能，获得5（8）点格挡；防御标签。 | 已实现 | `HEAVYSILENT_DEFEND`<br>[HeavyDefend](../../mod/HeavySilentCode/Cards/HeavyDefend.cs) | 起始5张；数值沿用教学样例，非草稿新增约定。 |
| <a id="hs-start-001"></a>HS-START-001 | 凝息 | Gather Breath | 1（0） | 技能 获得5点格挡，将一张气加入抽牌堆 | 已实现 | `HEAVYSILENT_GATHER_BREATH`<br>[GatherBreath](../../mod/HeavySilentCode/Cards/GatherBreath.cs) | [local #3](../collaboration/issues/3.md)：气插入抽牌堆的位置。 |
| <a id="hs-start-002"></a>HS-START-002 | 偏锋 | Oblique Strike | 1（0） | 攻击 磨顶1 造成伤害，数值等于你抽牌堆与弃牌堆牌数之差的绝对值。 | 已实现 | `HEAVYSILENT_OBLIQUE_STRIKE`<br>[ObliqueStrike](../../mod/HeavySilentCode/Cards/ObliqueStrike.cs) | 磨顶；[local #1](../collaboration/issues/1.md)。欧罗巴斯转化为[合流](#hs-ancient-001)。 |

## 角色卡牌

以下按攻击／技能／能力以及稀有度组织，默认进入卡牌奖励。

### 攻击牌

#### 攻击牌 · 白卡

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-atk-001"></a>HS-ATK-001 | 断流 | Sever | 1 | 磨顶3 造成13（16）点伤害。 | 已实现 | `HEAVYSILENT_SEVER`<br>[Sever](../../mod/HeavySilentCode/Cards/Attacks/Sever.cs) | 磨顶；[local #1](../collaboration/issues/1.md)。 |
| <a id="hs-atk-002"></a>HS-ATK-002 | 乱刃 | Wild Blades | 1 | 随机造成7（9）点伤害 磨顶4 随机造成7（9）点伤害。 | 已实现 | `HEAVYSILENT_WILD_BLADES`<br>[WildBlades](../../mod/HeavySilentCode/Cards/Attacks/WildBlades.cs) | 磨顶；[local #1](../collaboration/issues/1.md)；两段各用游戏随机目标。 |
| <a id="hs-atk-003"></a>HS-ATK-003 | 追索 | Pursuit | 1 | 造成8（9）点伤害 ，征召1（2）。 | 已实现 | `HEAVYSILENT_PURSUIT`<br>[Pursuit](../../mod/HeavySilentCode/Cards/Attacks/Pursuit.cs) | 征召；[local #2](../collaboration/issues/2.md)。 |
| <a id="hs-atk-004"></a>HS-ATK-004 | 破风 | Windbreak | 1 | 造成9（10）点伤害，磨顶4，将1张气（+）加入手牌。 | 已实现 | `HEAVYSILENT_WINDBREAK`<br>[Windbreak](../../mod/HeavySilentCode/Cards/Attacks/Windbreak.cs) | 磨顶、生成气；[local #1](../collaboration/issues/1.md)、[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-atk-005"></a>HS-ATK-005 | 气刃 | Qi Blade | 0 | 只有在手牌中有气才能打出。消耗手牌中一张气，造成14（17）点伤害。 | 已实现 | `HEAVYSILENT_QI_BLADE`<br>[QiBlade](../../mod/HeavySilentCode/Cards/Attacks/QiBlade.cs) | 检查气、选牌消耗；[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-atk-006"></a>HS-ATK-006 | 余息 | Lingering Breath | 1 | 造成7（9）点伤害，向弃牌堆加入2（3）张气。 | 已实现 | `HEAVYSILENT_LINGERING_BREATH`<br>[LingeringBreath](../../mod/HeavySilentCode/Cards/Attacks/LingeringBreath.cs) | 生成气；[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-atk-007"></a>HS-ATK-007 | 重压 | Pressure | 2 | 造成伤害，数值等于抽牌堆与弃牌堆中牌数较大者（+5）。 | 已实现 | `HEAVYSILENT_PRESSURE`<br>[Pressure](../../mod/HeavySilentCode/Cards/Attacks/Pressure.cs) | 牌堆计数。 |
| <a id="hs-atk-008"></a>HS-ATK-008 | 卸劲 | Sap | 1 | 造成3（4）点伤害，施加等同于造成伤害数的层数的虚弱。 | 已实现 | `HEAVYSILENT_SAP`<br>[Sap](../../mod/HeavySilentCode/Cards/Attacks/Sap.cs) | [local #4](../collaboration/issues/4.md)：伤害口径。 |
| <a id="hs-atk-009"></a>HS-ATK-009 | 横扫 | Sweep | 0 | 磨顶2 对全体敌人造成7（9）点伤害。 | 已实现 | `HEAVYSILENT_SWEEP`<br>[Sweep](../../mod/HeavySilentCode/Cards/Attacks/Sweep.cs) | 磨顶；[local #1](../collaboration/issues/1.md)。 |
| <a id="hs-atk-010"></a>HS-ATK-010 | 三叠 | Threefold | 1 | 对一名敌人造成4（5）点伤害 磨顶2 对一名随机敌人造成4（5）点伤害 磨顶2 对所有敌人造成4（5）点伤害。 | 已实现 | `HEAVYSILENT_THREEFOLD`<br>[Threefold](../../mod/HeavySilentCode/Cards/Attacks/Threefold.cs) | 磨顶；[local #1](../collaboration/issues/1.md)：两次失败与后续段落。 |
| <a id="hs-atk-011"></a>HS-ATK-011 | 沉锋 | Heavy Edge | 3 | 造成12（15）点伤害 你的弃牌堆每有9张牌，这张牌的费用-1. | 已实现 | `HEAVYSILENT_HEAVY_EDGE`<br>[HeavyEdge](../../mod/HeavySilentCode/Cards/Attacks/HeavyEdge.cs) | 动态费用；[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-atk-012"></a>HS-ATK-012 | 回锋 | Returning Edge | 1 | 造成5点伤害，从弃牌堆将一张牌加入手牌。消耗（删去消耗）。 | 已实现 | `HEAVYSILENT_RETURNING_EDGE`<br>[ReturningEdge](../../mod/HeavySilentCode/Cards/Attacks/ReturningEdge.cs) | 弃牌选牌；空弃牌堆边界。 |

#### 攻击牌 · 蓝卡

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-atk-013"></a>HS-ATK-013 | 伏刃 | Hidden Blade | 0 | 造成4（6）点伤害，征召1.若此牌因磨顶而进入弃牌堆，将其打出。 | 已实现 | `HEAVYSILENT_HIDDEN_BLADE`<br>[HiddenBlade](../../mod/HeavySilentCode/Cards/Attacks/HiddenBlade.cs) | 磨顶、征召、自动出牌；[local #1](../collaboration/issues/1.md)、[local #2](../collaboration/issues/2.md)。 |
| <a id="hs-atk-014"></a>HS-ATK-014 | 铭刻 | Etch | 1 | 消耗 造成12（16）点伤害。斩杀时，选择卡组中一张牌获得保留。 | 已实现 | `HEAVYSILENT_ETCH`<br>[Etch](../../mod/HeavySilentCode/Cards/Attacks/Etch.cs) | 斩杀判定、卡组选牌范围；[local #4](../collaboration/issues/4.md)、[local #5](../collaboration/issues/5.md)。 |
| <a id="hs-atk-015"></a>HS-ATK-015 | 化息 | Transmute | 1 | 造成7（8）点伤害，将弃牌堆中的一张牌转化为气（+）。 | 已实现 | `HEAVYSILENT_TRANSMUTE`<br>[Transmute](../../mod/HeavySilentCode/Cards/Attacks/Transmute.cs) | 弃牌选牌、转化；[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-atk-016"></a>HS-ATK-016 | 烬火 | Embers | 1 | 造成6点伤害。消耗堆每有一张气，此牌伤害增加3（5）。 | 已实现 | `HEAVYSILENT_EMBERS`<br>[Embers](../../mod/HeavySilentCode/Cards/Attacks/Embers.cs) | 消耗堆气计数（含气+的口径见 [local #3](../collaboration/issues/3.md)）。 |
| <a id="hs-atk-017"></a>HS-ATK-017 | 连斩 | Flurry | 1 | 造成5（6）点伤害。本回合每进行过一次磨顶，此牌便多造成一次伤害。 | 已实现 | `HEAVYSILENT_FLURRY`<br>[Flurry](../../mod/HeavySilentCode/Cards/Attacks/Flurry.cs) | 磨顶次数；[local #1](../collaboration/issues/1.md)、[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-atk-018"></a>HS-ATK-018 | 先声 | Opening Note | 0 | 固有 消耗 造成6（7）点伤害，向手牌中添加一张气（+）。 | 已实现 | `HEAVYSILENT_OPENING_NOTE`<br>[OpeningNote](../../mod/HeavySilentCode/Cards/Attacks/OpeningNote.cs) | 生成气；[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-atk-019"></a>HS-ATK-019 | 攻守 | Poise | 2 | 虚无（删去虚无）造成伤害，数值等于弃牌堆中牌的数量；获得格挡，数值等于抽牌堆中牌的数量。 | 已实现 | `HEAVYSILENT_POISE`<br>[Poise](../../mod/HeavySilentCode/Cards/Attacks/Poise.cs) | 牌堆计数；升级删去虚无。 |
| <a id="hs-atk-020"></a>HS-ATK-020 | 落潮 | Ebb | 1 | 磨顶6 造成伤害，数值等于本回合进入弃牌堆的牌的总数（+3）。 | 已实现 | `HEAVYSILENT_EBB`<br>[Ebb](../../mod/HeavySilentCode/Cards/Attacks/Ebb.cs) | 磨顶、入堆计数；[local #1](../collaboration/issues/1.md)、[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-atk-021"></a>HS-ATK-021 | 裂息 | Split Breath | 1 | 造成6（8）点伤害，向抽牌堆和弃牌堆分别加入等于此牌造成伤害一半的气。 | 已实现 | `HEAVYSILENT_SPLIT_BREATH`<br>[SplitBreath](../../mod/HeavySilentCode/Cards/Attacks/SplitBreath.cs) | [local #3](../collaboration/issues/3.md)、[local #4](../collaboration/issues/4.md)：伤害、取整与插牌位置。 |
| <a id="hs-atk-022"></a>HS-ATK-022 | 回澜 | Resurgence | 1 | 造成7（8）点伤害。若抽牌堆本回合洗过牌，额外造成两次伤害。 | 已实现 | `HEAVYSILENT_RESURGENCE`<br>[Resurgence](../../mod/HeavySilentCode/Cards/Attacks/Resurgence.cs) | 洗牌记录、同一敌人；升级已按[local #8](../collaboration/issues/8.md)确认。 |
| <a id="hs-atk-023"></a>HS-ATK-023 | 舍离 | Severance | 1 | 造成10（13）点伤害。当你将这张牌加入牌组时，选择牌组中的一张牌删除。 | 已实现 | `HEAVYSILENT_SEVERANCE`<br>[Severance](../../mod/HeavySilentCode/Cards/Attacks/Severance.cs) | 获得卡牌生命周期；[local #5](../collaboration/issues/5.md)。 |

#### 攻击牌 · 金卡

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-atk-024"></a>HS-ATK-024 | 归零 | Nullity | X | （保留） 磨顶X 若抽牌堆中牌数为0，对所有敌人造成50点伤害。 | 已实现 | `HEAVYSILENT_NULLITY`<br>[Nullity](../../mod/HeavySilentCode/Cards/Attacks/Nullity.cs) | 磨顶；[local #1](../collaboration/issues/1.md)：X、磨顶0与空堆。 |
| <a id="hs-atk-025"></a>HS-ATK-025 | 轮斩 | Blade Cycle | 2 | 对所有敌人造成6（7）点伤害 磨顶2 将这张牌再打出一次。 | 已实现 | `HEAVYSILENT_BLADE_CYCLE`<br>[BladeCycle](../../mod/HeavySilentCode/Cards/Attacks/BladeCycle.cs) | 磨顶、自动重打；[local #1](../collaboration/issues/1.md)：终止与目标死亡。 |
| <a id="hs-atk-026"></a>HS-ATK-026 | 掠影 | Fleeting Edge | 0 | 造成5（6）点伤害，抽2（3）张牌. 每当你磨顶时，将这张牌从抽牌堆加入手牌。 | 已实现 | `HEAVYSILENT_FLEETING_EDGE`<br>[FleetingEdge](../../mod/HeavySilentCode/Cards/Attacks/FleetingEdge.cs) | 磨顶监听、普通抽牌；[local #1](../collaboration/issues/1.md)：触发顺序。 |
| <a id="hs-atk-027"></a>HS-ATK-027 | 寸劲 | One Inch | 5（4） | 造成66点伤害。本回合每打出过一张气，这张牌的耗能便减1. | 已实现 | `HEAVYSILENT_ONE_INCH`<br>[OneInch](../../mod/HeavySilentCode/Cards/Attacks/OneInch.cs) | 打出气计数；[local #6](../collaboration/issues/6.md)；本稿没有保留关键词。 |
| <a id="hs-atk-028"></a>HS-ATK-028 | 涤尘 | Purge | 2 | 消耗战斗牌堆中除消耗堆以外的所有状态牌。每消耗一张状态牌，对同一名敌人造成7（9）点伤害。 | 已实现 | `HEAVYSILENT_PURGE`<br>[Purge](../../mod/HeavySilentCode/Cards/Attacks/Purge.cs) | 沿用散射炮的牌堆范围；已消耗状态牌不再计数，见[伤害与牌堆规则](mechanics.md#伤害斩杀与目标)。 |
| <a id="hs-atk-029"></a>HS-ATK-029 | 回光 | Afterglow | 1 | 造成40点伤害 磨顶5（7） 令这名敌人恢复40点生命。 | 已实现 | `HEAVYSILENT_AFTERGLOW`<br>[Afterglow](../../mod/HeavySilentCode/Cards/Attacks/Afterglow.cs) | 磨顶；[local #1](../collaboration/issues/1.md)、[local #4](../collaboration/issues/4.md)：失败、斩杀与治疗时机。 |

### 技能牌

#### 技能牌 · 白卡

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-skl-001"></a>HS-SKL-001 | 沉壁 | Stonewall | 1 | 磨顶3 获得11（14）点格挡。 | 已实现 | `HEAVYSILENT_STONEWALL`<br>[Stonewall](../../mod/HeavySilentCode/Cards/Skills/Stonewall.cs) | 磨顶；[local #1](../collaboration/issues/1.md)。 |
| <a id="hs-skl-002"></a>HS-SKL-002 | 叠障 | Layered Guard | 1 | 获得5（7）点格挡 磨顶2  获得5（7）点格挡。 | 已实现 | `HEAVYSILENT_LAYERED_GUARD`<br>[LayeredGuard](../../mod/HeavySilentCode/Cards/Skills/LayeredGuard.cs) | 磨顶失败跳过后段，保留前段格挡与费用；[规则](mechanics.md#磨顶-x实现约束)。 |
| <a id="hs-skl-003"></a>HS-SKL-003 | 藏息 | Hidden Breath | 1 | 获得8（9）点格挡，磨顶4，向抽牌堆加入一张气（+）。 | 已实现 | `HEAVYSILENT_HIDDEN_BREATH`<br>[HiddenBreath](../../mod/HeavySilentCode/Cards/Skills/HiddenBreath.cs) | 磨顶、生成气；[local #1](../collaboration/issues/1.md)、[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-skl-004"></a>HS-SKL-004 | 整备 | Readiness | 1 | 获得5（8）点格挡，征召2. | 已实现 | `HEAVYSILENT_READINESS`<br>[Readiness](../../mod/HeavySilentCode/Cards/Skills/Readiness.cs) | 征召；[local #2](../collaboration/issues/2.md)。 |
| <a id="hs-skl-005"></a>HS-SKL-005 | 旧垒 | Old Rampart | 1（0） | 消耗 获得格挡，数值等于你的弃牌堆牌数。 | 已实现 | `HEAVYSILENT_OLD_RAMPART`<br>[OldRampart](../../mod/HeavySilentCode/Cards/Skills/OldRampart.cs) | 牌堆计数。 |
| <a id="hs-skl-006"></a>HS-SKL-006 | 露隙 | Opening | 1 | 获得5（6）点格挡，对所有敌人施加1（2）层易伤。 | 已实现 | `HEAVYSILENT_OPENING`<br>[Opening](../../mod/HeavySilentCode/Cards/Skills/Opening.cs) | 格挡、群体状态。 |
| <a id="hs-skl-007"></a>HS-SKL-007 | 分影 | Split Shadow | 0 | 消耗 获得2（3）点格挡。当你抽到这张牌时，将其两张复制加入你的手牌。 | 已实现 | `HEAVYSILENT_SPLIT_SHADOW`<br>[SplitShadow](../../mod/HeavySilentCode/Cards/Skills/SplitShadow.cs) | 抽牌触发、复制；[local #2](../collaboration/issues/2.md)、[local #3](../collaboration/issues/3.md)：复制是否触发连锁。 |
| <a id="hs-skl-008"></a>HS-SKL-008 | 蓄势 | Bide | 1 | 在本回合保留你的手牌。下个回合开始时，获得1（2）费。 | 已实现 | `HEAVYSILENT_BIDE`<br>[Bide](../../mod/HeavySilentCode/Cards/Skills/Bide.cs) | 保留、延迟费用；[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-skl-009"></a>HS-SKL-009 | 吐纳 | Breathwork | 1 | 磨顶6（8），打出其中所有的气。 | 已实现 | `HEAVYSILENT_BREATHWORK`<br>[Breathwork](../../mod/HeavySilentCode/Cards/Skills/Breathwork.cs) | 磨顶、自动出牌；[local #1](../collaboration/issues/1.md)、[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-skl-010"></a>HS-SKL-010 | 轻身 | Lightfoot | 2 | 获得12（16）点格挡，获得1点敏捷。 | 已实现 | `HEAVYSILENT_LIGHTFOOT`<br>[Lightfoot](../../mod/HeavySilentCode/Cards/Skills/Lightfoot.cs) | 格挡、敏捷。 |
| <a id="hs-skl-011"></a>HS-SKL-011 | 留影 | Imprint | 1 | 选择手牌中的一张攻击或技能牌，将其1（2）张复制加入抽牌堆。 | 已实现 | `HEAVYSILENT_IMPRINT`<br>[Imprint](../../mod/HeavySilentCode/Cards/Skills/Imprint.cs) | 选牌、复制；[local #3](../collaboration/issues/3.md)。 |

#### 技能牌 · 蓝卡

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-skl-012"></a>HS-SKL-012 | 砺甲 | Honed Armor | 3 | 获得8（11）点格挡。本回合每触发过一次磨顶，此牌的耗能便减1. | 已实现 | `HEAVYSILENT_HONED_ARMOR`<br>[HonedArmor](../../mod/HeavySilentCode/Cards/Skills/HonedArmor.cs) | 磨顶计数、动态费用；[local #1](../collaboration/issues/1.md)、[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-skl-013"></a>HS-SKL-013 | 匀息 | Even Breath | 1（0） | 向抽牌堆或弃牌堆加入气，直至两个牌堆中气的数量相等。获得等于加入气的数量的格挡。 | 已实现 | `HEAVYSILENT_EVEN_BREATH`<br>[EvenBreath](../../mod/HeavySilentCode/Cards/Skills/EvenBreath.cs) | 气计数、生成；[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-skl-014"></a>HS-SKL-014 | 兼收 | Windfall | 1 | 获得9（12）点格挡。当你在卡牌奖励中选择这张牌时，将这组奖励中余下的牌也加入你的牌组。 | 已实现 | `HEAVYSILENT_WINDFALL`<br>[Windfall](../../mod/HeavySilentCode/Cards/Skills/Windfall.cs) | 奖励生命周期；[local #5](../collaboration/issues/5.md)。 |
| <a id="hs-skl-015"></a>HS-SKL-015 | 拣锋 | Chosen Edge | 0 | 消耗 磨顶3（4），选择其中一张牌打出。 | 已实现 | `HEAVYSILENT_CHOSEN_EDGE`<br>[ChosenEdge](../../mod/HeavySilentCode/Cards/Skills/ChosenEdge.cs) | 磨顶、选牌、自动出牌；[local #1](../collaboration/issues/1.md)。 |
| <a id="hs-skl-016"></a>HS-SKL-016 | 养气 | Nurture Qi | 1 | 消耗 将3（4）张气加入手牌，它们获得保留。 | 已实现 | `HEAVYSILENT_NURTURE_QI`<br>[NurtureQi](../../mod/HeavySilentCode/Cards/Skills/NurtureQi.cs) | 生成气、保留；[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-skl-017"></a>HS-SKL-017 | 拾遗 | Reclaim | 1 | 磨顶3（4），将这些牌从弃牌堆加入你的手牌。 | 已实现 | `HEAVYSILENT_RECLAIM`<br>[Reclaim](../../mod/HeavySilentCode/Cards/Skills/Reclaim.cs) | 磨顶结果追踪；[local #1](../collaboration/issues/1.md)、[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-skl-018"></a>HS-SKL-018 | 燃息 | Burn Breath | 0 | （保留）选择消耗手牌中任意多张气，获得等同于它们总抽牌数的费用。 | 已实现 | `HEAVYSILENT_BURN_BREATH`<br>[BurnBreath](../../mod/HeavySilentCode/Cards/Skills/BurnBreath.cs) | 升级添加保留，已按[local #8](../collaboration/issues/8.md)确认；多选消耗与气计数见[共用规则](mechanics.md#共用交互)。 |
| <a id="hs-skl-019"></a>HS-SKL-019 | 暗涌 | Undercurrent | 2 | 获得2（3）费。若此牌因磨顶而进入弃牌堆，将其打出。 | 已实现 | `HEAVYSILENT_UNDERCURRENT`<br>[Undercurrent](../../mod/HeavySilentCode/Cards/Skills/Undercurrent.cs) | 磨顶、自动出牌；[local #1](../collaboration/issues/1.md)。 |
| <a id="hs-skl-020"></a>HS-SKL-020 | 伏笔 | Foresight | X | 消耗 选择手牌中X（+1）张牌，使它们本回合获得保留。选牌完成时记录它们的当前总费用；下回合开始时，获得等于记录金额的费用。 | 已实现 | `HEAVYSILENT_FORESIGHT`<br>[Foresight](../../mod/HeavySilentCode/Cards/Skills/Foresight.cs) | 返费复用侧步的延迟能量状态，在下个己方回合能量重置后发放；见[费用与回合规则](mechanics.md#计数费用保留与回合)。 |
| <a id="hs-skl-021"></a>HS-SKL-021 | 留招 | Reserve | 1 | 获得7（9）点格挡，为手牌中的一张牌添加保留。 | 已实现 | `HEAVYSILENT_RESERVE`<br>[Reserve](../../mod/HeavySilentCode/Cards/Skills/Reserve.cs) | 选牌；[local #6](../collaboration/issues/6.md)：保留持续范围。 |

#### 技能牌 · 金卡

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-skl-022"></a>HS-SKL-022 | 重影 | Double Vision | 0 | （固有） 保留 消耗 复制你的整个抽牌堆。 | 已实现 | `HEAVYSILENT_DOUBLE_VISION`<br>[DoubleVision](../../mod/HeavySilentCode/Cards/Skills/DoubleVision.cs) | 批量复制；[local #3](../collaboration/issues/3.md)：顺序与实例。 |
| <a id="hs-skl-023"></a>HS-SKL-023 | 止水 | Still Water | 1 | （保留） 消耗 当你的抽牌堆牌数与弃牌堆牌数相等时才能打出，击晕所有敌人。 | 已实现 | `HEAVYSILENT_STILL_WATER`<br>[StillWater](../../mod/HeavySilentCode/Cards/Skills/StillWater.cs) | 打出条件、群体击晕；[local #7](../collaboration/issues/7.md)。 |
| <a id="hs-skl-024"></a>HS-SKL-024 | 空山 | Empty Mountain | 1（0） | 磨顶整个抽牌堆。将1张气加入手牌。 | 已实现 | `HEAVYSILENT_EMPTY_MOUNTAIN`<br>[EmptyMountain](../../mod/HeavySilentCode/Cards/Skills/EmptyMountain.cs) | 磨顶、生成气；[local #1](../collaboration/issues/1.md)、[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-skl-025"></a>HS-SKL-025 | 孤注 | All In | 8（6） | 本回合你的手牌耗能变为0. 打出此牌时，不足的费用可以以磨顶4倍数量的牌代替。 | 已实现 | `HEAVYSILENT_ALL_IN`<br>[AllIn](../../mod/HeavySilentCode/Cards/Skills/AllIn.cs) | 自带磨顶支付；易脉生效时优先使用2倍磨顶，见[替代支付规则](mechanics.md#替代支付修正与原版术语)。 |
| <a id="hs-skl-026"></a>HS-SKL-026 | 归气 | Return to Qi | 2 | 消耗 将你的手牌变化为气。每因此变化一张牌，获得5（7）点格挡。 | 已实现 | `HEAVYSILENT_RETURN_TO_QI`<br>[ReturnToQi](../../mod/HeavySilentCode/Cards/Skills/ReturnToQi.cs) | 批量转化；[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-skl-027"></a>HS-SKL-027 | 回环 | Recurrence | 0 | 征召2（3）。若回合结束时这张牌在弃牌堆，将其置于牌堆顶。 | 已实现 | `HEAVYSILENT_RECURRENCE`<br>[Recurrence](../../mod/HeavySilentCode/Cards/Skills/Recurrence.cs) | 征召、回合末移动；[local #2](../collaboration/issues/2.md)、[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-skl-028"></a>HS-SKL-028 | 赠印 | Parting Mark | 0 | 不可被打出。不可升级。此牌出现在卡牌奖励时，同组的其他牌永久获得固有和保留。 | 已实现 | `HEAVYSILENT_PARTING_MARK`<br>[PartingMark](../../mod/HeavySilentCode/Cards/Skills/PartingMark.cs) | 在奖励出现阶段处理；不可打出与不可升级已按[local #8](../collaboration/issues/8.md)定案，草稿问号已移除。 |

### 能力牌

#### 能力牌 · 蓝卡

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-pow-001"></a>HS-POW-001 | 飞砂 | Flying Sand | 1 | 每当你磨顶时，随机对一名敌人造成4（6）点伤害。 | 已实现 | `HEAVYSILENT_FLYING_SAND`<br>[FlyingSand](../../mod/HeavySilentCode/Cards/Powers/FlyingSand.cs) | 磨顶监听；[local #1](../collaboration/issues/1.md)。 |
| <a id="hs-pow-002"></a>HS-POW-002 | 归序 | Restore Order | 2（1） | 回合结束时，选择弃牌堆中至多1张牌置于牌堆顶。 | 已实现 | `HEAVYSILENT_RESTORE_ORDER`<br>[RestoreOrder](../../mod/HeavySilentCode/Cards/Powers/RestoreOrder.cs) | 回合末选牌；[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-pow-003"></a>HS-POW-003 | 阴阳 | Yin and Yang | 1 | 奇数回合获得3（4）点临时力量，偶数回合获得3（4）点临时敏捷。 | 已实现 | `HEAVYSILENT_YIN_AND_YANG`<br>[YinAndYang](../../mod/HeavySilentCode/Cards/Powers/YinAndYang.cs) | 回合计数、临时属性；[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-pow-004"></a>HS-POW-004 | 气衣 | Qi Mantle | 1 | 每当你生成气时，获得2（3）点格挡。 | 已实现 | `HEAVYSILENT_QI_MANTLE`<br>[QiMantle](../../mod/HeavySilentCode/Cards/Powers/QiMantle.cs) | 生成气事件；[local #3](../collaboration/issues/3.md)。 |
| <a id="hs-pow-005"></a>HS-POW-005 | 夜息 | Night Breath | 2（1） | 回合结束时，向抽牌堆加入一张气+。 | 已实现 | `HEAVYSILENT_NIGHT_BREATH`<br>[NightBreath](../../mod/HeavySilentCode/Cards/Powers/NightBreath.cs) | 生成气；[local #3](../collaboration/issues/3.md)：插牌位置。 |
| <a id="hs-pow-006"></a>HS-POW-006 | 守拙 | Simple Guard | 2 | （固有） 回合结束时，获得等同于手牌数的格挡。 | 已实现 | `HEAVYSILENT_SIMPLE_GUARD`<br>[SimpleGuard](../../mod/HeavySilentCode/Cards/Powers/SimpleGuard.cs) | 结算顺序；[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-pow-007"></a>HS-POW-007 | 绵息 | Lasting Breath | 1（0） | 你的气获得保留。 | 已实现 | `HEAVYSILENT_LASTING_BREATH`<br>[LastingBreath](../../mod/HeavySilentCode/Cards/Powers/LastingBreath.cs) | [local #3](../collaboration/issues/3.md)、[local #6](../collaboration/issues/6.md)：已有／新生成气与保留范围。 |

#### 能力牌 · 金卡

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-pow-008"></a>HS-POW-008 | 忆招 | Recall | 1 | （固有）回合开始时，你少抽2张牌，并将弃牌堆中随机3张牌加入手牌。 | 已实现 | `HEAVYSILENT_RECALL`<br>[Recall](../../mod/HeavySilentCode/Cards/Powers/Recall.cs) | 回合开始、随机取牌；[local #3](../collaboration/issues/3.md)、[local #6](../collaboration/issues/6.md)。 |
| <a id="hs-pow-009"></a>HS-POW-009 | 微调 | Fine Tuning | 1（0） | 选择一项：你的所有磨顶牌的磨顶数值+1/-1. | 已实现 | `HEAVYSILENT_FINE_TUNING`<br>[FineTuning](../../mod/HeavySilentCode/Cards/Powers/FineTuning.cs) | 磨顶参数；[local #1](../collaboration/issues/1.md)、[local #7](../collaboration/issues/7.md)。 |
| <a id="hs-pow-010"></a>HS-POW-010 | 流壁 | Flowing Guard | 2 | （固有） 每当你磨顶时，获得等于磨顶数值的格挡。 | 已实现 | `HEAVYSILENT_FLOWING_GUARD`<br>[FlowingGuard](../../mod/HeavySilentCode/Cards/Powers/FlowingGuard.cs) | 磨顶监听；[local #1](../collaboration/issues/1.md)。 |
| <a id="hs-pow-011"></a>HS-POW-011 | 先觉 | Anticipation | 3 | （保留） 每当你磨顶时，先抽1张牌。 | 已实现 | `HEAVYSILENT_ANTICIPATION`<br>[Anticipation](../../mod/HeavySilentCode/Cards/Powers/Anticipation.cs) | 磨顶前触发；[local #1](../collaboration/issues/1.md)：检查牌数与抽牌先后。 |
| <a id="hs-pow-012"></a>HS-POW-012 | 温故 | Reflection | 1 | 每当你将一张牌保留时，将其在本场战斗中升级。（升级新增：在本回合保留你的手牌。） | 已实现 | `HEAVYSILENT_REFLECTION`<br>[Reflection](../../mod/HeavySilentCode/Cards/Powers/Reflection.cs) | 升级新增即时保留效果，已按[local #8](../collaboration/issues/8.md)确认；回合保留参照汇流。 |
| <a id="hs-pow-013"></a>HS-POW-013 | 淬新 | Fresh Forge | 1 | （固有） 你在战斗中生成的牌将升级。 | 已实现 | `HEAVYSILENT_FRESH_FORGE`<br>[FreshForge](../../mod/HeavySilentCode/Cards/Powers/FreshForge.cs) | 生成牌监听；[local #3](../collaboration/issues/3.md)。 |

## 状态牌

状态牌不进入常规卡牌奖励。气是具体卡牌，其费用、抽牌与消耗效果在本节维护；生成、复制、转化等共用规则见[机制设计](mechanics.md#生成复制与转化)，决策来源见[协作 local #3](../collaboration/issues/3.md)。

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-token-001"></a>HS-TOKEN-001 | 气 | Qi | 0 | 状态牌，抽1（2）张牌，消耗。 | 已实现 | `HEAVYSILENT_QI`<br>[Qi](../../mod/HeavySilentCode/Cards/Qi.cs) | 普通抽牌允许洗牌，易脉生效时改为征召；[结算与验收例子](teaching-samples.md)。 |

气通常使用普通抽牌并允许洗牌，易脉生效时改为征召；详细结算与验收例子见[教学样例](teaching-samples.md)。生成气、转化为气及相关监听均已接入。未明确永久性的生成／转化只影响本场战斗；气+同样计作气。

## 诅咒牌

当前没有本角色的诅咒牌设计，未分配编号，不计入现有卡牌总数。后续新增时单列获取方式及可用范围，不进入常规卡牌奖励。

## 先古卡

HS-ANCIENT-001 沿用欧罗巴斯 ArchaicTooth 的永久初始牌转化流程，由[偏锋 HS-START-002](#hs-start-002)转化为合流，保留原牌升级与已有附魔；确认来源见[local #36 补充回答](../collaboration/issues/36.md#timeline)。HS-ANCIENT-002 沿用达弗 DustyTome 流程，给予升级版并加入永久牌组。它们在原始草稿中位于“遗物一览”，现统一归入本节；欧罗巴斯的遗物升级另见[遗物设计](relics-events-potions.md#hs-relic-002)。原版流程与现有扩展接口见[效果参考](../../.agents/skills/sts2-heavysilent-dev/references/vanilla-effects.md#先古内容沿用原版流程)。

| 设计编号 | 中文名 | 英文名 | 费用 | 设计效果 | 状态 | 模型 ID／代码 | 依赖／规则与问题 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| <a id="hs-ancient-001"></a>HS-ANCIENT-001 | 合流 | Confluence | 1（0） | 完美契合 造成伤害，等于你抽牌堆与弃牌堆的牌数之和。 | 已实现 | `HEAVYSILENT_CONFLUENCE`<br>[Confluence](../../mod/HeavySilentCode/Cards/Confluence.cs) | 完美契合采用原版 PerfectFit 附魔语义；替换[偏锋](#hs-start-002)，沿用[先古获取与保存规则](mechanics.md#奖励先古获取与保存)。 |
| <a id="hs-ancient-002"></a>HS-ANCIENT-002 | 易脉 | Shifting Meridians | 1 | （固有） 费用不足时，你可以磨顶2以代替支付1费。气的抽牌改为征召。 | 已实现 | `HEAVYSILENT_SHIFTING_MERIDIANS`<br>[ShiftingMeridians](../../mod/HeavySilentCode/Cards/ShiftingMeridians.cs) | 能力牌；达弗给予升级版；支付与X值锁定见[共用规则](mechanics.md#替代支付修正与原版术语)。 |

## 其他特殊奖励卡

当前没有仅在事件或其他特定奖励中获得的新增卡牌设计，不计入现有卡牌总数。后续在本节记录卡牌效果、状态、源码和获取入口；[事件设计](relics-events-potions.md#事件)通过卡牌编号引用，不重复维护效果。

## local Issue 引用

问题与讨论从[协作入口](../collaboration/README.md)进入，本页各条目直接链接对应 local Issue；已确认的共用规则在[特殊机制设计](mechanics.md#共用交互)维护。

升级与可打出性已按 [local #8](../collaboration/issues/8.md) 写回并实现于回澜、燃息、赠印和温故。设计结论、实现状态与实机证据分别维护。
