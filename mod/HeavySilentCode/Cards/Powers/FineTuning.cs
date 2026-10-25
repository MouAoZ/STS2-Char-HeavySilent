using BaseLib.Utils.Attributes;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Cards.Powers;

/// <summary>HS-POW-009：通过原版选择界面决定磨顶修正方向。</summary>
[CustomID("HEAVYSILENT_FINE_TUNING")]
public sealed class FineTuning() : HeavySilentCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override bool UsesMill => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Concat([HoverTipFactory.FromPower<FineTuningPower>(1), HoverTipFactory.FromPower<FineTuningPower>(-1)]);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 选项只用于选择界面，不进入战斗牌堆或触发生成事件。
        CardModel[] choices =
        [
            ModelDb.Card<FineTuningIncrease>().ToMutable(),
            ModelDb.Card<FineTuningDecrease>().ToMutable()
        ];
        var selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, Owner);
        if (selected == null) return;
        await PowerCmd.Apply<FineTuningPower>(choiceContext, Owner.Creature,
            selected is FineTuningIncrease ? 1 : -1, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
