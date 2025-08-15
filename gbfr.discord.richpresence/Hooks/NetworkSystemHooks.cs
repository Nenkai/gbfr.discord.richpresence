using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;

using SharedScans.Interfaces;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static gbfr.discord.richpresence.Hooks.QuestSystemHooks;

namespace gbfr.discord.richpresence.Hooks;

public unsafe class NetworkSystemHooks
{
    private ISharedScans _scans;

    private nint NetworkSystemPtr;
    private LobbySteam* LobbySteamPtr;

    /// <summary>
    /// Returns whether the network interface is up (steam), also used by FSM Condition 'IsOnlineMode'
    /// </summary>
    public bool IsNetworkInterfaceUp => (NetworkSystemPtr != 0 && *(long*)NetworkSystemPtr != 0 && ((NetworkSystem*)*(long*)NetworkSystemPtr)->OnlineState == 3);

    /// <summary>
    /// Returns the number of players in the online lobby
    /// </summary>
    public int NumPlayersInLobby { get; set; }

    /// <summary>
    /// Returns whether we're in a lobby
    /// </summary>
    public bool IsInLobby { get; set; }

    public ulong LobbyID => (LobbySteamPtr is not null && LobbySteamPtr->LobbyInfo is not null) ? LobbySteamPtr->LobbyInfo->SteamLobbyId : 0UL;

    public unsafe delegate int Network__GetNumPlayersInOnlineLobby(nint a1);
    public unsafe delegate bool hw__network__LobbySteam__IsInLobby(LobbySteam* a1); // hw::network::LobbySteam

    public static HookContainer<Network__GetNumPlayersInOnlineLobby> HOOK_Network__GetNumPlayersInOnlineLobby { get; private set; }
    public static HookContainer<hw__network__LobbySteam__IsInLobby> HOOK_hw__network__LobbySteam__IsInLobby { get; private set; }

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(Network__GetNumPlayersInOnlineLobby)] = "48 81 EC ?? ?? ?? ?? C5 79 7F BC 24 ?? ?? ?? ?? C5 79 7F B4 24 ?? ?? ?? ?? C5 79 7F 6C 24",
        [nameof(hw__network__LobbySteam__IsInLobby)] = "44 8B 89 ?? ?? ?? ?? 45 89 CA 41 C1 EA ?? 41 83 E2 ?? 45 8D 42 ?? 31 C0 41 83 F8 ?? 72 ?? 41 8D 91 ?? ?? ?? ?? 81 FA ?? ?? ?? ?? 72 ?? 44 8B 81 ?? ?? ?? ?? 41 83 FA ?? 75 ?? 31 C0 45 85 C0 74 ?? 44 89 CA",
    };

    public NetworkSystemHooks(ISharedScans scans)
    {
        _scans = scans;

    }

    public void Init(IStartupScanner startupScanner, IModConfig modConfig)
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        HOOK_Network__GetNumPlayersInOnlineLobby = _scans.CreateHook<Network__GetNumPlayersInOnlineLobby>(HOOK_Network__GetNumPlayersInOnlineLobbyImpl, modConfig.ModId);
        HOOK_hw__network__LobbySteam__IsInLobby = _scans.CreateHook<hw__network__LobbySteam__IsInLobby>(HOOK_hw__network__LobbySteam__IsInLobbyImpl, modConfig.ModId);

        // Quest id (bgm related code?)
        // Find (cmp     edi, cs:g_QuestId)
        var mainModule = Process.GetCurrentProcess().MainModule!;

        // Pointer to phase id
        startupScanner.AddMainModuleScan("48 89 05 ?? ?? ?? ?? 48 C7 40 ?? ?? ?? ?? ?? 66 C7 40", e =>
        {
            nint addr = mainModule.BaseAddress + e.Offset;
            NetworkSystemPtr = addr + *(int*)(addr + 3) + 7; // +10 because size of instruction
        });
    }

    public int HOOK_Network__GetNumPlayersInOnlineLobbyImpl(nint a1)
    {
        // the argument passed to this is pretty convoluted (stack structure, where a1[0] is some structure pointer from another structure (v5 + 0x17DD8)
        // so just hook this function since it's actually what the game uses for getting the current lobby player count. a bit of overhead but it'll be fine
        NumPlayersInLobby = HOOK_Network__GetNumPlayersInOnlineLobby.Hook!.OriginalFunction(a1);
        return NumPlayersInLobby;
    }

    public bool HOOK_hw__network__LobbySteam__IsInLobbyImpl(LobbySteam* this_)
    {
        LobbySteamPtr = this_;

        IsInLobby = HOOK_hw__network__LobbySteam__IsInLobby.Hook!.OriginalFunction(this_);
        return IsInLobby;
    }
}

public unsafe struct NetworkSystem // Size = 0x4B0
{
    public int field_0x00;
    public int OnlineState;
}

public unsafe struct LobbySteam
{
    public nint vtable;
    public fixed byte unk[0x1B8];
    public LobbyData* LobbyInfo;
}

public unsafe struct LobbyData
{
    public fixed byte unks[0x198];
    public ulong SteamLobbyId;
    // public fixed byte name[16]; + long length + long capacity
}