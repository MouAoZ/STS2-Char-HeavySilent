using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_FINE_TUNING_POWER")]
public sealed class FineTuningPower : HeavySilentPower, IMillModifier
{
    public override bool AllowNegative => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MechanicHoverTips.Mill];

    // 多个修正先汇总，最终的非负下限由 MillCmd 统一施加。
    public int ModifyMill(CardModel source, int amount) =>
        source.Owner.Creature == Owner ? amount + Amount : amount;
}
