using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using HeavySilent.Mechanics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HeavySilent.Cards;

/// <summary>HS-START-002：偏锋；欧罗巴斯的替换映射交给 BaseLib。</summary>
[CustomID("HEAVYSILENT_OBLIQUE_STRIKE")]
public sealed class ObliqueStrike() : HeavySilentCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy), ITranscendenceCard
{
    protected override bool UsesMill => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new MillVar(1), new CalculationBaseVar(0), new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) =>
            Math.Abs(PileType.Draw.GetPile(card.Owner).Cards.Count - PileType.Discard.GetPile(card.Owner).Cards.Count))
    ];
    public CardModel GetTranscendenceTransformedCard() => ModelDb.Card<Confluence>();
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!(await MillCmd.Execute(choiceContext, this, DynamicVars["Mill"].IntValue)).Success) return;
        await DamageCmd.Attack(DynamicVars.CalculatedDamage).FromCard(this, cardPlay).Targeting(cardPlay.Target!).Execute(choiceContext);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
