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

/// <summary>HS-SKL-017：拾遗。</summary>
[CustomID("HEAVYSILENT_RECLAIM")]
public sealed class Reclaim() : HeavySilentCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool UsesMill => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MillVar(3)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        MillResult result = await MillCmd.Execute(choiceContext, this, DynamicVars["Mill"].IntValue);
        if (!result.Success) return;
        foreach (CardModel card in result.Cards)
        {
            if (card.Pile?.Type == PileType.Discard) await CardPileCmd.Add(card, PileType.Hand);
        }
    }
    protected override void OnUpgrade() => DynamicVars["Mill"].UpgradeValueBy(1);
}
