using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper.Types;

public class Activity
{
    public unsafe Discord_Activity* NativePointer { get; }

    public unsafe Activity()
    {
        NativePointer = (Discord_Activity*)Marshal.AllocHGlobal(sizeof(nint));
        Functions.Discord_Activity_Init(NativePointer);
    }

    public unsafe void SetName(string name)
    {
        using var nameHdl = DiscordStringHandle.Create(name);
        Functions.Discord_Activity_SetName(NativePointer, nameHdl.String);
    }

    public unsafe void SetType(ActivityTypes value) => Functions.Discord_Activity_SetType(NativePointer, value);
    // SetStatusDisplayType

    public unsafe void SetState(string value)
    {
        using var valueHdl = DiscordStringHandle.Create(value);
        Functions.Discord_Activity_SetState(NativePointer, &valueHdl.String);
    }

    public unsafe void SetDetails(string value)
    {
        using var valueHdl = DiscordStringHandle.Create(value);
        Functions.Discord_Activity_SetDetails(NativePointer, &valueHdl.String);
    }

}
