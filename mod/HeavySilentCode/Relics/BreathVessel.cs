using BaseLib.Utils.Attributes;
using HeavySilent.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HeavySilent.Relics;

/// <summary>HS-RELIC-003：息壶，在初始抽牌前生成气+。</summary>
[CustomID("HEAVYSILENT_BREATH_VESSEL")]
public sealed class BreathVessel : HeavySilentRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Qi>()];

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CombatState is not { } combat) return;
        Flash();
        for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            var qi = combat.CreateCard<Qi>(Owner);
            CardCmd.Upgrade(qi);
            await CardPileCmd.AddGeneratedCardToCombat(qi, PileType.Draw, Owner, CardPilePosition.Random);
        }
    }
}
