using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Reloaded.Mod.Interfaces;

namespace gbfr.discord.richpresence.Hooks;

public unsafe class GameStateHook
{
    private IReloadedHooks _hooks;

    public nint PlayerPosPtr;
    public nint CamPosPtr;
    public nint QuestIdPtr;
    public uint QuestId => QuestIdPtr != 0 ? *(uint*)QuestIdPtr : 0;

    public nint PhaseIdPtr;
    public ushort PhaseId => PhaseIdPtr != 0 ? *(ushort*)PhaseIdPtr : (ushort)0;

    public nint PartyMembersPtr;

    public GameStateHook(IReloadedHooks hooks)
    {
        _hooks = hooks;

    }

    public BehaviorPlayerBase* GetPartyMemberPointer(int index)
    {
        if (PartyMembersPtr == 0)
            return null;

        return ((BehaviorPlayerBase**)PartyMembersPtr)[index];
    }

    public void Init(IStartupScanner startupScanner)
    {
        // Quest id (bgm related code?)
        // Find (cmp     edi, cs:g_QuestId)
        var mainModule = Process.GetCurrentProcess().MainModule!;

        // Pointer to phase id
        startupScanner.AddMainModuleScan("C7 05 ?? ?? ?? ?? ?? ?? ?? ?? C5 F8 57 C0 C5 F8 11 05 ?? ?? ?? ?? C5 F8 11 05 ?? ?? ?? ?? C7 05 ?? ?? ?? ?? ?? ?? ?? ?? C5 F0 57 C9", e =>
        {
            nint addr = mainModule.BaseAddress + e.Offset;
            PhaseIdPtr = addr + *(int*)(addr + 2) + 10; // +10 because size of instruction
        });

        // Pointer to party members pointer array
        startupScanner.AddMainModuleScan("48 8B 0D ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 01 89 DA 41 89 F8 41 89 E9 FF 90 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 8B 50 ?? 81 FA ?? ?? ?? ?? 75 ?? BA ?? ?? ?? ?? 83 3D ?? ?? ?? ?? ?? 77 ?? 48 8B 0D ?? ?? ?? ?? 8B 50 ?? 8B 30 8B 40 ?? 89 74 24 ?? C7 44 24", e =>
        {
            nint addr = mainModule.BaseAddress + e.Offset;
            PartyMembersPtr = addr + *(int*)(addr + 3) + 7; // +7 because size of instruction
        });
    }
}

public unsafe struct BehaviorPlayerBase
{
    public nint vtable;
    public int field_0x08;
    public int field_0x0C;
    public int field_0x10;
    public int field_0x14;
    public int field_0x18;
    public int field_0x1C;
    public int field_0x20;
    public int field_0x24;
    public cObj* Obj;
}

public unsafe struct cObj
{
    public ObjReadWithAppend* ObjReadPtr;
}

public unsafe struct ObjReadWithAppend
{
    public nint vtable1;
    public nint vtable2;
    public int unk;
    public uint objId;
}
