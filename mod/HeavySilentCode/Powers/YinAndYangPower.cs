using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HeavySilent.Powers;

[CustomID("HEAVYSILENT_YIN_AND_YANG_POWER")]
public sealed class YinAndYangPower : HeavySilentPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>()];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        if (combatState.RoundNumber % 2 == 1)
            await PowerCmd.Apply<YinStrengthPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
        else
            await PowerCmd.Apply<YangDexterityPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
    }
}
