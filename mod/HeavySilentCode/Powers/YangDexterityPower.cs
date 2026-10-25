using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using HeavySilent.Cards.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Powers;

/// <summary>复用原版临时敏捷的施加与回合末回收。</summary>
[CustomID("HEAVYSILENT_YANG_DEXTERITY_POWER")]
public sealed class YangDexterityPower : TemporaryDexterityPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<YinAndYang>();
    public string CustomPackedIconPath => $"{ModEntry.ResourceRoot}/images/charui/text_energy.png";
    public string CustomBigIconPath => $"{ModEntry.ResourceRoot}/images/charui/big_energy.png";
    public string CustomBigBetaIconPath => CustomBigIconPath;
}
