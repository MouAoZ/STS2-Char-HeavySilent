using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Cards;

/// <summary>HS-START-001：凝息。</summary>
[CustomID("HEAVYSILENT_GATHER_BREATH")]
public sealed class GatherBreath() : HeavySilentCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Qi>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        var qi = CombatState!.CreateCard<Qi>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(qi, PileType.Draw, Owner, CardPilePosition.Random);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
