using BaseLib.Utils.Attributes;
using HeavySilent.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_NIGHT_BREATH_POWER")]
public sealed class NightBreathPower : HeavySilentPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Qi>()];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player is not { } player) return;
        Flash();
        for (var i = 0; i < Amount; i++)
        {
            var qi = CombatState.CreateCard<Qi>(player);
            CardCmd.Upgrade(qi);
            await CardPileCmd.AddGeneratedCardToCombat(qi, PileType.Draw, player, CardPilePosition.Random);
        }
    }
}
