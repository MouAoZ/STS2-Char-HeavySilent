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

/// <summary>HS-SKL-012：砺甲。</summary>
[CustomID("HEAVYSILENT_HONED_ARMOR")]
public sealed class HonedArmor() : HeavySilentCard(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool UsesMill => true;
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this) return false;
        modifiedCost -= MillTurnState.For(Owner).MillCount;
        return modifiedCost != originalCost;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}
