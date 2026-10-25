using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Cards;

/// <summary>HS-ATK-028：涤尘。</summary>
[CustomID("HEAVYSILENT_PURGE")]
public sealed class Purge() : HeavySilentCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. base.ExtraHoverTips, HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        // 原版 FlakCannon 的范围；已消耗状态牌既不再次消耗，也不贡献攻击次数。
        var statuses = Owner.PlayerCombatState!.AllCards
            .Where(c => c.Type == CardType.Status && c.Pile?.Type != PileType.Exhaust).ToArray();
        var exhausted = 0;
        foreach (var status in statuses)
        {
            // 前一次消耗的插入效果可能已经移走后面的牌。
            if (status.Pile == null || !status.Pile.IsCombatPile || status.Pile.Type == PileType.Exhaust) continue;
            if (await CardCmd.Exhaust(choiceContext, status) != null) exhausted++;
        }
        if (exhausted > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
                .Targeting(cardPlay.Target).WithHitCount(exhausted)
                .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
