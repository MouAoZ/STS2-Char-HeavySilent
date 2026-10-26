using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_FLYING_SAND_POWER")]
public sealed class FlyingSandPower : HeavySilentPower, IMillListener
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MechanicHoverTips.Mill];

    public async Task AfterMill(PlayerChoiceContext choiceContext, CardModel source, MillResult result)
    {
        if (source.Owner.Creature != Owner) return;
        var target = source.Owner.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
        if (target == null) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unpowered, Owner);
    }
}
