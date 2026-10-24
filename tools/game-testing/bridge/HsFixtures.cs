using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_MCP;

public static partial class McpMod
{
    private static readonly HashSet<string> HsCases = new()
    {
        "strike", "strike_plus", "defend", "defend_plus", "qi", "qi_plus", "qi_shuffle",
        "qi_empty", "qi_full", "qi_near_full", "qi_no_draw", "strength", "weak", "dexterity",
        "enemy_block", "visual_cards", "save_upgrade"
    };

    private static async Task HsPrepare(string name)
    {
        if (!HsCases.Contains(name)) throw new ArgumentException($"Unknown test case: {name}");
        var run = RunManager.Instance.DebugOnlyGetState() ?? throw new InvalidOperationException("No active run.");
        var player = run.Players.Single();
        if (player.Character.Id.Entry != "HEAVYSILENT_CHARACTER") throw new InvalidOperationException("Expected HeavySilent.");
        if (name == "save_upgrade")
        {
            if (CombatManager.Instance.IsInProgress) throw new InvalidOperationException("Upgrade the permanent deck outside combat.");
            foreach (string id in new[] { "HEAVYSILENT_STRIKE", "HEAVYSILENT_DEFEND", "HEAVYSILENT_QI" })
                CardCmd.Upgrade(player.Deck.Cards.First(c => c.Id.Entry == id));
            return;
        }
        if (!CombatManager.Instance.IsInProgress || !IsPlayPhase(player)) throw new InvalidOperationException("Prepare during a playable combat turn.");
        var combat = CombatManager.Instance.DebugOnlyGetState() ?? throw new InvalidOperationException("No active combat.");
        var state = player.PlayerCombatState!;
        var choice = new ThrowingPlayerChoiceContext();

        // Only fixture arrangement uses direct setup. Assertions and card play live in the client.
        // Removing models with skipVisuals leaves stale NHandCardHolder nodes behind.
        // Exercise the normal visual cleanup as well as the model mutation.
        await CardPileCmd.RemoveFromCombat(state.AllCards.ToArray());
        foreach (var creature in combat.Enemies.Append(player.Creature))
        {
            foreach (var power in creature.Powers.ToArray()) await PowerCmd.Remove(power);
            if (creature.Block > 0) await CreatureCmd.LoseBlock(choice, creature, creature.Block, null);
            await CreatureCmd.SetMaxAndCurrentHp(creature, creature == player.Creature ? 70 : 200);
        }
        state.ResetEnergy();
        string testCard = name.StartsWith("qi") ? "HEAVYSILENT_QI" :
            name.StartsWith("defend") || name == "dexterity" ? "HEAVYSILENT_DEFEND" : "HEAVYSILENT_STRIKE";
        bool upgraded = name.EndsWith("_plus") || name is "qi_full" or "qi_near_full";
        await AddHsCard(player, testCard, PileType.Hand, upgraded);
        if (name == "visual_cards")
        {
            await AddHsCard(player, "HEAVYSILENT_DEFEND", PileType.Hand);
            await AddHsCard(player, "HEAVYSILENT_QI", PileType.Hand);
        }
        int handSize = name == "qi_full" ? CardPile.MaxCardsInHand : name == "qi_near_full" ? CardPile.MaxCardsInHand - 1 : 1;
        for (int i = 1; i < handSize; ++i) await AddHsCard(player, "HEAVYSILENT_DEFEND", PileType.Hand);
        if (name != "qi_empty")
            for (int i = 0; i < 5; ++i)
                await AddHsCard(player, "HEAVYSILENT_DEFEND", name == "qi_shuffle" ? PileType.Discard : PileType.Draw);
        if (name == "strength") await PowerCmd.Apply<StrengthPower>(choice, player.Creature, 2, null, null);
        if (name == "weak") await PowerCmd.Apply<WeakPower>(choice, player.Creature, 1, null, null);
        if (name == "dexterity") await PowerCmd.Apply<DexterityPower>(choice, player.Creature, 2, null, null);
        if (name == "qi_no_draw") await PowerCmd.Apply<NoDrawPower>(choice, player.Creature, 1, null, null);
        if (name == "enemy_block") await CreatureCmd.GainBlock(combat.Enemies.First(), 3, ValueProp.Unpowered, null);
        state.RecalculateCardValues();
    }

    private static async Task AddHsCard(Player player, string id, PileType pile, bool upgrade = false)
    {
        var template = ModelDb.AllCards.Single(c => c.Id.Entry == id);
        var combat = CombatManager.Instance.DebugOnlyGetState() ?? throw new InvalidOperationException("No active combat.");
        var card = combat.CreateCard(template, player);
        if (upgrade) CardCmd.Upgrade(card);
        await CardPileCmd.Add(card, pile);
    }
}
