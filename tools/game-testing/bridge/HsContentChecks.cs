using System.Runtime.CompilerServices;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;

namespace STS2_MCP;

public static partial class McpMod
{
    private static Task<object>? _hsContentTask;
    private static string? _hsContentOperationId;
    private static string? _hsContentOperation;
    private static DateTimeOffset _hsContentStartedAt;

    // HsControl.HsBusy uses this to prevent scenario setup or save/quit while a
    // native reward/selection command is still waiting. Ordinary UI actions stay available.
    private static bool HsContentBusy => _hsContentTask is { IsCompleted: false };

    /// <summary>
    /// POST /hs/content. Called by the same main-thread dispatcher and managed-session
    /// authorization as the existing /hs routes. No reference to HeavySilent.dll.
    /// </summary>
    private static Task<object> HsContentDispatch(JsonElement args)
        => Task.FromResult(HsContentCommand(args));

    private static object HsContentCommand(JsonElement args)
    {
        string op = args.GetProperty("op").GetString() ?? throw new ArgumentException("Missing content operation.");
        // Polling must work while the native task is waiting on a player choice.
        if (op == "status")
        {
            HsContentFields(args, "op", "operation_id");
            if (args.TryGetProperty("operation_id", out var expected) && expected.GetString() != _hsContentOperationId)
                throw new ArgumentException("Unknown content operation ID; do not start another operation to poll this one.");
            return HsContentOperationState();
        }

        Player player = HsContentPlayer();
        switch (op)
        {
            case "snapshot":
                HsContentFields(args, "op");
                return HsContentSnapshot(player);
            case "models":
                HsContentFields(args, "op", "query");
                string query = args.TryGetProperty("query", out var q) ? q.GetString() ?? "" : "HEAVYSILENT_";
                if (query.Length > 128) throw new ArgumentException("Model query is too long.");
                return HsContentModels(player, query);
            case "reward":
            {
                HsContentFields(args, "op", "cards", "no_hook_upgrades");
                HsContentCanMutate();
                bool noHookUpgrades = args.TryGetProperty("no_hook_upgrades", out var noUpgrade) && noUpgrade.GetBoolean();
                JsonElement cards = args.GetProperty("cards");
                if (cards.GetArrayLength() is < 1 or > 6) throw new ArgumentException("Offer 1 to 6 reward cards.");
                var specs = new List<(CardModel Canonical, bool Upgraded)>();
                foreach (JsonElement card in cards.EnumerateArray())
                {
                    HsContentFields(card, "id", "upgraded");
                    string id = card.GetProperty("id").GetString() ?? throw new ArgumentException("Card ID is required.");
                    CardModel model = ModelDb.AllCards.Single(c => c.Id.Entry == id);
                    bool upgraded = card.TryGetProperty("upgraded", out var upgrade) && upgrade.GetBoolean();
                    if (upgraded && model.MaxUpgradeLevel == 0) throw new ArgumentException($"{id} cannot be upgraded.");
                    specs.Add((model, upgraded));
                }
                CardCreationOptions? generatedOptions = null;
                if (noHookUpgrades)
                {
                    // The manual-candidates constructor has its own fixed Options. Use
                    // the native factory constructor so NoHookUpgrades reaches reward hooks.
                    // Asking for the whole filtered pool yields these distinct candidates
                    // in native RNG order; upgrades happen only when cards enter Deck.
                    var ids = specs.Select(spec => spec.Canonical.Id).ToHashSet();
                    if (specs.Any(spec => spec.Upgraded) || ids.Count != specs.Count)
                        throw new ArgumentException("no_hook_upgrades requires distinct, unupgraded candidates.");
                    generatedOptions = CardCreationOptions.ForNonCombatWithUniformOdds(
                        [player.Character.CardPool], card => ids.Contains(card.Id))
                        .WithFlags(CardCreationFlags.NoHookUpgrades);
                    int eligible = generatedOptions.GetPossibleCards(player)
                        .Where(card => card.Rarity is not CardRarity.Basic and not CardRarity.Ancient)
                        .Select(card => card.Id).Distinct().Count();
                    if (eligible != specs.Count)
                        throw new ArgumentException("no_hook_upgrades candidates must be available in the player's native reward pool.");
                }
                // Validate the complete request before creating even the first run card.
                return HsStartContentOperation(op, async () =>
                {
                    CardReward reward;
                    if (generatedOptions != null)
                    {
                        reward = new CardReward(generatedOptions, specs.Count, player);
                    }
                    else
                    {
                        var offered = new List<CardModel>();
                        foreach (var spec in specs)
                        {
                            CardModel card = player.RunState.CreateCard(spec.Canonical, player);
                            if (spec.Upgraded) CardCmd.Upgrade(card);
                            offered.Add(card);
                        }
                        var options = new CardCreationOptions([player.Character.CardPool], CardCreationSource.Other, CardRarityOddsType.Uniform);
                        reward = new CardReward(offered, CardCreationSource.Other, player, options);
                    }
                    // This shows the actual rewards screen. Its normal selection routes
                    // through RewardsSetSynchronizer -> Reward.SelectUnsynchronized ->
                    // CardReward.OnSelect; the bridge never chooses or grants candidates.
                    await RewardsCmd.OfferCustom(player, [reward]);
                    return new { selected = reward.SuccessfullySelected, snapshot = HsContentSnapshot(player) };
                });
            }
            case "relic":
            {
                HsContentFields(args, "op", "id");
                HsContentCanMutate();
                string id = args.GetProperty("id").GetString() ?? throw new ArgumentException("Relic ID is required.");
                RelicModel canonical = ModelDb.AllRelics.Single(r => r.Id.Entry == id);
                if (!canonical.IsStackable && player.Relics.Any(r => r.Id == canonical.Id))
                    throw new ArgumentException($"Player already owns non-stackable relic {id}.");
                return HsStartContentOperation(op, async () =>
                {
                    RelicModel relic = canonical.ToMutable();
                    switch (relic)
                    {
                        case TouchOfOrobas orobas when !orobas.SetupForPlayer(player):
                            throw new InvalidOperationException("Touch of Orobas found no starter relic to upgrade.");
                        case ArchaicTooth tooth when !tooth.SetupForPlayer(player):
                            throw new InvalidOperationException("Archaic Tooth found no mapped starter card.");
                        case DustyTome tome:
                            tome.SetupForPlayer(player);
                            break;
                    }
                    // Native AfterObtained performs the mapped replacement or creation.
                    // Do not assign an expected card/relic ID or invoke mod internals here.
                    RelicModel obtained = await RelicCmd.Obtain(relic, player);
                    return new { obtained = obtained.Id.Entry, snapshot = HsContentSnapshot(player) };
                });
            }
            case "upgrade_deck":
            {
                HsContentFields(args, "op", "indices");
                HsContentCanMutate();
                int[] indices = args.GetProperty("indices").EnumerateArray().Select(i => i.GetInt32()).ToArray();
                if (indices.Length is < 1 or > 20 || indices.Distinct().Count() != indices.Length)
                    throw new ArgumentException("Supply 1 to 20 distinct deck indices.");
                if (indices.Any(i => i < 0 || i >= player.Deck.Cards.Count)) throw new ArgumentOutOfRangeException("indices");
                CardModel[] selected = indices.Select(i => player.Deck.Cards[i]).ToArray();
                if (selected.Any(c => !c.IsUpgradable)) throw new ArgumentException("Every selected deck card must be upgradable.");
                return HsStartContentOperation(op, () =>
                {
                    foreach (CardModel card in selected) CardCmd.Upgrade(card);
                    return Task.FromResult(HsContentSnapshot(player));
                });
            }
            default:
                throw new ArgumentException($"Unknown content operation: {op}");
        }
    }

    private static void HsContentFields(JsonElement args, params string[] allowed)
    {
        foreach (JsonProperty field in args.EnumerateObject())
            if (!allowed.Contains(field.Name)) throw new ArgumentException($"Unknown content field: {field.Name}");
    }

    private static Player HsContentPlayer()
    {
        if (!HsEnabled || HsProfile != 3) throw new InvalidOperationException("Content checks require managed test profile 3.");
        var run = RunManager.Instance.IsInProgress ? RunManager.Instance.DebugOnlyGetState() : null;
        if (run == null || run.Players.Count != 1) throw new InvalidOperationException("Start a singleplayer test run first.");
        Player player = run.Players.Single();
        if (player.Character.Id.Entry != "HEAVYSILENT_CHARACTER") throw new InvalidOperationException("Expected HeavySilent.");
        return player;
    }

    private static void HsContentCanMutate()
    {
        if (HsBusy() || HsContentBusy) throw new InvalidOperationException("Wait for the current action or content operation to finish.");
        if (CombatManager.Instance.IsInProgress) throw new InvalidOperationException("Run reward/deck/relic checks outside combat.");
        if (NOverlayStack.Instance?.Peek() != null) throw new InvalidOperationException("Close the current overlay before preparing content.");
    }

    private static object HsStartContentOperation(string op, Func<Task<object>> action)
    {
        _hsContentOperationId = Guid.NewGuid().ToString("N");
        _hsContentOperation = op;
        _hsContentStartedAt = DateTimeOffset.UtcNow;
        // Invoke on the game thread, without Task.Run. Awaited engine commands retain
        // their synchronization context and may yield to real selection screens.
        _hsContentTask = HsRunContentOperation(action);
        return HsContentOperationState();
    }

    private static async Task<object> HsRunContentOperation(Func<Task<object>> action)
    {
        try
        {
            object result = await action();
            ++_hsRevision;
            return result;
        }
        catch (Exception error)
        {
            GD.PrintErr($"[HeavySilentTestBridge] Content operation {_hsContentOperationId}: {error}");
            throw;
        }
    }

    private static object HsContentOperationState()
    {
        Task<object>? task = _hsContentTask;
        return new
        {
            operation_id = _hsContentOperationId,
            operation = _hsContentOperation,
            started_at = task == null ? (DateTimeOffset?)null : _hsContentStartedAt,
            status = task == null ? "none" : !task.IsCompleted ? "pending" : task.IsCanceled ? "cancelled" : task.IsFaulted ? "failed" : "completed",
            result = task?.IsCompletedSuccessfully == true ? task.Result : null,
            error = task?.Exception?.GetBaseException().ToString(),
            overlay = NOverlayStack.Instance?.Peek()?.GetType().Name
        };
    }

    private static object HsContentCard(CardModel card, int? index = null) => new
    {
        index, id = card.Id.Entry, instance_id = RuntimeHelpers.GetHashCode(card),
        type = card.Type.ToString(), rarity = card.Rarity.ToString(), target = card.TargetType.ToString(),
        title = card.Title, description = SafeGetCardDescription(card),
        upgraded = card.IsUpgraded, upgrade_level = card.CurrentUpgradeLevel, max_upgrades = card.MaxUpgradeLevel,
        cost = card.EnergyCost.GetWithModifiers(CostModifiers.Local), cost_x = card.EnergyCost.CostsX,
        keywords = card.Keywords.Select(k => k.ToString()).ToArray(),
        variables = card.DynamicVars.ToDictionary(kv => kv.Key, kv => kv.Value.BaseValue),
        enchantment = card.Enchantment?.Id.Entry,
        serialized = card.ToSerializable()
    };

    private static object HsContentSnapshot(Player player)
    {
        CardModel[] unlocked = player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).ToArray();
        return new
        {
            character_id = player.Character.Id.Entry,
            deck = player.Deck.Cards.Select((c, i) => HsContentCard(c, i)).ToArray(),
            relics = player.Relics.Select(r => new
            {
                id = r.Id.Entry, title = r.Title.GetFormattedText(), rarity = r.Rarity.ToString(),
                description = r.DynamicDescription.GetFormattedText(), serialized = r.ToSerializable()
            }).ToArray(),
            card_pool = player.Character.CardPool.Id.Entry,
            all_pool_cards = player.Character.CardPool.AllCards.Select(c => new { id = c.Id.Entry, rarity = c.Rarity.ToString(), type = c.Type.ToString() }).ToArray(),
            unlocked_pool_cards = unlocked.Select(c => c.Id.Entry).ToArray(),
            regular_rarity_candidates = unlocked.Where(c => c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare).Select(c => c.Id.Entry).ToArray(),
            relic_pool = player.Character.RelicPool.AllRelics.Select(r => new { id = r.Id.Entry, rarity = r.Rarity.ToString() }).ToArray(),
            // wasPicked is a field; explicitly project it because the HTTP serializer
            // serializes properties only (unlike the game's save serializer).
            card_choice_history = player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId).CardChoices
                .Select(entry => new { card = entry.Card, was_picked = entry.wasPicked }).ToArray()
        };
    }

    private static object HsContentModels(Player player, string query)
    {
        var models = new List<object>();
        foreach (CardModel canonical in ModelDb.AllCards.Where(c => c.Id.Entry.Contains(query, StringComparison.OrdinalIgnoreCase)).OrderBy(c => c.Id.Entry))
        {
            // Detached mutable previews: no addition to RunState, Deck or combat piles.
            CardModel normal = canonical.ToMutable();
            normal.Owner = player;
            CardModel upgraded = canonical.ToMutable();
            upgraded.Owner = player;
            if (upgraded.IsUpgradable) CardCmd.Upgrade(upgraded);
            models.Add(new
            {
                id = canonical.Id.Entry, model_type = canonical.GetType().FullName,
                in_card_library = canonical.ShouldShowInCardLibrary,
                in_character_pool = player.Character.CardPool.AllCardIds.Contains(canonical.Id),
                normal = HsContentCard(normal), upgraded = HsContentCard(upgraded)
            });
        }
        return new { count = models.Count, models };
    }
}
