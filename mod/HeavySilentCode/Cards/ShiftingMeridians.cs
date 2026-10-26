using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using HeavySilent.Character;
using HeavySilent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace HeavySilent.Cards;

/// <summary>HS-ANCIENT-002：易脉，达弗按原版流程给予升级版。</summary>
[CustomID("HEAVYSILENT_SHIFTING_MERIDIANS")]
public sealed class ShiftingMeridians() : HeavySilentCard(1, CardType.Power, CardRarity.Ancient, TargetType.Self), ITomeCard
{
    public CharacterModel TomeCharacter => ModelDb.Character<HeavySilentCharacter>();
    protected override bool UsesMill => true;
    protected override bool UsesRecruit => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. base.ExtraHoverTips, HoverTipFactory.FromCard<Qi>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await PowerCmd.Apply<ShiftingMeridiansPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
