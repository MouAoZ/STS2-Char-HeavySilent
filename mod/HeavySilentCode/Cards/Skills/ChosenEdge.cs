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

/// <summary>HS-SKL-015：拣锋。</summary>
[CustomID("HEAVYSILENT_CHOSEN_EDGE")]
public sealed class ChosenEdge() : HeavySilentCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool UsesMill => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MillVar(3)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        MillResult result = await MillCmd.Execute(choiceContext, this, DynamicVars["Mill"].IntValue);
        if (!result.Success) return;
        // 磨出牌的即时效果可能抽牌、洗牌；候选仍是本次磨到且尚在战斗中的原实例。
        CardModel[] candidates = result.Cards
            .Where(c => !c.HasBeenRemovedFromState && c.Pile is { IsCombatPile: true }).ToArray();
        CardModel? selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, candidates, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
        if (selected is { HasBeenRemovedFromState: false, Pile.IsCombatPile: true })
            await CardCmd.AutoPlay(choiceContext, selected, null);
    }
    protected override void OnUpgrade() => DynamicVars["Mill"].UpgradeValueBy(1);
}
