using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reloaded.Hooks.Definitions;

using NenTools.Reloaded.ScanManager.Interfaces;

namespace gbfr.discord.richpresence.Hooks;

public unsafe class QuestSystemHooks
{
    private readonly IReloadedHooks _hooks;

    public delegate void QuestSystem__Initialize(nint this_);
    public delegate void QuestSystem__GetCurrentQuestId(nint this_, uint* outQuestId);

    public nint QuestSystemPtr;

    public static IHook<QuestSystem__Initialize> HOOK_QuestSystem_Initialize { get; private set; }
    public static QuestSystem__GetCurrentQuestId WRAPPER_QuestSystem_GetCurrentQuestId { get; private set; }

    public QuestSystemHooks(IReloadedHooks hooks)
    {
        _hooks = hooks;
    }

    public void Init(IScanManager scanManager, string signatureGroup)
    {
        scanManager.AddScan(nameof(QuestSystem__Initialize), signatureGroup, (addr) =>
            HOOK_QuestSystem_Initialize = _hooks.CreateHook<QuestSystem__Initialize>(HOOK_QuestSystem_InitializeImpl, addr).Activate());
        scanManager.AddScan(nameof(QuestSystem__GetCurrentQuestId), signatureGroup, (addr) =>
            WRAPPER_QuestSystem_GetCurrentQuestId = _hooks.CreateWrapper<QuestSystem__GetCurrentQuestId>(addr, out _));
    }

    private void HOOK_QuestSystem_InitializeImpl(nint ptr)
    {
        HOOK_QuestSystem_Initialize!.OriginalFunction(ptr);
        HOOK_QuestSystem_Initialize!.Disable();

        QuestSystemPtr = ptr;
    }

    public uint GetCurrentQuestId()
    {
        if (QuestSystemPtr == 0)
            return 0;

        uint questId = 0;
        WRAPPER_QuestSystem_GetCurrentQuestId(QuestSystemPtr, &questId);
        return questId;
    }
}
