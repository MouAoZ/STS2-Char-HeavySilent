using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Mechanics;

public sealed record MillResult(bool Success, int Amount, int OrdinalThisTurn, IReadOnlyList<CardModel> Cards);

/// <summary>仅本模组的磨顶事件；其他生命周期继续使用原版 Hook。</summary>
public interface IMillListener
{
    Task BeforeMill(PlayerChoiceContext choiceContext, CardModel source, int amount) => Task.CompletedTask;
    Task AfterMill(PlayerChoiceContext choiceContext, CardModel source, MillResult result) => Task.CompletedTask;
}

public interface IMillModifier
{
    int ModifyMill(CardModel source, int amount);
}

/// <summary>牌实际被磨入弃牌堆后立即等待该触发，允许其嵌套抽牌和自动出牌。</summary>
public interface IMilledCard
{
    Task OnMilled(PlayerChoiceContext choiceContext, CardModel source);
}
