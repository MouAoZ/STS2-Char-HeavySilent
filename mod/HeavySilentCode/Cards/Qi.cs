using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;

namespace HeavySilent.Cards;

/// <summary>HS-TOKEN-001：0 费状态牌，抽 1（2）张牌，消耗。</summary>
[CustomID("HEAVYSILENT_QI")]
public sealed class Qi() : HeavySilentCard(0, CardType.Status, CardRarity.Status, TargetType.None)
{
    // 原版升级预览会创建尚未分配 Owner 的可变牌。
    protected override bool UsesRecruit => !IsCanonical && Owner?.Creature.HasPower<ShiftingMeridiansPower>() == true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override void AddExtraArgsToDescription(LocString description) => description.Add("IsRecruit", UsesRecruit);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 默认普通抽牌允许洗牌；易脉在实际打出时改为征召。
        if (Owner.Creature.HasPower<ShiftingMeridiansPower>())
            await RecruitCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        else
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        // 消耗由关键词驱动的原版出牌流程执行；不要在此重复调用 Exhaust。
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
}
