using DiscordRPC;

using gbfr.discord.richpresence.Configuration;
using gbfr.discord.richpresence.Template;
using gbfr.discord.richpresence.Hooks;

using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using SharedScans.Interfaces;

using CsvHelper;

using System.Globalization;

using System.Timers;

#if DEBUG
using System.Diagnostics;
#endif

namespace gbfr.discord.richpresence;

/// <summary>
/// Your mod logic goes here.
/// </summary>
public class Mod : ModBase // <= Do not Remove.
{
    /// <summary>
    /// Provides access to the mod loader API.
    /// </summary>
    private readonly IModLoader _modLoader;

    /// <summary>
    /// Provides access to the Reloaded.Hooks API.
    /// </summary>
    /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
    private readonly IReloadedHooks? _hooks;

    /// <summary>
    /// Provides access to the Reloaded logger.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// Entry point into the mod, instance that created this class.
    /// </summary>
    private readonly IMod _owner;

    /// <summary>
    /// Provides access to this mod's configuration.
    /// </summary>
    private Config _configuration;

    /// <summary>
    /// The configuration of the currently executing mod.
    /// </summary>
    private readonly IModConfig _modConfig;

    private static IStartupScanner? _startupScanner = null!;
    private static ISharedScans? _sharedScans = null!;

    private readonly GameStateHook _gameStateHook;
    private readonly NetworkSystemHooks _networkSystemHooks;
    private readonly QuestSystemHooks _questSystemHooks;

    private DiscordRpcClient? _rpc;
    private readonly System.Timers.Timer _timer;
    private DateTimeOffset _lastUpdated;
    private readonly TimeSpan _updateDelaySeconds = TimeSpan.FromSeconds(3);

    public Dictionary<string, string> _localizedUiKeys = [];
    public Dictionary<uint, string> _playerIdToName = [];
    public Dictionary<string, string> _locationToName = [];
    public Dictionary<uint, string> _questMap = [];
    public Dictionary<uint, (string? LocationKey, string? LocationImage)> _questToLocation = [];

    public Mod(ModContext context)
    {
        _modLoader = context.ModLoader;
        _hooks = context.Hooks;
        _logger = context.Logger;
        _owner = context.Owner;
        _configuration = context.Configuration;
        _modConfig = context.ModConfig;

#if DEBUG
        // Attaches debugger in debug mode; ignored in release.
        Debugger.Launch();
#endif

        var startupScannerController = _modLoader.GetController<IStartupScanner>();
        if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Unable to get IStartupScanner. Rich presence will not load!");
            return;
        }

        var sharedScansController = _modLoader.GetController<ISharedScans>();
        if (sharedScansController == null || !sharedScansController.TryGetTarget(out _sharedScans))
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Unable to get ISharedScans. Rich presence will not load!");
            return;
        }

        // TODO: Fetch from game data directly?
        try
        {
            LoadUiLocalization();
            LoadCharaNames();
            LoadQuestNames();
            LoadQuestLocations();
            LoadLocationNames();
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"[{_modConfig.ModId}] Errored while loading data files: {ex.Message}. Rich Presence/Activity will not load!", System.Drawing.Color.Red);
            return;
        }

        _logger.WriteLine($"[{_modConfig.ModId}] Database loaded.", System.Drawing.Color.Green);

        _gameStateHook = new GameStateHook(_hooks);
        _gameStateHook.Init(_startupScanner);

        _networkSystemHooks = new NetworkSystemHooks(_sharedScans);
        _networkSystemHooks.Init(_startupScanner, _modConfig);

        _questSystemHooks = new QuestSystemHooks(_sharedScans);
        _questSystemHooks.Init(_modConfig);

        if (_configuration.EnableDiscordRichPresence)
            InitializeDiscordRpc();
        else
            _logger.WriteLine($"[{_modConfig.ModId}] Discord Rich Presence/Activity is currently disabled in mod configuration.", System.Drawing.Color.Yellow);

        _timer = new System.Timers.Timer(TimeSpan.FromSeconds(3)) { AutoReset = true };
        _timer.Elapsed += OnTimerTick;
    }

    private void InitializeDiscordRpc()
    {
        if (_rpc is null)
        {
            _rpc = new DiscordRpcClient("1405660886399586304");
            _rpc.OnReady += Rpc_OnReady;
            //_rpc.RegisterUriScheme(executable: $"explorer steam://joinlobby/{881020}/");
            //_rpc.Subscribe(EventType.Join);
            //_rpc.OnJoin += _rpc_OnJoin;

            _rpc.Initialize();

            _logger.WriteLine($"[{_modConfig.ModId}] Discord RPC Initialized.", System.Drawing.Color.Green);
        }
    }

    private void _rpc_OnJoin(object sender, DiscordRPC.Message.JoinMessage args)
    {
        ;
    }

    private void DeinitializeDiscordRpc()
    {
        if (_rpc is not null)
        {
            _timer.Stop();

            _rpc.ClearPresence();
            _rpc.OnReady -= Rpc_OnReady;
            _rpc.Deinitialize();
            _rpc = null;

            _logger.WriteLine($"[{_modConfig.ModId}] Discord RPC shutdown.", System.Drawing.Color.Green);
        }
    }

    private void LoadQuestNames()
    {
        string modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);
        string fileName = Path.Combine(modDir, "Data", "quest_names", "quest_names_en.csv");
        if (!File.Exists(fileName))
            throw new FileNotFoundException("quest_names file was not found.");

        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Skip first
        csv.Read();

        while (csv.Read())
        {
            if (!uint.TryParse(csv.GetField(0), NumberStyles.HexNumber, null, out uint questId))
                continue;

            string? questName = csv.GetField(1);
            if (string.IsNullOrEmpty(questName))
                continue;
            _questMap.Add(questId, questName);
        }
    }

    private void LoadLocationNames()
    {
        string modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);
        string fileName = Path.Combine(modDir, "Data", "location_names", "location_names_en.csv");
        if (!File.Exists(fileName))
            throw new FileNotFoundException("location_names file was not found.");

        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Skip first
        csv.Read();

        while (csv.Read())
        {
            string? locationKey = csv.GetField(0);
            string? locationName = csv.GetField(2);

            _locationToName.Add(locationKey, locationName);
        }
    }

    private void LoadQuestLocations()
    {
        string modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);
        string fileName = Path.Combine(modDir, "Data", "quest_locations.csv");
        if (!File.Exists(fileName))
            throw new FileNotFoundException("quest_locations file was not found.");

        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Skip first
        csv.Read();

        while (csv.Read())
        {
            if (!uint.TryParse(csv.GetField(0), NumberStyles.HexNumber, null, out uint questId))
                continue;

            string? questLocation = csv.GetField(1);
            string? locationImage = null;
            if (csv.ColumnCount > 2)
                locationImage = csv.GetField(2);

            _questToLocation.Add(questId, (questLocation, locationImage));
        }
    }

    private void LoadCharaNames()
    {
        string modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);
        string fileName = Path.Combine(modDir, "Data", "chara_names.csv");
        if (!File.Exists(fileName))
            throw new FileNotFoundException("chara_names file was not found.");

        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Skip first
        csv.Read();

        while (csv.Read())
        {
            if (!uint.TryParse(csv.GetField(0), NumberStyles.HexNumber, null, out uint plId))
                continue;

            string? plName = csv.GetField(1);
            if (string.IsNullOrEmpty(plName))
                continue;

            _playerIdToName.Add(plId, plName);
        }
    }

    private void LoadUiLocalization()
    {
        string modDir = _modLoader.GetDirectoryForModId(_modConfig.ModId);
        string fileName = Path.Combine(modDir, "Data", "ui", "en.csv");
        if (!File.Exists(fileName))
            throw new FileNotFoundException("ui csv file was not found.");

        using var reader = new StreamReader(fileName);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        // Skip first
        csv.Read();

        while (csv.Read())
        {
            _localizedUiKeys.Add(csv.GetField(0), csv.GetField(1));
        }
    }

    void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        UpdateDiscord();
    }

    private void Rpc_OnReady(object sender, DiscordRPC.Message.ReadyMessage args)
    {
        _logger.WriteLine($"[{_modConfig.ModId}] Successfully connected to discord as {args.User.Username} ({args.User.DisplayName}).", System.Drawing.Color.Green);
        _timer.Start();

        UpdateDiscord();
    }

    private readonly List<string> _playerNames = new(4);
    private unsafe void UpdateDiscord()
    {
        var now = DateTimeOffset.UtcNow;
        if (_lastUpdated + _updateDelaySeconds > now)
            return;

        var presence = new RichPresence();
        uint questId = _questSystemHooks.GetCurrentQuestId();

        BehaviorPlayerBase* mainPlayer = _gameStateHook.GetPartyMemberPointer(0);
        string? smallImageKey = null, smallImageText = null;
        string? largeImageKey = null, largeImageText = null;
        if (mainPlayer is not null)
        {
            var objId = mainPlayer->Obj->ObjReadPtr->objId;
            uint mainCharaObjId = objId & 0x0000FF00;

            if (!_playerIdToName.TryGetValue(mainCharaObjId, out string? playerName))
                playerName = "Unknown player";

            smallImageKey = $"mini_pl{mainCharaObjId & 0x0000FF00:X4}"; // Get lower 16 bits, but also don't care about first 8 (variants)

            _playerNames.Clear();
            for (int i = 0; i < 4; i++)
            {
                BehaviorPlayerBase* player = _gameStateHook.GetPartyMemberPointer(i);
                if (player is null || player->Obj is null || player->Obj->ObjReadPtr is null)
                    break;

                var currentObjId = player->Obj->ObjReadPtr->objId;
                uint charaObjId = currentObjId & 0x0000FF00;

                if (!_playerIdToName.TryGetValue(charaObjId, out string? currentCharaName))
                    currentCharaName = "Unknown player";

                _playerNames.Add(currentCharaName);
            }

            if (_playerNames.Count > 1)
                smallImageText = $"{playerName} ({string.Join(" / ", _playerNames)})";
            else
                smallImageText = playerName;
        }

        if (questId == 0)
        {
            if (_gameStateHook.PhaseId == 0xFFFF)
            {
                presence.Details = "Loading...";
            }
            else if (_gameStateHook.PhaseId == 0xF06)
            {
                presence.Details = "In Titlescreen";
            }
        }
        else
        {
            if (_questMap.TryGetValue(questId, out string? questName))
            {
                uint category = questId >> 20;
                uint subCategory = (questId >> 12) & 0xFF;

                if (category == 4) // "multi' quest
                {
                    string diffName = subCategory switch
                    {
                        1 => "EASY",
                        2 => "NORMAL",
                        3 => "HARD",
                        4 => "VERY_HARD",
                        5 => "EXTREME",
                        6 => "MANIAC",
                        7 => "PROUD",
                        _ => string.Empty,
                    };

                    if (_localizedUiKeys.TryGetValue(diffName, out string? localizedDiffStr))
                        questName += $" ({localizedDiffStr})";
                }
                presence.Details = questName;

                if (_questToLocation.TryGetValue(questId, out (string? LocationKey, string? LocationImage) location))
                {
                    largeImageKey = location.LocationImage;

                    if (!string.IsNullOrWhiteSpace(location.LocationKey) && _locationToName.TryGetValue(location.LocationKey, out string? locationName))
                    {
                        largeImageText = locationName;
                    }
                }

            }
        }

        if (_networkSystemHooks.IsNetworkInterfaceUp && _networkSystemHooks.IsInLobby)
        {
            presence.WithState("Online Lobby")
                .WithParty(new Party() { ID = "dummy", Max = 4, Size = _networkSystemHooks.NumPlayersInLobby });

            /*
            if (_networkSystemHooks.LobbyID != 0)
            {
                presence.WithSecrets(new Secrets()
                {
                    JoinSecret = _networkSystemHooks.LobbyID.ToString(),
                });
            }
            */
        }

        presence.WithAssets(new Assets()
        {
            SmallImageKey = smallImageKey,
            SmallImageText = smallImageText,
            LargeImageKey = largeImageKey,
            LargeImageText = largeImageText,
        });

        _rpc?.SetPresence(presence);
        _lastUpdated = now;
    }

    #region Standard Overrides
    public override void ConfigurationUpdated(Config configuration)
    {
        // Apply settings from configuration.
        // ... your code here.
        _configuration = configuration;
        _logger.WriteLine($"[{_modConfig.ModId}] Config Updated: Applying");

        if (configuration.EnableDiscordRichPresence)
            InitializeDiscordRpc();
        else
            DeinitializeDiscordRpc();
    }
    #endregion

    #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Mod() { }
#pragma warning restore CS8618
    #endregion
}