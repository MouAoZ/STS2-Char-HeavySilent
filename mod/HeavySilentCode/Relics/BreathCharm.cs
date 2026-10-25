using BaseLib.Utils.Attributes;
using HeavySilent.Cards;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Relics;

/// <summary>HS-RELIC-004：护息符，读取本回合实际打出气计数。</summary>
[CustomID("HEAVYSILENT_BREATH_CHARM")]
public sealed class BreathCharm : HeavySilentRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4, ValueProp.Unpowered)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<Qi>(), HoverTipFactory.Static(StaticHoverTip.Block)];

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || cardPlay.Card is not Qi || MillTurnState.For(Owner).QiPlayed != 1) return;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
    }
}
