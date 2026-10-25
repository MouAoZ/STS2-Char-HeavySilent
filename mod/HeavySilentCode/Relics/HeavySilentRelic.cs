using BaseLib.Abstracts;
using BaseLib.Utils;
using HeavySilent.Character;

namespace HeavySilent.Relics;

/// <summary>独立遗物池与自有占位图；正式美术不属于现有玩法实现范围。</summary>
[Pool(typeof(HeavySilentRelicPool))]
public abstract class HeavySilentRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{ModEntry.ResourceRoot}/images/charui/text_energy.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => $"{ModEntry.ResourceRoot}/images/charui/big_energy.png";
}
