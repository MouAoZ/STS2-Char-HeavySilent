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

/// <summary>HS-SKL-027：回环。</summary>
[CustomID("HEAVYSILENT_RECURRENCE")]
public sealed class Recurrence() : HeavySilentCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override bool UsesRecruit => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => await RecruitCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature) || Pile?.Type != PileType.Discard) return;
        await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Top);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}
