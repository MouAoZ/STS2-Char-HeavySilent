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

/// <summary>HS-SKL-011：留影。</summary>
[CustomID("HEAVYSILENT_IMPRINT")]
public sealed class Imprint() : HeavySilentCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? selected = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1), c => c.Type is CardType.Attack or CardType.Skill, this)).FirstOrDefault();
        if (selected == null) return;
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(selected.CreateClone(), PileType.Draw, Owner, CardPilePosition.Random));
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}
