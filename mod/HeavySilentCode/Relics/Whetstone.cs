using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Relics;

/// <summary>HS-RELIC-001：砥石，初始遗物。</summary>
[CustomID("HEAVYSILENT_WHETSTONE")]
public sealed class Whetstone : HeavySilentRelic, IMillListener
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(2, ValueProp.Unpowered)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [MechanicHoverTips.Mill, HoverTipFactory.Static(StaticHoverTip.Block)];
    public override RelicModel GetUpgradeReplacement() => ModelDb.Relic<JadeWhetstone>();

    public async Task AfterMill(PlayerChoiceContext choiceContext, CardModel source, MillResult result)
    {
        if (source.Owner != Owner || result.OrdinalThisTurn != 1) return;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
    }
}
