using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Mechanics;

/// <summary>征召逐张复用原版抽牌；每次调用前确认仍有顶牌，绝不请求空堆抽牌。</summary>
public static class RecruitCmd
{
    public static async Task<IReadOnlyList<CardModel>> Draw(PlayerChoiceContext choiceContext, int count, Player player)
    {
        List<CardModel> drawn = [];
        for (int i = 0; i < count && !CombatManager.Instance.IsOverOrEnding; i++)
        {
            if (PileType.Draw.GetPile(player).Cards.Count == 0)
                break;

            // 单张原版 Draw 保留禁抽、满手提示、History、早期/普通 Hook、动画和选择上下文。
            // 插入效果完全结束后才读下一张，不能一次 Draw(min(count, 初始牌数))。
            CardModel? card = await CardPileCmd.Draw(choiceContext, player);
            if (card is null)
                break;
            drawn.Add(card);
        }
        return drawn;
    }
}
