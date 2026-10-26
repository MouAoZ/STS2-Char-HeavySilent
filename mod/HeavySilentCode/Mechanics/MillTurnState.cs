using System.Runtime.CompilerServices;
using HarmonyLib;
using HeavySilent.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Mechanics;

/// <summary>绑定本场战斗的玩家状态；不会跨战斗或不同玩家共享计数。</summary>
public sealed class MillTurnState
{
    private static readonly ConditionalWeakTable<PlayerCombatState, MillTurnState> States = new();
    public int MillCount { get; internal set; }
    public int MilledCards { get; internal set; }
    public int QiPlayed { get; private set; }
    public int CardsEnteredDiscard { get; private set; }
    public bool Shuffled { get; private set; }

    public static MillTurnState For(Player player) => player.PlayerCombatState is { } state
        ? States.GetOrCreateValue(state) : new MillTurnState();

    internal static void NotifyCosts(Player player)
    {
        if (player.PlayerCombatState is not { } state) return;
        foreach (CardModel card in state.AllPiles.SelectMany(p => p.Cards).ToArray())
            card.InvokeEnergyCostChanged();
    }

    // BeforeSideTurnStart precedes start-of-turn effects and the initial hand draw;
    // AfterSideTurnStart runs after both and would erase their counters.
    [HarmonyPatch(typeof(Hook), nameof(Hook.BeforeSideTurnStart))]
    private static class TurnStarted
    {
        private static void Prefix(IReadOnlyList<Creature> participants)
        {
            foreach (Creature creature in participants)
            {
                if (creature.Player?.PlayerCombatState is not { } combat) continue;
                States.Remove(combat);
                NotifyCosts(creature.Player);
            }
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardPlayed))]
    private static class CardPlayed
    {
        private static void Prefix(CardPlay cardPlay)
        {
            if (cardPlay.Card is not Qi) return;
            For(cardPlay.Card.Owner).QiPlayed++;
            NotifyCosts(cardPlay.Card.Owner);
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterCardChangedPiles))]
    private static class ChangedPiles
    {
        private static void Prefix(CardModel card, PileType oldPile)
        {
            if (card.Pile?.Type == PileType.Discard && oldPile != PileType.Discard)
                For(card.Owner).CardsEnteredDiscard++;
            NotifyCosts(card.Owner);
        }
    }

    [HarmonyPatch(typeof(Hook), nameof(Hook.AfterShuffle))]
    private static class ShuffledPile
    {
        private static void Prefix(Player shuffler) => For(shuffler).Shuffled = true;
    }
}
