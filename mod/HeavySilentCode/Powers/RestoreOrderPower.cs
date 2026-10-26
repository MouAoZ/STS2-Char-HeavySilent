using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_RESTORE_ORDER_POWER")]
public sealed class RestoreOrderPower : HeavySilentPower
{
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player is not { } player) return;
        var pile = PileType.Discard.GetPile(player);
        if (pile.IsEmpty) return;
        var chosen = (await CardSelectCmd.FromCombatPile(choiceContext, pile, player,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, Amount))).ToArray();
        if (chosen.Length == 0) return;
        Flash();
        await CardPileCmd.Add(chosen, PileType.Draw, CardPilePosition.Top);
    }
}
