using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_FLOWING_GUARD_POWER")]
public sealed class FlowingGuardPower : HeavySilentPower, IMillListener
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MechanicHoverTips.Mill];

    public async Task AfterMill(PlayerChoiceContext choiceContext, CardModel source, MillResult result)
    {
        if (source.Owner.Creature != Owner) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, result.Amount * Amount, ValueProp.Unpowered, null);
    }
}
