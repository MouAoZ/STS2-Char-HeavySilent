using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Cards.Skills;

/// <summary>HS-SKL-019：暗涌。</summary>
[CustomID("HEAVYSILENT_UNDERCURRENT")]
public sealed class Undercurrent() : HeavySilentCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IMilledCard
{
    protected override bool UsesMill => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(EnergyHoverTip);
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    public async Task OnMilled(PlayerChoiceContext choiceContext, CardModel source)
    {
        if (Pile?.Type == PileType.Discard) await CardCmd.AutoPlay(choiceContext, this, null);
    }
    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1);
}
