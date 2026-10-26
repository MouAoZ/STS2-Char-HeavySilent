using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Cards;

/// <summary>HS-ANCIENT-001：合流。内建完美契合委托原版附魔，独立于继承的附魔槽。</summary>
[CustomID("HEAVYSILENT_CONFLUENCE")]
public sealed class Confluence() : HeavySilentCard(1, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0), new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            PileType.Draw.GetPile(card.Owner).Cards.Count + PileType.Discard.GetPile(card.Owner).Cards.Count)
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<PerfectFit>();
    public override void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
    {
        if (player != Owner || isInitialShuffle || !cards.Contains(this)) return;
        // 单附魔槽要保留偏锋原有附魔；直接复用原版 PerfectFit 的规则和提示，不复制规则代码。
        EnchantmentModel perfectFit = ModelDb.Enchantment<PerfectFit>().ToMutable();
        perfectFit.ApplyInternal(this, 1);
        perfectFit.ModifyShuffleOrder(player, cards, isInitialShuffle);
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target!).Execute(choiceContext);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
