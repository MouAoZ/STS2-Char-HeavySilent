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

/// <summary>HS-SKL-003：藏息。</summary>
[CustomID("HEAVYSILENT_HIDDEN_BREATH")]
public sealed class HiddenBreath() : HeavySilentCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override bool UsesMill => true;
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MillVar(4), new BlockVar(8, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromCard<Qi>(IsUpgraded));
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (!(await MillCmd.Execute(choiceContext, this, DynamicVars["Mill"].IntValue)).Success) return;
        CardModel qi = CombatState!.CreateCard<Qi>(Owner);
        if (IsUpgraded) CardCmd.Upgrade(qi);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(qi, PileType.Draw, Owner, CardPilePosition.Random));
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(1);
}
