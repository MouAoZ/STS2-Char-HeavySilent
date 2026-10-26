using BaseLib.Abstracts;
using BaseLib.Utils;
using HeavySilent.Character;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace HeavySilent.Cards;

/// <summary>共享内容池与模板占位图；不把每张卡的具体效果藏在基类中。</summary>
[Pool(typeof(HeavySilentCardPool))]
public abstract class HeavySilentCard(int cost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)
    : CustomCardModel(cost, type, rarity, target, showInCardLibrary)
{
    protected virtual bool UsesMill => false;
    protected virtual bool UsesRecruit => false;
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            foreach (IHoverTip tip in base.ExtraHoverTips) yield return tip;
            if (UsesMill) yield return MechanicHoverTips.Mill;
            if (UsesRecruit) yield return MechanicHoverTips.Recruit;
        }
    }
    public override string PortraitPath => $"{ModEntry.ResourceRoot}/images/card_portraits/card.png";
    public override string BetaPortraitPath => PortraitPath;
    public override string CustomPortraitPath => $"{ModEntry.ResourceRoot}/images/card_portraits/big/card.png";
}
