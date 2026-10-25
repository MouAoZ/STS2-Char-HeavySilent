using BaseLib.Utils.Attributes;
using HeavySilent.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_LASTING_BREATH_POWER")]
public sealed class LastingBreathPower : HeavySilentPower
{
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<Qi>(), HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords)
    {
        // 全局关键词 Hook 自动覆盖已有、生成和转化后的气，不污染永久 Deck。
        return card.Owner.Creature == Owner && card is Qi && keywords.Add(CardKeyword.Retain);
    }
}
