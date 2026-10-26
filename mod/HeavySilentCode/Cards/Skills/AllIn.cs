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

/// <summary>HS-SKL-025：孤注。</summary>
[CustomID("HEAVYSILENT_ALL_IN")]
public sealed class AllIn() : HeavySilentCard(8, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override bool UsesMill => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(EnergyHoverTip);
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards)
        {
            if (!card.EnergyCost.CostsX) card.SetToFreeThisTurn();
        }
        return Task.CompletedTask;
    }
    // 支付与不足能量时的可打出性由共用替代支付入口处理。
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-2);
}
