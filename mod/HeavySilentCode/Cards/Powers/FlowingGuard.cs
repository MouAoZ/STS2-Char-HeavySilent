using BaseLib.Utils.Attributes;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Cards.Powers;

/// <summary>HS-POW-010：流壁。</summary>
[CustomID("HEAVYSILENT_FLOWING_GUARD")]
public sealed class FlowingGuard() : HeavySilentCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<FlowingGuardPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<FlowingGuardPower>(DynamicVars["FlowingGuardPower"].IntValue));
    protected override bool UsesMill => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FlowingGuardPower>(choiceContext, Owner.Creature, DynamicVars["FlowingGuardPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { AddKeyword(CardKeyword.Innate); }
}
