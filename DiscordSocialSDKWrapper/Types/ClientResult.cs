using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper.Types;

public class ClientResult
{
    public unsafe Discord_ClientResult* NativePointer { get; }

    public unsafe ClientResult(nint nativePointer)
    {
        NativePointer = (Discord_ClientResult*)nativePointer;
    }

    public unsafe bool Successful()
    {
        return Functions.Discord_ClientResult_Successful(NativePointer);
    }
}
