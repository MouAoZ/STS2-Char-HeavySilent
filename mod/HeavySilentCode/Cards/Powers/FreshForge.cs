using BaseLib.Utils.Attributes;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Cards.Powers;

/// <summary>HS-POW-013：淬新。</summary>
[CustomID("HEAVYSILENT_FRESH_FORGE")]
public sealed class FreshForge() : HeavySilentCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<FreshForgePower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<FreshForgePower>(DynamicVars["FreshForgePower"].IntValue));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FreshForgePower>(choiceContext, Owner.Creature, DynamicVars["FreshForgePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { AddKeyword(CardKeyword.Innate); }
}
