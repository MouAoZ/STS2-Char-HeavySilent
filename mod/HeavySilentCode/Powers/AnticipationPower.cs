using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_ANTICIPATION_POWER")]
public sealed class AnticipationPower : HeavySilentPower, IMillListener
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MechanicHoverTips.Mill];

    public async Task BeforeMill(PlayerChoiceContext choiceContext, CardModel source, int amount)
    {
        if (source.Owner.Creature != Owner) return;
        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, source.Owner);
    }
}
