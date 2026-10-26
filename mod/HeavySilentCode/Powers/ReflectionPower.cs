using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_REFLECTION_POWER")]
public sealed class ReflectionPower : HeavySilentPower
{
    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
    {
        if (player == Owner.Player && retainedCards.Count > 0)
        {
            Flash();
            // 原版提供实际保留集合，包含全手牌保留，升级仅作用于战斗实例。
            CardCmd.Upgrade(retainedCards, CardPreviewStyle.HorizontalLayout);
        }
        return Task.CompletedTask;
    }
}
