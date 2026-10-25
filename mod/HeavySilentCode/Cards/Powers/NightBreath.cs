using BaseLib.Utils.Attributes;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Cards.Powers;

/// <summary>HS-POW-005：夜息。</summary>
[CustomID("HEAVYSILENT_NIGHT_BREATH")]
public sealed class NightBreath() : HeavySilentCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<NightBreathPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<NightBreathPower>(DynamicVars["NightBreathPower"].IntValue));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<NightBreathPower>(choiceContext, Owner.Creature, DynamicVars["NightBreathPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() { EnergyCost.UpgradeBy(-1); }
}
