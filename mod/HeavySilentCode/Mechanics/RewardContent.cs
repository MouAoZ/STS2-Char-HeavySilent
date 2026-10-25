using System.Runtime.CompilerServices;
using HeavySilent.Cards;
using HeavySilent.Cards.Skills;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs.History;

namespace HeavySilent.Mechanics;

/// <summary>
/// 奖励候选不是原版 Hook 监听者。本入口只扩展 CardReward 的显示与实际选择阶段，
/// 不把战斗生成、商店购买或兼收附赠当作玩家选择。
/// </summary>
public static class RewardContent
{
    private static readonly AsyncLocal<SelectionScope?> Current = new();
    private static readonly ConditionalWeakTable<CardModel, object> MarkedOptions = new();

    public sealed class SelectionScope(CardReward reward, SelectionScope? parent)
    {
        internal CardReward Reward { get; } = reward;
        internal SelectionScope? Parent { get; } = parent;
        internal bool HasSelectedWindfall { get; set; }
        internal bool GrantingExtras { get; set; }
        internal bool Closed { get; set; }
        internal int PendingAdds { get; set; }
        internal HashSet<CardModel> SelectedOptions { get; } = new(ReferenceEqualityComparer.Instance);
    }

    public sealed class SelectionTicket(SelectionScope scope, CardModel offered)
    {
        internal SelectionScope Scope { get; } = scope;
        internal CardModel Offered { get; } = offered;
        private bool _released;

        internal void Release()
        {
            if (_released) return;
            _released = true;
            Scope.PendingAdds--;
        }
    }

    /// <summary>生成/重掷以及显示前都可调用，永久关键词合并是幂等操作。</summary>
    public static void OnAppearing(CardReward reward)
    {
        CardModel[] cards = reward.Cards.ToArray();
        int marks = cards.Count(c => c is PartingMark);
        if (marks == 0) return;
        foreach (CardModel card in cards)
        {
            // 两张赠印互为“其他牌”；只有一张时，它不改写自己。
            if (marks == 1 && card is PartingMark) continue;
            PermanentKeywords.Apply(card, CardKeyword.Innate, CardKeyword.Retain);
            MarkedOptions.GetValue(card, _ => new object());
        }
    }

    public static SelectionScope Begin(CardReward reward)
    {
        OnAppearing(reward);
        SelectionScope scope = new(reward, Current.Value);
        Current.Value = scope;
        return scope;
    }

    /// <summary>
    /// 原版 async 方法已捕获作用域；同步调用者立即恢复原作用域，避免不同玩家的并行奖励串台。
    /// </summary>
    public static void RestoreCaller(SelectionScope scope)
    {
        if (ReferenceEquals(Current.Value, scope)) Current.Value = scope.Parent;
    }

    public static SelectionTicket? Capture(CardModel offered, PileType destination)
    {
        SelectionScope? scope = Current.Value;
        if (destination != PileType.Deck || scope == null || scope.Closed || scope.GrantingExtras ||
            scope.PendingAdds != 0 || offered.Owner != scope.Reward.Player ||
            !scope.Reward.Cards.Any(c => ReferenceEquals(c, offered))) return null;

        scope.PendingAdds++;
        return new SelectionTicket(scope, offered);
    }

    public static async Task<CardPileAddResult> AfterSelectedAdd(Task<CardPileAddResult> original, SelectionTicket ticket)
    {
        try
        {
            CardPileAddResult result = await original;
            if (!result.success) return result;
            ticket.Scope.SelectedOptions.Add(ticket.Offered);
            TransferMark(ticket.Offered, result.cardAdded);
            if (ticket.Offered is Windfall) ticket.Scope.HasSelectedWindfall = true;
            if (ticket.Offered is Severance severance) await severance.OnRewardSelected();
            return result;
        }
        finally
        {
            ticket.Release();
        }
    }

    public static async Task<bool> Complete(Task<bool> original, SelectionScope scope)
    {
        try
        {
            bool completed = await original;
            RemoveSelectedOriginalsFromHistory(scope, completed);
            if (scope.HasSelectedWindfall)
            {
                // 等原版多选、UI 清理与历史记录完成，再获得尚未选中的实例。
                // 原版允许继续选牌时已选过的牌已从 reward.Cards 移除，不会重复获得。
                SelectionScope? previous = Current.Value;
                Current.Value = scope;
                scope.GrantingExtras = true;
                try
                {
                    await GrantRemaining(scope, completed);
                }
                finally
                {
                    scope.GrantingExtras = false;
                    Current.Value = previous;
                }
            }
            return completed;
        }
        finally
        {
            scope.Closed = true;
        }
    }

    private static void RemoveSelectedOriginalsFromHistory(SelectionScope scope, bool completed)
    {
        CardReward reward = scope.Reward;
        CardModel[] remaining = reward.Cards.ToArray();
        var history = reward.Player.RunState.CurrentMapPointHistoryEntry?.GetEntry(reward.Player.NetId).CardChoices;
        int historyStart = FindUnpickedHistoryTail(history, remaining, completed);
        if (historyStart < 0 || history == null) return;

        // 原版按最终入组实例移除候选；蛋类遗物克隆升级时，已选原件会残留并被记作未选。
        // 原版已为最终实例记录了选中历史，仅删除这些原件的多余未选项。
        for (int i = remaining.Length - 1; i >= 0; i--)
            if (scope.SelectedOptions.Contains(remaining[i])) history.RemoveAt(historyStart + i);
    }

    private static int FindUnpickedHistoryTail(IReadOnlyList<CardChoiceHistoryEntry>? history,
        IReadOnlyList<CardModel> remaining, bool completed)
    {
        // CardReward.OnSelect 最后按当前候选顺序追加未选历史；按位置关联，不能仅按模型 ID 匹配重复牌。
        int historyStart = completed && history != null ? history.Count - remaining.Count : -1;
        return historyStart >= 0 && history != null &&
            remaining.Select((card, i) => !history[historyStart + i].wasPicked &&
                history[historyStart + i].Card.Id == card.Id).All(matches => matches) ? historyStart : -1;
    }

    private static async Task GrantRemaining(SelectionScope scope, bool completed)
    {
        CardReward reward = scope.Reward;
        // 原版入组 Hook 可以替换实例，不能仅凭旧候选当前是否在 Deck 判定它是否被选择过。
        CardModel[] remaining = reward.Cards.Where(card => !scope.SelectedOptions.Contains(card)).ToArray();
        var history = reward.Player.RunState.CurrentMapPointHistoryEntry?.GetEntry(reward.Player.NetId).CardChoices;
        int historyStart = FindUnpickedHistoryTail(history, remaining, completed);

        for (int i = 0; i < remaining.Length; i++)
        {
            CardModel offered = remaining[i];
            if (offered.Pile?.Type == PileType.Deck || offered.HasBeenRemovedFromState) continue;
            CardPileAddResult result = await CardPileCmd.Add(offered, PileType.Deck);
            if (!result.success) continue;
            TransferMark(offered, result.cardAdded);
            if (history == null) continue;
            CardChoiceHistoryEntry entry = new(result.cardAdded, wasPicked: true);
            if (historyStart >= 0) history[historyStart + i] = entry;
            else history.Add(entry);
        }
    }

    private static void TransferMark(CardModel offered, CardModel obtained)
    {
        // 原版入组 Hook 可以替换实例；改牌必须落在最终获得的实例上。
        if (!ReferenceEquals(offered, obtained) && MarkedOptions.TryGetValue(offered, out _))
            PermanentKeywords.Apply(obtained, CardKeyword.Innate, CardKeyword.Retain);
    }
}
