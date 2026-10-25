using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_SHIFTING_MERIDIANS_POWER")]
public sealed class ShiftingMeridiansPower : HeavySilentPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override string CustomPackedIconPath => $"{ModEntry.ResourceRoot}/images/charui/text_energy.png";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MechanicHoverTips.Mill, MechanicHoverTips.Recruit];
}
