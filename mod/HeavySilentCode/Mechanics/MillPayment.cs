using System.Runtime.CompilerServices;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using HeavySilent.Cards.Skills;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HeavySilent.Mechanics;

/// <summary>
/// 原版只支持能量不足改付星星，没有异步磨顶支付 Hook。
/// 这里仅适配资源资格、实际能量支出与进入原版出牌包装前的磨顶，不改普通出牌流程。
/// </summary>
public static class MillPayment
{
    private sealed record Payment(int BaseMill, int? CapturedX);
    private static readonly ConditionalWeakTable<CardModel, Payment> Pending = new();
    private static readonly ConditionalWeakTable<CardEnergyCost, object> ClampNextSpend = new();
    private static readonly ConditionalWeakTable<CardModel, object> FailedPayments = new();

    private static int Ratio(CardModel card) => card.Owner.Creature.HasPower<ShiftingMeridiansPower>()
        ? 2 : card is AllIn ? 4 : 0;

    [HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.HasEnoughResourcesFor))]
    private static class Eligibility
    {
        private static void Postfix(PlayerCombatState __instance, CardModel card, ref UnplayableReason reason, ref bool __result)
        {
            int ratio = Ratio(card);
            if (ratio == 0) return;
            if (card.EnergyCost.CostsX)
            {
                int drawCount = PileType.Draw.GetPile(card.Owner).Cards.Count;
                int shortage = drawCount / 2;
                if (shortage > 0 && drawCount < MillCmd.ModifiedAmount(card, shortage * ratio))
                {
                    reason |= UnplayableReason.EnergyCostTooHigh;
                    __result = false;
                }
                return;
            }
            if (!reason.HasFlag(UnplayableReason.EnergyCostTooHigh)) return;
            int missing = Math.Max(0, card.EnergyCost.GetWithModifiers(CostModifiers.All) - __instance.Energy);
            if (PileType.Draw.GetPile(card.Owner).Cards.Count < MillCmd.ModifiedAmount(card, missing * ratio)) return;
            reason &= ~UnplayableReason.EnergyCostTooHigh;
            __result = reason == UnplayableReason.None;
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
    private static class CapturePayment
    {
        private static void Prefix(CardModel __instance)
        {
            int ratio = Ratio(__instance);
            if (ratio == 0) return;
            int energy = __instance.Owner.PlayerCombatState!.Energy;
            bool isX = __instance.EnergyCost.CostsX;
            int missing = isX
                ? PileType.Draw.GetPile(__instance.Owner).Cards.Count / 2
                : Math.Max(0, __instance.EnergyCost.GetAmountToSpend() - energy);
            if (missing == 0) return;
            Pending.Remove(__instance);
            Pending.Add(__instance, new(missing * ratio, isX ? energy + missing : null));
            ClampNextSpend.Remove(__instance.EnergyCost);
            ClampNextSpend.Add(__instance.EnergyCost, new object());
        }

        private static void Postfix(CardModel __instance, ref Task<(int, int)> __result)
        {
            if (Pending.TryGetValue(__instance, out Payment? payment))
                __result = FinishCapture(__instance, payment, __result);
        }

        private static async Task<(int, int)> FinishCapture(CardModel card, Payment payment, Task<(int, int)> original)
        {
            try
            {
                var spent = await original;
                if (payment.CapturedX is { } x) card.EnergyCost.CapturedXValue = x;
                return spent;
            }
            catch
            {
                Pending.Remove(card);
                throw;
            }
            finally { ClampNextSpend.Remove(card.EnergyCost); }
        }
    }

    [HarmonyPatch(typeof(CardEnergyCost), nameof(CardEnergyCost.GetAmountToSpend))]
    private static class ActualEnergy
    {
        private static void Postfix(CardEnergyCost __instance, CardModel ____card, ref int __result)
        {
            if (!ClampNextSpend.Remove(__instance)) return;
            __result = Math.Min(__result, ____card.Owner.PlayerCombatState!.Energy);
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    private static class PayBeforePlay
    {
        private static bool Prefix(CardModel __instance, PlayerChoiceContext choiceContext, Creature? target,
            bool isAutoPlay, ResourceInfo resources, bool skipCardPileVisuals, ref Task __result)
        {
            if (isAutoPlay || !Pending.TryGetValue(__instance, out Payment? payment)) return true;
            Pending.Remove(__instance);
            __result = PayAndPlay(__instance, payment, choiceContext, target, resources, skipCardPileVisuals);
            return false;
        }

        private static async Task PayAndPlay(CardModel card, Payment payment, PlayerChoiceContext context,
            Creature? target, ResourceInfo resources, bool skipVisuals)
        {
            MillResult result = await MillCmd.Execute(context, card, payment.BaseMill);
            if (!result.Success) FailedPayments.GetValue(card, _ => new object());
            try
            {
                // local #58：失败仍进入原版出牌、费用历史、打出 Hook 与归堆，只跳过源卡效果。
                // Pending 已移除，调用原版包装；插入触发不重算本次 X。
                if (payment.CapturedX is { } x) card.EnergyCost.CapturedXValue = x;
                await card.OnPlayWrapper(context, target, false, resources, skipVisuals);
            }
            finally { FailedPayments.Remove(card); }
        }
    }

    private static readonly Func<CardModel, PlayerChoiceContext, CardPlay, Task> OriginalEffects =
        AccessTools.MethodDelegate<Func<CardModel, PlayerChoiceContext, CardPlay, Task>>(
            AccessTools.Method(typeof(CardModel), "OnPlay"));

    private static Task PlayEffects(CardModel card, PlayerChoiceContext context, CardPlay play) =>
        FailedPayments.TryGetValue(card, out _) ? Task.CompletedTask : OriginalEffects(card, context, play);

    [HarmonyPatch]
    private static class FailedPaymentEffects
    {
        private static MethodBase TargetMethod() => AccessTools.Method(typeof(CardModel), nameof(CardModel.OnPlayWrapper))
            .GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic)!;

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo onPlay = AccessTools.Method(typeof(CardModel), "OnPlay");
            MethodInfo dispatch = AccessTools.Method(typeof(MillPayment), nameof(PlayEffects));
            int matches = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(onPlay))
                {
                    matches++;
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = dispatch;
                }
                yield return instruction;
            }
            if (matches != 1) throw new InvalidOperationException("CardModel.OnPlayWrapper changed: expected one OnPlay dispatch.");
        }
    }
}
