using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Relics;

/// <summary>HS-RELIC-002：玉砥，仅由欧罗巴斯替换初始遗物。</summary>
[CustomID("HEAVYSILENT_JADE_WHETSTONE")]
public sealed class JadeWhetstone : HeavySilentRelic, IMillListener
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MechanicHoverTips.Mill, HoverTipFactory.Static(StaticHoverTip.Block)];

    public async Task AfterMill(PlayerChoiceContext choiceContext, CardModel source, MillResult result)
    {
        if (source.Owner != Owner || result.OrdinalThisTurn != 1) return;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, 2 * result.Amount, ValueProp.Unpowered, null);
    }
}
