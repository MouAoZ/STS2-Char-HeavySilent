using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Mechanics;

/// <summary>
/// 固定磨顶量的卡面预览。沿用原版动态变量的全局修正显示范围；
/// IntValue 保持基值，实际结算仍由 MillCmd 锁定并修正一次。
/// </summary>
public sealed class MillVar(decimal amount) : DynamicVar("Mill", amount)
{
    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode,
        Creature? target, bool runGlobalHooks)
    {
        PreviewValue = runGlobalHooks && card.CombatState != null
            ? MillCmd.ModifiedAmount(card, IntValue)
            : BaseValue;
    }
}
