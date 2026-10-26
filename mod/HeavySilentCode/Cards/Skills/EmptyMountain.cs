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

/// <summary>HS-SKL-024：空山。</summary>
[CustomID("HEAVYSILENT_EMPTY_MOUNTAIN")]
public sealed class EmptyMountain() : HeavySilentCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override bool UsesMill => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromCard<Qi>());
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 基础数量在打出时锁定，MillCmd 再应用修正与足量检查。
        int count = PileType.Draw.GetPile(Owner).Cards.Count;
        if (!(await MillCmd.Execute(choiceContext, this, count)).Success) return;
        await CardPileCmd.AddGeneratedCardToCombat(CombatState.CreateCard<Qi>(Owner), PileType.Hand, Owner);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
