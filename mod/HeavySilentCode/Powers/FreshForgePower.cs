using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_FRESH_FORGE_POWER")]
public sealed class FreshForgePower : HeavySilentPower
{
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (creator == Owner.Player && card.IsUpgradable)
        {
            Flash();
            CardCmd.Upgrade(card);
        }
        return Task.CompletedTask;
    }
}
