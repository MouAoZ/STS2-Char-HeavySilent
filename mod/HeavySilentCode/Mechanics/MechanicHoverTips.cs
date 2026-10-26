using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace HeavySilent.Mechanics;

/// <summary>规则术语不占用原版关键词枚举，使用原版额外悬停提示入口。</summary>
public static class MechanicHoverTips
{
    public static IHoverTip Mill => new HoverTip(
        new LocString("static_hover_tips", "HEAVYSILENT_MILL.title"),
        new LocString("static_hover_tips", "HEAVYSILENT_MILL.description"));
    public static IHoverTip Recruit => new HoverTip(
        new LocString("static_hover_tips", "HEAVYSILENT_RECRUIT.title"),
        new LocString("static_hover_tips", "HEAVYSILENT_RECRUIT.description"));
}
