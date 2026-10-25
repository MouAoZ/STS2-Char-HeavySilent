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

/// <summary>HS-SKL-018：燃息。</summary>
[CustomID("HEAVYSILENT_BURN_BREATH")]
public sealed class BurnBreath() : HeavySilentCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip, HoverTipFactory.FromCard<Qi>(), HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel[] selected = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, PileType.Hand.GetPile(Owner).Cards.Count),
            c => c is Qi, this)).ToArray();
        int energy = selected.Sum(c => c.DynamicVars.Cards.IntValue);
        foreach (CardModel qi in selected) await CardCmd.Exhaust(choiceContext, qi);
        await PlayerCmd.GainEnergy(energy, Owner);
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
