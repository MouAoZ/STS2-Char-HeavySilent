using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Cards;

/// <summary>HS-ATK-023：舍离。</summary>
[CustomID("HEAVYSILENT_SEVERANCE")]
public sealed class Severance() : HeavySilentCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move)
    ];
    /// <summary>由奖励选择阶段调用；战斗生成、复制和转化不会触发永久删牌。</summary>
    public async Task OnRewardSelected()
    {
        var selected = (await CardSelectCmd.FromDeckForRemoval(Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).ToArray();
        if (selected.Length > 0) await CardPileCmd.RemoveFromDeck(selected);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
