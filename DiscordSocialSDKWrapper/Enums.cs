using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper;

public enum LoggingSeverity
{
    Verbose = 1,
    Info = 2,
    Warning = 3,
    Error = 4,
    None = 5,
};

public enum AuthorizationTokenType
{
    User = 0,
    Bearer = 1,
};

public enum Discord_StatusDisplayTypes
{
    Name = 0,
    State = 1,
    Details = 2,
}

public enum ActivityTypes
{
    Discord_ActivityTypes_Playing = 0,
    Discord_ActivityTypes_Streaming = 1,
    Discord_ActivityTypes_Listening = 2,
    Discord_ActivityTypes_Watching = 3,
    Discord_ActivityTypes_CustomStatus = 4,
    Discord_ActivityTypes_Competing = 5,
    Discord_ActivityTypes_HangStatus = 6,
}