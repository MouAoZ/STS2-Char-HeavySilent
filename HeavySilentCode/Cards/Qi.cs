using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HeavySilent.Cards;

/// <summary>HS-TOKEN-001：0 费状态牌，抽 1（2）张牌，消耗。</summary>
[CustomID("HEAVYSILENT_QI")]
public sealed class Qi() : HeavySilentCard(0, CardType.Status, CardRarity.Status, TargetType.None)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 气是普通抽牌：牌堆耗尽时允许游戏洗牌。它不是“征召”。
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        // 消耗由关键词驱动的原版出牌流程执行；不要在此重复调用 Exhaust。
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}
