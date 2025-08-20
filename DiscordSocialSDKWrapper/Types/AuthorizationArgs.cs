using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper.Types;

public class AuthorizationArgs
{
    public unsafe Discord_AuthorizationArgs* NativePointer { get; }

    public unsafe AuthorizationArgs()
    {
        NativePointer = (Discord_AuthorizationArgs*)Marshal.AllocHGlobal(sizeof(nint));
        Functions.Discord_AuthorizationArgs_Init(NativePointer);
    }

    public unsafe void SetClientId(ulong value)
    {
        Functions.Discord_AuthorizationArgs_SetClientId(NativePointer, value);
    }

    public unsafe void SetScopes(string value)
    {
        using var valueHdl = DiscordStringHandle.Create(value);
        Functions.Discord_AuthorizationArgs_SetScopes(NativePointer, valueHdl.String);
    }

    public unsafe void SetCodeChallenge(AuthorizationCodeChallenge value)
    {
        Functions.Discord_AuthorizationArgs_SetCodeChallenge(NativePointer, value.NativePointer);
    }
}
