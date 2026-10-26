using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HeavySilent.Cards.Powers;

/// <summary>微调的界面选项，不是可获取的设计卡。</summary>
[CustomID("HEAVYSILENT_FINE_TUNING_INCREASE")]
public sealed class FineTuningIncrease() : HeavySilentCard(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
{
    protected override bool UsesMill => true;
    public override bool CanBeGeneratedInCombat => false;
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
}
