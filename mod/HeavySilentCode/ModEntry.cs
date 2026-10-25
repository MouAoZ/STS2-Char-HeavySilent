using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using HarmonyLib;

namespace HeavySilent;

/// <summary>游戏在加载 DLL 后调用此入口；BaseLib 负责扫描、注册内容模型。</summary>
[ModInitializer(nameof(Initialize))]
public static class ModEntry
{
    public const string ModId = "HeavySilent";
    public const string ResourceRoot = "res://HeavySilent";
    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        new Harmony(ModId).PatchAll(typeof(ModEntry).Assembly);
        Logger.Info("HeavySilent 0.1.0 initialized.");
    }
}
