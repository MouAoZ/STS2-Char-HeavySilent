using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Mechanics;

/// <summary>
/// 奖励和铭刻的永久改牌。BaseLib CardModifier 负责任意卡牌的克隆、存档与联机序列化；
/// 原版 AddKeyword 仅改变当前实例，因此在载入及升降级后重新应用。
/// </summary>
public static class PermanentKeywords
{
    public static void Apply(CardModel card, params CardKeyword[] keywords)
    {
        card.AssertMutable();
        var flags = 0;
        foreach (var keyword in keywords)
        {
            flags |= keyword switch
            {
                CardKeyword.Retain => PermanentKeywordModifier.RetainFlag,
                CardKeyword.Innate => PermanentKeywordModifier.InnateFlag,
                _ => throw new ArgumentOutOfRangeException(nameof(keywords), keyword,
                    "Only the designed permanent Retain and Innate grants are supported.")
            };
        }
        if (flags != 0) CardModifier.AddModifier<PermanentKeywordModifier>(card, flags);
    }
}

/// <summary>Amount 是本模组稳定的位标记，不依赖原版 CardKeyword 枚举的数值。</summary>
[CustomID("HEAVYSILENT_PERMANENT_KEYWORDS")]
public sealed class PermanentKeywordModifier : CardModifier
{
    internal const int RetainFlag = 1;
    internal const int InnateFlag = 2;

    public override void OnInitialApplication() => ApplyKeywords();

    public override bool ApplyStacked(CardModifier newApplied)
    {
        Amount |= newApplied.Amount;
        ApplyKeywords();
        return true;
    }

    // BaseLib 先恢复 Owner 和 Amount，再调用 LoadSaveData；无需自建保存格式。
    public override void LoadSaveData(ModifierSave save) => ApplyKeywords();

    public override void OnUpgrade() => ApplyKeywords();

    public override void OnDowngrade()
    {
        base.OnDowngrade();
        ApplyKeywords();
    }

    private void ApplyKeywords()
    {
        if (Owner == null) return;
        if ((Amount & RetainFlag) != 0) Owner.AddKeyword(CardKeyword.Retain);
        if ((Amount & InnateFlag) != 0) Owner.AddKeyword(CardKeyword.Innate);
    }
}
