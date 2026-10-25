using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_SIMPLE_GUARD_POWER")]
public sealed class SimpleGuardPower : HeavySilentPower
{
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player is not { } player) return;
        Flash();
        // 原版常规回合末 Hook 在清手牌之前运行，故此处读取清理前的当前手牌。
        await CreatureCmd.GainBlock(Owner, Amount * PileType.Hand.GetPile(player).Cards.Count, ValueProp.Unpowered, null);
    }
}
