using BaseLib.Utils.Attributes;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Cards.Powers;

/// <summary>HS-POW-001：飞砂。</summary>
[CustomID("HEAVYSILENT_FLYING_SAND")]
public sealed class FlyingSand() : HeavySilentCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<FlyingSandPower>(4)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<FlyingSandPower>(DynamicVars["FlyingSandPower"].IntValue));
    protected override bool UsesMill => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FlyingSandPower>(choiceContext, Owner.Creature, DynamicVars["FlyingSandPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { DynamicVars["FlyingSandPower"].UpgradeValueBy(2); }
}
