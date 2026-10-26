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

/// <summary>HS-SKL-020：伏笔。</summary>
[CustomID("HEAVYSILENT_FORESIGHT")]
public sealed class Foresight() : HeavySilentCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool HasEnergyCostX => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("ExtraCards", 0)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip, HoverTipFactory.FromKeyword(CardKeyword.Retain)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count = ResolveEnergyXValue() + DynamicVars["ExtraCards"].IntValue;
        if (count <= 0) return;
        CardModel[] selected = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, count), null, this)).ToArray();
        // 选牌完成后一次采样；此后减费或改牌不能改变返还金额。
        int energy = selected.Sum(c => c.EnergyCost.GetAmountToSpend());
        foreach (CardModel card in selected) CardCmd.ApplySingleTurnRetain(card);
        await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, energy, Owner.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars["ExtraCards"].UpgradeValueBy(1);
}
