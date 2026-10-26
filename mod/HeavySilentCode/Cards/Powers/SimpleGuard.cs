using BaseLib.Utils.Attributes;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Cards.Powers;

/// <summary>HS-POW-006：守拙。</summary>
[CustomID("HEAVYSILENT_SIMPLE_GUARD")]
public sealed class SimpleGuard() : HeavySilentCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SimpleGuardPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<SimpleGuardPower>(DynamicVars["SimpleGuardPower"].IntValue));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SimpleGuardPower>(choiceContext, Owner.Creature, DynamicVars["SimpleGuardPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { AddKeyword(CardKeyword.Innate); }
}
