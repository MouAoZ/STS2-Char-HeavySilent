using System.Text.Json;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_MCP;

public static partial class McpMod
{
    private static readonly Dictionary<string, PileType> HsPiles = new()
    {
        ["hand"] = PileType.Hand, ["draw"] = PileType.Draw,
        ["discard"] = PileType.Discard, ["exhaust"] = PileType.Exhaust
    };

    private static void HsValidateCreature(JsonElement spec)
    {
        foreach (var field in spec.EnumerateObject())
            if (field.Name is not ("index" or "hp" or "max_hp" or "block" or "energy" or "powers"))
                throw new ArgumentException($"Unknown creature field: {field.Name}");
        foreach (string name in new[] { "hp", "max_hp", "block", "energy" })
            if (spec.TryGetProperty(name, out var n) && (n.GetInt32() < (name is "hp" or "max_hp" ? 1 : 0) || n.GetInt32() > 9999))
                throw new ArgumentException($"Invalid fixture {name}");
        if (spec.TryGetProperty("powers", out var powers))
            foreach (var power in powers.EnumerateArray())
            {
                _ = ModelDb.AllPowers.Single(p => p.Id.Entry == power.GetProperty("id").GetString());
                if (Math.Abs(power.GetProperty("amount").GetInt32()) > 9999) throw new ArgumentException("Power amount too large.");
            }
    }

    private static async Task HsSetCreature(Creature creature, JsonElement spec, ThrowingPlayerChoiceContext choice)
    {
        if (spec.TryGetProperty("max_hp", out var maxHp)) await CreatureCmd.SetMaxHp(creature, maxHp.GetInt32());
        if (spec.TryGetProperty("hp", out var hp)) await CreatureCmd.SetCurrentHp(creature, hp.GetInt32());
        if (spec.TryGetProperty("block", out var block))
        {
            if (creature.Block > 0) await CreatureCmd.LoseBlock(choice, creature, creature.Block, null);
            if (block.GetInt32() > 0) await CreatureCmd.GainBlock(creature, block.GetInt32(), ValueProp.Unpowered, null);
        }
        if (spec.TryGetProperty("powers", out var powers))
            foreach (var power in powers.EnumerateArray())
            {
                var model = ModelDb.AllPowers.Single(p => p.Id.Entry == power.GetProperty("id").GetString()).ToMutable();
                await PowerCmd.Apply(choice, model, creature, power.GetProperty("amount").GetInt32(), null, null);
            }
    }

    private static async Task HsPrepareScenario(JsonElement spec)
    {
        var run = RunManager.Instance.DebugOnlyGetState() ?? throw new InvalidOperationException("Start a test run first.");
        var player = run.Players.Single();
        if (player.Character.Id.Entry != "HEAVYSILENT_CHARACTER" || !CombatManager.Instance.IsInProgress || !IsPlayPhase(player))
            throw new InvalidOperationException("Arrange a scenario during a HeavySilent singleplayer combat turn.");
        var combat = CombatManager.Instance.DebugOnlyGetState()!;
        var state = player.PlayerCombatState!;
        // Validate all model IDs and input shapes before any mutation. Unknown fields
        // fail visibly so a misspelled setup cannot silently test the wrong conditions.
        foreach (var property in spec.EnumerateObject())
        {
            if (HsPiles.ContainsKey(property.Name))
            {
                int limit = property.Name == "hand" ? CardPile.MaxCardsInHand : 100;
                if (property.Value.GetArrayLength() > limit) throw new ArgumentException($"Too many cards in {property.Name}");
                foreach (var card in property.Value.EnumerateArray())
                {
                    _ = ModelDb.AllCards.Single(c => c.Id.Entry == card.GetProperty("id").GetString());
                    foreach (var field in card.EnumerateObject())
                        if (field.Name is not ("id" or "upgraded")) throw new ArgumentException($"Unknown card field: {field.Name}");
                    if (card.TryGetProperty("upgraded", out var upgraded)) _ = upgraded.GetBoolean();
                }
            }
            else if (property.Name == "player") HsValidateCreature(property.Value);
            else if (property.Name == "enemies")
                foreach (var enemy in property.Value.EnumerateArray())
                {
                    int index = enemy.GetProperty("index").GetInt32();
                    if (index < 0 || index >= combat.Enemies.Count) throw new ArgumentException("Enemy index out of range.");
                    HsValidateCreature(enemy);
                }
            else if (property.Name == "clear_powers") _ = property.Value.GetBoolean();
            else throw new ArgumentException($"Unknown scenario field: {property.Name}");
        }

        await CardPileCmd.RemoveFromCombat(state.AllCards.ToArray());
        var choice = new ThrowingPlayerChoiceContext();
        bool clear = !spec.TryGetProperty("clear_powers", out var flag) || flag.GetBoolean();
        if (clear)
            foreach (var creature in combat.Enemies.Append(player.Creature))
                foreach (var power in creature.Powers.ToArray()) await PowerCmd.Remove(power);
        state.ResetEnergy();
        foreach (var pile in HsPiles)
            if (spec.TryGetProperty(pile.Key, out var cards))
                foreach (var card in cards.EnumerateArray())
                    await AddHsCard(player, card.GetProperty("id").GetString()!, pile.Value,
                        card.TryGetProperty("upgraded", out var upgraded) && upgraded.GetBoolean());
        if (spec.TryGetProperty("player", out var own))
        {
            await HsSetCreature(player.Creature, own, choice);
            if (own.TryGetProperty("energy", out var energy))
            {
                state.LoseEnergy(state.Energy);
                state.GainEnergy(energy.GetInt32());
            }
        }
        if (spec.TryGetProperty("enemies", out var enemies))
            foreach (var enemy in enemies.EnumerateArray())
                await HsSetCreature(combat.Enemies.ElementAt(enemy.GetProperty("index").GetInt32()), enemy, choice);
        state.RecalculateCardValues();
    }
}
