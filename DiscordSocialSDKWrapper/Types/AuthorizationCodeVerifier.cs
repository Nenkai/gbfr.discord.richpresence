using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper.Types;

public class AuthorizationCodeVerifier
{
    public unsafe Discord_AuthorizationCodeVerifier* NativePointer { get; }

    public unsafe AuthorizationCodeVerifier()
    {
        NativePointer = (Discord_AuthorizationCodeVerifier*)Marshal.AllocHGlobal(sizeof(nint));
    }

    public unsafe AuthorizationCodeVerifier(Discord_AuthorizationCodeVerifier* verifier)
    {
        NativePointer = verifier;
    }

    public unsafe string Verifier()
    {
        Discord_String str;
        Functions.Discord_AuthorizationCodeVerifier_Verifier(NativePointer, &str);
        return Utils.DiscordStringToString(str);
    }

    public unsafe AuthorizationCodeChallenge Challenge()
    {
        var challenge = new AuthorizationCodeChallenge();

        Functions.Discord_AuthorizationCodeVerifier_Challenge(NativePointer, challenge.NativePointer);
        return challenge;
        
    }
}
