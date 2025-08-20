using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper;

public class Utils
{
    public unsafe static string DiscordStringToString(Discord_String str)
    {
        return Marshal.PtrToStringUTF8(str.ptr, (int)str.size);
    }
}
