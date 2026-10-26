using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using HeavySilent.Cards.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Powers;

/// <summary>复用原版临时力量的施加与回合末回收。</summary>
[CustomID("HEAVYSILENT_YIN_STRENGTH_POWER")]
public sealed class YinStrengthPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<YinAndYang>();
    public string CustomPackedIconPath => $"{ModEntry.ResourceRoot}/images/charui/text_energy.png";
    public string CustomBigIconPath => $"{ModEntry.ResourceRoot}/images/charui/big_energy.png";
    public string CustomBigBetaIconPath => CustomBigIconPath;
}
