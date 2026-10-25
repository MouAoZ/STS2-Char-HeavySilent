using HarmonyLib;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace HeavySilent.Patches;

/// <summary>
/// v0.111.0：CardReward 不派发所选 CardModel 的回调，也不把候选加入 Hook 监听集合。
/// 仅包裹原版 OnSelect/单张入 Deck 调用；保留原版选择 ID、网络同步、UI 和获得 Hook。
/// </summary>
public static class RewardPatches
{
    [HarmonyPatch(typeof(CardReward), nameof(CardReward.Populate))]
    private static class Appearing
    {
        private static void Postfix(CardReward __instance) => RewardContent.OnAppearing(__instance);
    }

    [HarmonyPatch(typeof(CardReward), "OnSelect")]
    private static class Selecting
    {
        private static void Prefix(CardReward __instance, out RewardContent.SelectionScope __state)
            => __state = RewardContent.Begin(__instance);

        private static void Postfix(ref Task<bool> __result, RewardContent.SelectionScope __state)
        {
            RewardContent.RestoreCaller(__state);
            __result = RewardContent.Complete(__result, __state);
        }

        private static Exception? Finalizer(Exception? __exception, RewardContent.SelectionScope? __state)
        {
            if (__state != null) RewardContent.RestoreCaller(__state);
            return __exception;
        }
    }

    [HarmonyPatch(typeof(CardPileCmd), nameof(CardPileCmd.Add),
        [typeof(CardModel), typeof(PileType), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)])]
    private static class SelectedCardAdded
    {
        private static void Prefix(CardModel card, PileType newPileType, out RewardContent.SelectionTicket? __state)
            => __state = RewardContent.Capture(card, newPileType);

        private static void Postfix(ref Task<CardPileAddResult> __result, RewardContent.SelectionTicket? __state)
        {
            if (__state != null) __result = RewardContent.AfterSelectedAdd(__result, __state);
        }

        private static Exception? Finalizer(Exception? __exception, RewardContent.SelectionTicket? __state)
        {
            if (__exception != null) __state?.Release();
            return __exception;
        }
    }
}
