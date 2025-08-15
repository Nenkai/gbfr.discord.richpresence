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

namespace gbfr.discord.richpresence.Hooks;

public unsafe class QuestSystemHooks
{
    private readonly ISharedScans _scans;

    public unsafe delegate void QuestSystem__Initialize(nint this_);
    public unsafe delegate void QuestSystem__GetCurrentQuestId(nint this_, uint* outQuestId);

    public nint QuestSystemPtr;

    public static HookContainer<QuestSystem__Initialize> HOOK_QuestSystem_Initialize { get; private set; }
    public static WrapperContainer<QuestSystem__GetCurrentQuestId> FUNC_QuestSystem_GetCurrentQuestId { get; private set; }

    public Dictionary<string, string> Patterns = new()
    {
        [nameof(QuestSystem__Initialize)] = "55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? C5 78 29 8D ?? ?? ?? ?? C5 78 29 85 ?? ?? ?? ?? C5 F8 29 BD ?? ?? ?? ?? C5 F8 29 B5 ?? ?? ?? ?? 48 83 E4 ?? 48 89 E3 48 89 AB ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 49 89 CE",
        [nameof(QuestSystem__GetCurrentQuestId)] = "56 57 48 83 EC ?? 48 89 D6 48 89 CF 48 8B 89 ?? ?? ?? ?? 48 85 C9 74 ?? 48 8B 01 FF 50 ?? 84 C0 75 ?? 48 8B 87",

    };

    public QuestSystemHooks(ISharedScans scans)
    {
        _scans = scans;
    }

    public void Init(IModConfig modConfig)
    {
        foreach (var pattern in Patterns)
            _scans.AddScan(pattern.Key, pattern.Value);

        HOOK_QuestSystem_Initialize = _scans.CreateHook<QuestSystem__Initialize>(HOOK_QuestSystem_InitializeImpl, modConfig.ModId);
        FUNC_QuestSystem_GetCurrentQuestId = _scans.CreateWrapper<QuestSystem__GetCurrentQuestId>(modConfig.ModId);
    }

    private void HOOK_QuestSystem_InitializeImpl(nint ptr)
    {
        HOOK_QuestSystem_Initialize.Hook!.OriginalFunction(ptr);
        HOOK_QuestSystem_Initialize.Hook!.Disable();

        QuestSystemPtr = ptr;
    }

    public uint GetCurrentQuestId()
    {
        if (QuestSystemPtr == 0)
            return 0;

        uint questId = 0;
        FUNC_QuestSystem_GetCurrentQuestId.Wrapper(QuestSystemPtr, &questId);
        return questId;
    }
}
