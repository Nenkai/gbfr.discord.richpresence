using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NenTools.Reloaded.ScanManager.Interfaces;

using Reloaded.Hooks.Definitions;

namespace gbfr.discord.richpresence.Hooks;

public unsafe class NetworkSystemHooks
{
    private readonly IReloadedHooks _hooks;

    private nint NetworkSystemPtr;
    private LobbySteam_LobbyPlayFab* LobbyPtr;

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

    public ulong LobbyID => (LobbyPtr is not null && LobbyPtr->LobbyInfo is not null) ? LobbyPtr->LobbyInfo->SteamLobbyId : 0UL;

    public unsafe delegate int Network__GetNumPlayersInOnlineLobby(nint a1);
    public unsafe delegate bool hw__network__LobbySteam__IsInLobby(LobbySteam_LobbyPlayFab* a1); // hw::network::LobbySteam

    public static IHook<Network__GetNumPlayersInOnlineLobby> HOOK_Network__GetNumPlayersInOnlineLobby { get; private set; }
    public static IHook<hw__network__LobbySteam__IsInLobby> HOOK_hw__network__LobbySteam__IsInLobby { get; private set; }

    public NetworkSystemHooks(IReloadedHooks hooks)
    {
        _hooks = hooks;

    }

    public void Init(IScanManager scanManager, string signatureGroup)
    {
        scanManager.AddScan(nameof(Network__GetNumPlayersInOnlineLobby), signatureGroup, (addr) =>
            HOOK_Network__GetNumPlayersInOnlineLobby = _hooks.CreateHook<Network__GetNumPlayersInOnlineLobby>(HOOK_Network__GetNumPlayersInOnlineLobbyImpl, addr).Activate());
        scanManager.AddScan(nameof(hw__network__LobbySteam__IsInLobby), signatureGroup, (addr) =>
            HOOK_hw__network__LobbySteam__IsInLobby = _hooks.CreateHook<hw__network__LobbySteam__IsInLobby>(HOOK_hw__network__LobbySteam__IsInLobbyImpl, addr).Activate());

        // Quest id (bgm related code?)
        // Find (cmp     edi, cs:g_QuestId)

        scanManager.AddScan("NetworkSystemPointer", signatureGroup, addr =>
        {
            NetworkSystemPtr = addr + *(int*)(addr + 3) + 7; // +10 because size of instruction
        });
    }

    public int HOOK_Network__GetNumPlayersInOnlineLobbyImpl(nint a1)
    {
        // the argument passed to this is pretty convoluted (stack structure, where a1[0] is some structure pointer from another structure (v5 + 0x17DD8)
        // so just hook this function since it's actually what the game uses for getting the current lobby player count. a bit of overhead but it'll be fine
        NumPlayersInLobby = HOOK_Network__GetNumPlayersInOnlineLobby!.OriginalFunction(a1);
        return NumPlayersInLobby;
    }

    public bool HOOK_hw__network__LobbySteam__IsInLobbyImpl(LobbySteam_LobbyPlayFab* this_)
    {
        LobbyPtr = this_;

        IsInLobby = HOOK_hw__network__LobbySteam__IsInLobby!.OriginalFunction(this_);
        return IsInLobby;
    }
}

public struct NetworkSystem
{
    public int field_0x00;
    public int OnlineState;
}

public unsafe struct LobbySteam_LobbyPlayFab
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