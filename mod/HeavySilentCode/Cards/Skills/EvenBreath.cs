using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Cards.Skills;

/// <summary>HS-SKL-013：匀息。</summary>
[CustomID("HEAVYSILENT_EVEN_BREATH")]
public sealed class EvenBreath() : HeavySilentCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Qi>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int draw = PileType.Draw.GetPile(Owner).Cards.Count(c => c is Qi);
        int discard = PileType.Discard.GetPile(Owner).Cards.Count(c => c is Qi);
        int count = Math.Abs(draw - discard);
        PileType destination = draw < discard ? PileType.Draw : PileType.Discard;
        var cards = new List<CardModel>();
        for (int i = 0; i < count; i++) cards.Add(CombatState.CreateCard<Qi>(Owner));
        var results = await CardPileCmd.AddGeneratedCardsToCombat(cards, destination, Owner,
            destination == PileType.Draw ? CardPilePosition.Random : CardPilePosition.Bottom);
        CardCmd.PreviewCardPileAdd(results);
        await CreatureCmd.GainBlock(Owner.Creature, results.Count(r => r.success), ValueProp.Move, cardPlay);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
