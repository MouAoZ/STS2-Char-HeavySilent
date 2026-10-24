using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using Godot;
using Environment = System.Environment;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace STS2_MCP;

// Development extensions share the upstream main-thread dispatcher and HTTP listener.
// They are intentionally compiled into a separate DLL, never HeavySilent.dll.
public static partial class McpMod
{
    private static readonly string HsSession = Environment.GetEnvironmentVariable("HEAVYSILENT_TEST_SESSION") ?? "";
    private static bool HsEnabled => HsSession.Length >= 24;
    private static bool _hsPreparing;
    private static bool _hsLocalSaves;
    private static long _hsRevision;
    private static int HsProfile => SaveManager.Instance.IsProfileInitialized ? SaveManager.Instance.CurrentProfileId : 0;

    private static void HsUseLocalSaves()
    {
        // Public dependency-injection seam supplied by the game. Keep real serializers,
        // migrations and asynchronous disk writes; omit only the Steam cloud backend.
        // This runs before NGame initializes profiles or starts cloud synchronization.
        if (SaveManager.Instance.IsProfileInitialized)
            throw new InvalidOperationException("Test save isolation must precede profile initialization.");
        var local = new GodotFileIo(UserDataPathProvider.GetAccountScopedBasePath(null));
        var saves = new SaveManager(local);
        saves.InitSettingsData();
        SaveManager.MockInstanceForTesting(saves);
        _hsLocalSaves = true;
        GD.Print("[HeavySilentTestBridge] Managed session uses local saves only.");
    }

    private static bool AuthorizeHsRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (!HsEnabled || request.Headers["X-HeavySilent-Session"] != HsSession)
        {
            SendError(response, 403, "This bridge requires its managed test session.");
            return false;
        }
        string path = request.Url?.AbsolutePath ?? "";
        if (request.HttpMethod != "POST" && path.StartsWith("/hs/") && path is not ("/hs/health" or "/hs/state" or "/hs/screenshot"))
        {
            SendError(response, 405, "This control route requires POST.");
            return false;
        }
        if (request.HttpMethod == "POST" && !path.StartsWith("/hs/") && path != "/api/v1/singleplayer")
        {
            SendError(response, 403, "The managed bridge permits singleplayer actions only; profile management is disabled.");
            return false;
        }
        // Profile transitions are guarded separately. Never allow upstream profile deletion.
        if (request.HttpMethod == "POST" && path != "/hs/quit" && HsProfile != 3)
        {
            SendError(response, 409, "Switch to the registered test profile 3 before performing actions.");
            return false;
        }
        return true;
    }

    private static bool TryHandleHsRequest(HttpListenerContext context)
    {
        var path = context.Request.Url?.AbsolutePath;
        if (path?.StartsWith("/hs/") != true) return false;
        try
        {
            using var body = context.Request.HttpMethod == "POST"
                ? JsonDocument.Parse(new StreamReader(context.Request.InputStream).ReadToEnd())
                : JsonDocument.Parse("{}");
            var args = body.RootElement.Clone();
            object result = RunOnMainThread(() => HsDispatch(path, args)).Unwrap().GetAwaiter().GetResult();
            SendJson(context.Response, result);
        }
        catch (Exception error)
        {
            GD.PrintErr($"[HeavySilentTestBridge] {path}: {error}");
            SendError(context.Response, 409, error.Message);
        }
        return true;
    }

    private static async Task<object> HsDispatch(string path, JsonElement args)
    {
        switch (path)
        {
            case "/hs/health":
                return new
                {
                    service = "HeavySilentTestBridge", protocol = 1,
                    pid = Environment.ProcessId, session = HsSession, profile = HsProfile,
                    ready = NGame.Instance?.GameStartupComplete.IsCompletedSuccessfully == true,
                    local_saves = _hsLocalSaves,
                    bridge_sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(McpMod).Assembly.Location))).ToLowerInvariant(),
                    heavy_silent_sha256 = AssemblyHash("HeavySilent")
                };
            case "/hs/state": return HsSnapshot();
            case "/hs/catalog":
                string kind = args.GetProperty("kind").GetString()!;
                string query = args.TryGetProperty("query", out var q) ? q.GetString() ?? "" : "";
                IEnumerable<AbstractModel> models = kind == "cards" ? ModelDb.AllCards : kind == "powers" ? ModelDb.AllPowers :
                    throw new ArgumentException("Catalog kind must be cards or powers.");
                return new { models = models.Where(m => m.Id.Entry.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Take(100).Select(m => new { id = m.Id.Entry, type = m.GetType().FullName }).ToArray() };
            case "/hs/scenario":
                if (_hsPreparing || HsBusy()) throw new InvalidOperationException("Wait for the current action to settle.");
                _hsPreparing = true;
                try { await HsPrepareScenario(args); ++_hsRevision; }
                finally { _hsPreparing = false; }
                return HsSnapshot();
            case "/hs/profile-ready":
                if (RunManager.Instance.IsInProgress || NGame.Instance?.MainMenu == null)
                    throw new InvalidOperationException("Prepare profile unlocks only on the main menu.");
                // User-approved test-profile baseline: execute the game's exact
                // `unlock all` command, so new runs never wait on timeline reveals.
                var unlocked = new MegaCrit.Sts2.Core.DevConsole.ConsoleCommands.UnlockConsoleCmd().Process(null, new[] { "all" });
                if (!unlocked.success) throw new InvalidOperationException(unlocked.msg);
                NGame.Instance.MainMenu.RefreshButtons();
                return new { command = "unlock all", profile = HsProfile, pending_epochs = SaveManager.Instance.GetDiscoveredEpochCount() };
            case "/hs/screenshot":
                await ((SceneTree)Engine.GetMainLoop()).ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                using (var shot = ((SceneTree)Engine.GetMainLoop()).Root.GetTexture().GetImage())
                    return new { png_base64 = Convert.ToBase64String(shot.SavePngToBuffer()), width = shot.GetWidth(), height = shot.GetHeight() };
            case "/hs/prepare":
                if (_hsPreparing || HsBusy()) throw new InvalidOperationException("Wait for the current action to settle.");
                _hsPreparing = true;
                try { await HsPrepare(args.GetProperty("case_id").GetString()!); ++_hsRevision; }
                finally { _hsPreparing = false; }
                return HsSnapshot();
            case "/hs/inspect-card":
                var inspection = NGame.Instance!.GetInspectCardScreen();
                if (args.TryGetProperty("close", out var close) && close.GetBoolean()) inspection.Close();
                else
                {
                    var hand = RunManager.Instance.DebugOnlyGetState()!.Players.Single().PlayerCombatState!.Hand;
                    int index = args.GetProperty("index").GetInt32();
                    if (index < 0 || index >= hand.Cards.Count) throw new ArgumentOutOfRangeException(nameof(index));
                    inspection.Open(hand.Cards.ToList(), index);
                }
                return new { status = "ok" };
            case "/hs/save-quit":
                if (!RunManager.Instance.IsInProgress) throw new InvalidOperationException("No active run to save and quit.");
                // Preserve the game's room-entry checkpoint. Never serialize a fake mid-combat checkpoint.
                if (SaveManager.Instance.CurrentRunSaveTask is { } saveTask) await saveTask;
                await (NGame.Instance ?? throw new InvalidOperationException("Game unavailable.")).ReturnToMainMenu();
                return HsSnapshot();
            case "/hs/quit":
                if (RunManager.Instance.IsInProgress) throw new InvalidOperationException("Return to menu before closing the test session.");
                ((SceneTree)Engine.GetMainLoop()).CallDeferred(SceneTree.MethodName.Quit);
                return new { status = "quitting" };
            default: throw new ArgumentException($"Unknown test route: {path}");
        }
    }

    private static string? AssemblyHash(string name)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name);
        return assembly == null ? null : Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location))).ToLowerInvariant();
    }

    private static bool HsBusy()
    {
        if (_hsPreparing) return true;
        if (NGame.Instance?.Transition?.InTransition == true) return true;
        if (!RunManager.Instance.IsInProgress) return false;
        return RunManager.Instance.ActionExecutor?.CurrentlyRunningAction != null ||
               NCombatRoom.Instance?.Ui?.Hand?.InCardPlay == true;
    }

    private static object HsCard(CardModel card) => new
    {
        id = card.Id.Entry, instance_id = RuntimeHelpers.GetHashCode(card), upgraded = card.IsUpgraded,
        title = card.Title, description = SafeGetCardDescription(card),
        keywords = card.Keywords.Select(k => k.ToString()).ToArray()
    };

    private static object HsSnapshot()
    {
        var run = RunManager.Instance.IsInProgress ? RunManager.Instance.DebugOnlyGetState() : null;
        var player = run?.Players.FirstOrDefault();
        var combat = CombatManager.Instance.IsInProgress ? player?.PlayerCombatState : null;
        return new
        {
            revision = _hsRevision, profile = HsProfile, language = LocManager.Instance.Language,
            busy = HsBusy(), in_combat = combat != null, play_phase = combat != null && IsPlayPhase(player),
            max_hand = CardPile.MaxCardsInHand, character_id = player?.Character.Id.Entry,
            seed = run?.Rng.StringSeed,
            event_ui_ready = NEventRoom.Instance != null,
            visible_hand_count = NCombatRoom.Instance?.Ui?.Hand?.ActiveHolders.Count,
            hp = player?.Creature.CurrentHp, block = player?.Creature.Block, energy = combat?.Energy,
            deck = player?.Deck.Cards.Select(HsCard).ToArray(),
            piles = combat?.AllPiles.ToDictionary(p => p.Type.ToString().ToLowerInvariant(), p => p.Cards.Select(HsCard).ToArray()),
            enemies = player?.Creature.CombatState?.Enemies.Select((c, i) => new
            {
                id = c.Monster?.Id.Entry, combat_id = c.CombatId, hp = c.CurrentHp, block = c.Block,
                powers = c.Powers.Select(p => new { id = p.Id.Entry, amount = p.Amount }).ToArray()
            }).ToArray(),
            ui = BuildGameState()
        };
    }
}
