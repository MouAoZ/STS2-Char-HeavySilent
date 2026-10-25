using BaseLib.Utils.Attributes;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Cards.Powers;

/// <summary>HS-POW-003：阴阳。</summary>
[CustomID("HEAVYSILENT_YIN_AND_YANG")]
public sealed class YinAndYang() : HeavySilentCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<YinAndYangPower>(3)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<YinAndYangPower>(DynamicVars["YinAndYangPower"].IntValue));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<YinAndYangPower>(choiceContext, Owner.Creature, DynamicVars["YinAndYangPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { DynamicVars["YinAndYangPower"].UpgradeValueBy(1); }
}
