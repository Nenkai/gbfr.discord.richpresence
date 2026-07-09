using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Reloaded.Mod.Interfaces;

using Reloaded.Hooks.Definitions;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

using NenTools.Reloaded.ScanManager.Interfaces;

namespace gbfr.discord.richpresence.Hooks;

public unsafe class GameStateHook
{
    private readonly IReloadedHooks _hooks;

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

    public void Init(IScanManager scanManager, string signatureGroup)
    {
        // Quest id (bgm related code?)
        // Find (cmp     edi, cs:g_QuestId)
        // Pointer to phase id
        scanManager.AddScan("PointerToPhaseId", signatureGroup, (addr) =>
        {
            PhaseIdPtr = addr + *(int*)(addr + 2) + 10;
        });

        // Pointer to party members pointer array
        scanManager.AddScan("PointerToPartyMemberPointerArray", signatureGroup, (addr) =>
        {
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

public struct ObjReadWithAppend
{
    public nint vtable1;
    public nint vtable2;
    public int unk;
    public uint objId;
}
