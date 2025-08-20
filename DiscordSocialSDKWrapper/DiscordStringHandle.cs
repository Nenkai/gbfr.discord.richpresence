using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper;

public struct DiscordStringHandle : IDisposable
{
    public Discord_String String;

    public DiscordStringHandle()
    {
        String = new();
    }

    public static DiscordStringHandle Create(string str)
    {
        var handle = new DiscordStringHandle();

        if (!string.IsNullOrEmpty(str) && str.Length > 0)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            handle.String.ptr = Marshal.AllocHGlobal(bytes.Length);
            handle.String.size = bytes.Length;

            Marshal.Copy(bytes, 0, handle.String.ptr, (int)handle.String.size);
        }
        else
        {
            handle.String.ptr = Marshal.AllocHGlobal(0);
        }

        return handle;
    }

    public void Dispose()
    {
        if (String.ptr != 0)
            Marshal.FreeHGlobal(String.ptr);
    }
}
