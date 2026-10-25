using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Cards;

/// <summary>攻击牌的气生成共用流程；仍由原版命令处理生成事件、溢出与随机插入。</summary>
internal static class AttackCardEffects
{
    public static async Task AddQi(CardModel source, int count, PileType pile, bool upgraded)
    {
        for (var i = 0; i < count; i++)
        {
            var qi = source.CombatState!.CreateCard<Qi>(source.Owner);
            if (upgraded) CardCmd.Upgrade(qi);
            var result = await CardPileCmd.AddGeneratedCardToCombat(qi, pile, source.Owner,
                pile == PileType.Draw ? CardPilePosition.Random : CardPilePosition.Bottom);
            // 入手已经有原版生成动画；抽牌／弃牌堆及满手溢出才需要额外预览。
            if (result.cardAdded.Pile?.Type is PileType.Draw or PileType.Discard)
                CardCmd.PreviewCardPileAdd(result);
        }
    }
}
