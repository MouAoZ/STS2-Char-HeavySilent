using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using HeavySilent.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HeavySilent.Character;

/// <summary>独立角色。表现借用静默，数值和套牌仅用于验证工程。</summary>
[CustomID("HEAVYSILENT_CHARACTER")]
public sealed class HeavySilentCharacter : PlaceholderCharacterModel
{
    public override string PlaceholderID => "silent";
    public override Color NameColor => new("8fbc8f");
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;
    public override int StartingGold => 99;

    // ModelDb 返回规范定义；游戏在创建玩家套牌时生成各自的可变实例。
    // 两张气占据尚未实现的两张特殊初始牌的位置，不是最终平衡方案。
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<HeavyStrike>(), ModelDb.Card<HeavyStrike>(),
        ModelDb.Card<HeavyStrike>(), ModelDb.Card<HeavyStrike>(), ModelDb.Card<HeavyStrike>(),
        ModelDb.Card<HeavyDefend>(), ModelDb.Card<HeavyDefend>(),
        ModelDb.Card<HeavyDefend>(), ModelDb.Card<HeavyDefend>(), ModelDb.Card<HeavyDefend>(),
        ModelDb.Card<Qi>(), ModelDb.Card<Qi>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<RingOfTheSnake>()];
    public override CardPoolModel CardPool => ModelDb.CardPool<HeavySilentCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<HeavySilentRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<HeavySilentPotionPool>();
}
