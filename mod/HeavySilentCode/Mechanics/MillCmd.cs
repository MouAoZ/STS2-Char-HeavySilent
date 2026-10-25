using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Mechanics;

public static class MillCmd
{
    public static int ModifiedAmount(CardModel source, int baseAmount)
    {
        int amount = baseAmount;
        if (source.CombatState is { } combat)
            foreach (IMillModifier modifier in combat.IterateHookListeners().OfType<IMillModifier>())
                amount = modifier.ModifyMill(source, amount);
        return Math.Max(0, amount);
    }

    public static async Task<MillResult> Execute(PlayerChoiceContext choiceContext, CardModel source, int baseAmount)
    {
        int amount = ModifiedAmount(source, baseAmount);
        if (source.CombatState is not { } combat || CombatManager.Instance.IsOverOrEnding)
            return new(false, amount, 0, []);

        // local #57：先觉在数量检查之前抽牌，即使初始不足；修正量在本次开始锁定。
        foreach (IMillListener listener in combat.IterateHookListeners().OfType<IMillListener>().ToArray())
            await listener.BeforeMill(choiceContext, source, amount);

        CardPile draw = PileType.Draw.GetPile(source.Owner);
        if (CombatManager.Instance.IsOverOrEnding || draw.Cards.Count < amount)
            return new(false, amount, 0, []);

        MillTurnState state = MillTurnState.For(source.Owner);
        int ordinal = ++state.MillCount;
        List<CardModel> moved = [];
        MillTurnState.NotifyCosts(source.Owner);
        for (int i = 0; i < amount && !CombatManager.Instance.IsOverOrEnding; i++)
        {
            // 插入触发可能改堆甚至洗牌；继续处理实时顶牌，空堆结束仍是成功的一次磨顶。
            CardModel? card = draw.Cards.FirstOrDefault();
            if (card is null) break;
            await CardPileCmd.Add(card, PileType.Discard);
            moved.Add(card);
            state.MilledCards++;
            if (card is IMilledCard trigger)
                await trigger.OnMilled(choiceContext, source);
        }

        MillResult result = new(true, amount, ordinal, moved.AsReadOnly());
        foreach (IMillListener listener in combat.IterateHookListeners().OfType<IMillListener>().ToArray())
            await listener.AfterMill(choiceContext, source, result);
        return result;
    }
}
