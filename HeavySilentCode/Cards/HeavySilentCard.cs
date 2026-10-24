using BaseLib.Abstracts;
using BaseLib.Utils;
using HeavySilent.Character;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HeavySilent.Cards;

/// <summary>共享内容池与模板占位图；不把每张卡的具体效果藏在基类中。</summary>
[Pool(typeof(HeavySilentCardPool))]
public abstract class HeavySilentCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target)
{
    public override string PortraitPath => $"{ModEntry.ResourceRoot}/images/card_portraits/card.png";
    public override string BetaPortraitPath => PortraitPath;
    public override string CustomPortraitPath => $"{ModEntry.ResourceRoot}/images/card_portraits/big/card.png";
}
