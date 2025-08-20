using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper;

public unsafe struct Discord_String
{
    public nint ptr;
    public nint size;
}

public unsafe struct Discord_Activity
{
    void* opaque;
}

public unsafe struct Discord_Client
{
    void* opaque;
}

public unsafe struct Discord_AuthorizationCodeVerifier
{
    void* opaque;
}

public unsafe struct Discord_AuthorizationArgs
{
    void* opaque;
};

public unsafe struct Discord_ClientResult
{
    void* opaque;
};

public unsafe struct Discord_AuthorizationCodeChallenge
{
    void* opaque;
};