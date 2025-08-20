using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper.Types;

public class AuthorizationCodeChallenge
{
    public unsafe Discord_AuthorizationCodeChallenge* NativePointer { get; }

    public unsafe AuthorizationCodeChallenge(nint nativePointer)
    {
        NativePointer = (Discord_AuthorizationCodeChallenge*)nativePointer;
    }

    public unsafe AuthorizationCodeChallenge()
    {
        NativePointer = (Discord_AuthorizationCodeChallenge*)Marshal.AllocHGlobal(sizeof(nint));
        Functions.Discord_AuthorizationCodeChallenge_Init(NativePointer);
    }

}
