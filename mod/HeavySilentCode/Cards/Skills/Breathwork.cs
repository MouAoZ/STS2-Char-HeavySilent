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

/// <summary>HS-SKL-009：吐纳。</summary>
[CustomID("HEAVYSILENT_BREATHWORK")]
public sealed class Breathwork() : HeavySilentCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override bool UsesMill => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MillVar(6)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromCard<Qi>());
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        MillResult result = await MillCmd.Execute(choiceContext, this, DynamicVars["Mill"].IntValue);
        if (!result.Success) return;
        foreach (CardModel card in result.Cards)
        {
            // 前一张气的普通抽牌可能洗牌；按本次磨到的实例继续打出，不要求仍在弃牌堆。
            if (card is Qi && !card.HasBeenRemovedFromState && card.Pile is { IsCombatPile: true })
                await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }
    protected override void OnUpgrade() => DynamicVars["Mill"].UpgradeValueBy(2);
}
