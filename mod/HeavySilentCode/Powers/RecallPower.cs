using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_RECALL_POWER")]
public sealed class RecallPower : HeavySilentPower
{
    public override decimal ModifyHandDraw(Player player, decimal count) =>
        player == Owner.Player ? count - 2 * Amount : count;

    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != Owner.Player) return;
        var candidates = PileType.Discard.GetPile(player).Cards.ToList();
        Flash();
        for (var i = 0; i < 3 * Amount && candidates.Count > 0; i++)
        {
            var card = player.RunState.Rng.CombatCardSelection.NextItem(candidates);
            if (card == null) break;
            candidates.Remove(card);
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }
}
