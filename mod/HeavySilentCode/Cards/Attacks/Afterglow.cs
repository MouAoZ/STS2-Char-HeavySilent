using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Cards;

/// <summary>HS-ATK-029：回光。</summary>
[CustomID("HEAVYSILENT_AFTERGLOW")]
public sealed class Afterglow() : HeavySilentCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override bool UsesMill => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(40, ValueProp.Move),
        new MillVar(5),
        new DynamicVar("Heal", 40)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        if (!(await MillCmd.Execute(choiceContext, this, DynamicVars["Mill"].IntValue)).Success) return;
        if (cardPlay.Target is { IsAlive: true } target)
            await CreatureCmd.Heal(target, DynamicVars["Heal"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Mill"].UpgradeValueBy(2);
    }
}
