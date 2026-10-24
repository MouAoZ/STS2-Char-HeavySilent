using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;

namespace HeavySilent.Character;

// Pool 是内容的归属。基础牌和状态牌不会因此自动变成普通战斗奖励。
[CustomID("HEAVYSILENT_CARD_POOL")]
public sealed class HeavySilentCardPool : CustomCardPoolModel
{
    public override string Title => "HeavySilent";
    public override bool IsColorless => false;
    public override Color DeckEntryCardColor => new("8fbc8f");
    public override Color ShaderColor => new("8fbc8f");
    public override string BigEnergyIconPath => $"{ModEntry.ResourceRoot}/images/charui/big_energy.png";
    public override string TextEnergyIconPath => $"{ModEntry.ResourceRoot}/images/charui/text_energy.png";
}

[CustomID("HEAVYSILENT_RELIC_POOL")]
public sealed class HeavySilentRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => new("8fbc8f");
    public override string BigEnergyIconPath => $"{ModEntry.ResourceRoot}/images/charui/big_energy.png";
    public override string TextEnergyIconPath => $"{ModEntry.ResourceRoot}/images/charui/text_energy.png";
}

[CustomID("HEAVYSILENT_POTION_POOL")]
public sealed class HeavySilentPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => new("8fbc8f");
    public override string BigEnergyIconPath => $"{ModEntry.ResourceRoot}/images/charui/big_energy.png";
    public override string TextEnergyIconPath => $"{ModEntry.ResourceRoot}/images/charui/text_energy.png";
}
