using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using HeavySilent.Cards;
using HeavySilent.Relics;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HeavySilent.Character;

/// <summary>独立角色，使用正式初始套牌；角色表现暂借用静默。</summary>
[CustomID("HEAVYSILENT_CHARACTER")]
public sealed class HeavySilentCharacter : PlaceholderCharacterModel
{
    public override string PlaceholderID => "silent";
    public override Color NameColor => new("8fbc8f");
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 70;
    public override int StartingGold => 99;

    // ModelDb 返回规范定义；游戏在创建玩家套牌时生成各自的可变实例。
    // 正式初始牌保持独立模型，先古替换通过 BaseLib 映射。
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<HeavyStrike>(), ModelDb.Card<HeavyStrike>(),
        ModelDb.Card<HeavyStrike>(), ModelDb.Card<HeavyStrike>(), ModelDb.Card<HeavyStrike>(),
        ModelDb.Card<HeavyDefend>(), ModelDb.Card<HeavyDefend>(),
        ModelDb.Card<HeavyDefend>(), ModelDb.Card<HeavyDefend>(), ModelDb.Card<HeavyDefend>(),
        ModelDb.Card<GatherBreath>(), ModelDb.Card<ObliqueStrike>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<HeavySilent.Relics.Whetstone>()];
    public override CardPoolModel CardPool => ModelDb.CardPool<HeavySilentCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<HeavySilentRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<HeavySilentPotionPool>();

    // 原版只收集卡池的大能量图标；正文中的能量图标也要随角色整局保留。
    // 交给原版资源生命周期管理，避免每次切换房间后再次按需加载。
    protected override IEnumerable<string> ExtraAssetPaths =>
        base.ExtraAssetPaths.Append(ModelDb.CardPool<HeavySilentCardPool>().TextEnergyIconPath);
}
