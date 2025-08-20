using DiscordSocialSDKWrapper.Types;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper;

public unsafe partial class Functions
{
    // Global
    [LibraryImport("discord_partner_sdk")]
    public static partial void Discord_RunCallbacks();

    // Discord::Client
    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_Init(Discord_Client* self);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_AddLogCallback(Discord_Client* self,
                                               delegate* unmanaged<Discord_String, LoggingSeverity, void*, void> callback, // Discord_Client_LogCallback
                                               delegate* unmanaged<void*, void> callback__userDataFree, // Discord_FreeFn
                                               void* callback__userData,
                                               LoggingSeverity minSeverity);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_SetStatusChangedCallback(Discord_Client* self,
                                                         delegate* unmanaged<Client.Status, Client.Error, int, void*, void> cb, // Discord_Client_OnStatusChanged
                                                         delegate* unmanaged<void*, void> callback__userDataFree, // Discord_FreeFn
                                                         void* cb__userData);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_CreateAuthorizationCodeVerifier(Discord_Client* self, Discord_AuthorizationCodeVerifier* returnValue);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_GetDefaultPresenceScopes(Discord_String* returnValue);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_GetToken(Discord_Client* self, ulong applicationId, Discord_String code, Discord_String codeVerifier, Discord_String redirectUri,
                                         delegate* unmanaged<Discord_ClientResult*, Discord_String, Discord_String, AuthorizationTokenType, int, Discord_String, void*, void> callback, // Discord_Client_TokenExchangeCallback
                                         delegate* unmanaged<void*, void> callback__userDataFree, // Discord_FreeFn
                                         void* callback__userData);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_UpdateToken(Discord_Client* self, AuthorizationTokenType tokenType, Discord_String token,
                                            delegate* unmanaged<Discord_ClientResult*, void*, void> callback, // Discord_Client_UpdateTokenCallback,
                                            delegate* unmanaged<void*, void> callback__userDataFree, // Discord_FreeFn
                                            void* callback__userData);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_RefreshToken(Discord_Client* self, ulong applicationId, Discord_String token,
                                        delegate* unmanaged<Discord_ClientResult*, void*, void> callback, // Discord_Client_UpdateTokenCallback,
                                        delegate* unmanaged<void*, void> callback__userDataFree, // Discord_FreeFn
                                        void* callback__userData);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_Authorize(Discord_Client* self, Discord_AuthorizationArgs* args,
                                                          delegate* unmanaged<Discord_ClientResult*, Discord_String, Discord_String, void*, void> callback, // Discord_Client_AuthorizationCallback
                                                          delegate* unmanaged<void*, void> callback__userDataFree, // Discord_FreeFn
                                                          void* callback__userData);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_Connect(Discord_Client* self);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Client_Disconnect(Discord_Client* self);

    // Discord::AuthorizationArgs
    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_AuthorizationArgs_Init(Discord_AuthorizationArgs* self);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_AuthorizationArgs_SetClientId(Discord_AuthorizationArgs* self, ulong value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_AuthorizationArgs_SetScopes(Discord_AuthorizationArgs* self, Discord_String value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_AuthorizationArgs_SetCodeChallenge(Discord_AuthorizationArgs* self, Discord_AuthorizationCodeChallenge* value);

    // Discord::AuthorizationCodeVerifier
    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_AuthorizationCodeChallenge_Init(Discord_AuthorizationCodeChallenge* self);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_AuthorizationCodeVerifier_Challenge(Discord_AuthorizationCodeVerifier* self, Discord_AuthorizationCodeChallenge* returnValue);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_AuthorizationCodeVerifier_Verifier(Discord_AuthorizationCodeVerifier* self, Discord_String* returnValue);

    // Discord::ClientResult
    [LibraryImport("discord_partner_sdk")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool Discord_ClientResult_Successful(Discord_ClientResult* self);

    // Discord::Activity
    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_Init(Discord_Activity* self);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_SetName(Discord_Activity* self, Discord_String value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_SetType(Discord_Activity* self, ActivityTypes value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_SetStatusDisplayType(Discord_Activity* self, Discord_StatusDisplayTypes* value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_SetState(Discord_Activity* self, Discord_String* value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_SetDetails(Discord_Activity* self, Discord_String* value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_SetDetailsUrl(Discord_Activity* self, Discord_String* value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_SetApplicationId(Discord_Activity* self, ulong* value);

    [LibraryImport("discord_partner_sdk")]
    internal static partial void Discord_Activity_SetParentApplicationId(Discord_Activity* self, ulong* value);

}
