using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace HeavySilent.Powers;

/// <summary>能力状态共用注册与占位图；具体触发保留在各自模型。</summary>
public abstract class HeavySilentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => $"{ModEntry.ResourceRoot}/images/charui/text_energy.png";
    public override string CustomBigIconPath => $"{ModEntry.ResourceRoot}/images/charui/big_energy.png";
    public override string CustomBigBetaIconPath => CustomBigIconPath;
}
