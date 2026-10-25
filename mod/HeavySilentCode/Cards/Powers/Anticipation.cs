using BaseLib.Utils.Attributes;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Cards.Powers;

/// <summary>HS-POW-011：先觉。</summary>
[CustomID("HEAVYSILENT_ANTICIPATION")]
public sealed class Anticipation() : HeavySilentCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<AnticipationPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<AnticipationPower>(DynamicVars["AnticipationPower"].IntValue));
    protected override bool UsesMill => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AnticipationPower>(choiceContext, Owner.Creature, DynamicVars["AnticipationPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { AddKeyword(CardKeyword.Retain); }
}
