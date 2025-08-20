using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSocialSDKWrapper.Types;

public class Client
{
    // FIXME: Dispose GCHandles!!!!
    // FIXME: Dispose GCHandles!!!!
    // FIXME: Dispose GCHandles!!!!

    public unsafe Discord_Client* NativePointer { get; }

    public unsafe Client()
    {
        NativePointer = (Discord_Client *)Marshal.AllocHGlobal(sizeof(nint));
        Functions.Discord_Client_Init(NativePointer);
    }

    public unsafe delegate void LogCallbackDelegate(Discord_String message, LoggingSeverity severity, void* userData);
    public unsafe delegate void LogCallbackDelegateWrapperDelegate(string message, LoggingSeverity severity, nint userData);

    public unsafe delegate void OnStatusChangedDelegate(Status status, Error error, int errorDetail, void* userData);
    public unsafe delegate void OnStatusChangedWrapperDelegate(Status status, Error error, int errorDetail, nint userData);

    public unsafe delegate void AuthorizationCallbackDelegate(Discord_ClientResult* result, Discord_String code, Discord_String redirectUri, void* userData);
    public unsafe delegate void AuthorizationCallbackWrapperDelegate(ClientResult result, string code, string redirectUri, nint userData);

    public unsafe delegate void TokenExchangeCallbackDelegate(Discord_ClientResult* result, Discord_String accessToken, Discord_String refreshToken, 
        AuthorizationTokenType tokenType, int expiresIn, Discord_String scopes, void* userData);
    public unsafe delegate void TokenExchangeCallbackWrapperDelegate(ClientResult result, string accessToken, string refreshToken,
        AuthorizationTokenType tokenType, int expiresIn, string scopes, nint userData);

    public unsafe delegate void UpdateTokenCallbackDelegate(Discord_ClientResult* result, void* userData);
    public unsafe delegate void UpdateTokenCallbackWrapperDelegate(ClientResult result, nint userData);

    public unsafe void AddLogCallback(LogCallbackDelegateWrapperDelegate callback, // Discord_Client_LogCallback
                                      delegate* unmanaged<void*, void> callback__userDataFree = null, // Discord_FreeFn
                                      void* callback__userData = null,
                                      LoggingSeverity minSeverity = LoggingSeverity.Info)
    {
        var handle = GCHandle.Alloc(callback);
        var handlePtr = (void*)GCHandle.ToIntPtr(handle);
        Functions.Discord_Client_AddLogCallback(NativePointer, &LogWrapper, callback__userDataFree, callback__userData: handlePtr, minSeverity);

        [UnmanagedCallersOnly]
        static unsafe void LogWrapper(Discord_String message, LoggingSeverity severity, void* userData)
        {
            var handle = GCHandle.FromIntPtr((nint)userData);
            var callback = (LogCallbackDelegateWrapperDelegate)handle.Target!;

            string messageStr = Marshal.PtrToStringUTF8(message.ptr, (int)message.size);
            callback(messageStr, severity, (nint)userData);
        }
    }

    public unsafe void SetStatusChangedCallback(OnStatusChangedWrapperDelegate callback, // Discord_Client_OnStatusChanged
                                                delegate* unmanaged<void*, void> callback__userDataFree = null, // Discord_FreeFn
                                                void* cb__userData = null)
    {
        var handle = GCHandle.Alloc(callback);
        var handlePtr = (void*)GCHandle.ToIntPtr(handle);
        Functions.Discord_Client_SetStatusChangedCallback(NativePointer, &SetStatusChangedCallbackTrampoline, callback__userDataFree, cb__userData: handlePtr);

        [UnmanagedCallersOnly]
        static unsafe void SetStatusChangedCallbackTrampoline(Status status, Error error, int errorDetail, void* userData)
        {
            var handle = GCHandle.FromIntPtr((nint)userData);
            var callback = (OnStatusChangedWrapperDelegate)handle.Target!;
            callback(status, error, errorDetail, (nint)userData);
        }
    }

    public unsafe void Connect() => Functions.Discord_Client_Connect(NativePointer);
    public unsafe void Disconnect() => Functions.Discord_Client_Disconnect(NativePointer);

    public unsafe void Authorize(AuthorizationArgs args, AuthorizationCallbackWrapperDelegate callback, delegate* unmanaged<void*, void> callback__userDataFree = null, void* callback__userData = null)
    {
        var handle = GCHandle.Alloc(callback);
        var handlePtr = (void*)GCHandle.ToIntPtr(handle);
        Functions.Discord_Client_Authorize(NativePointer, args.NativePointer, &AuthorizeCallbackTrampoline, callback__userDataFree, callback__userData: handlePtr);

        [UnmanagedCallersOnly]
        static unsafe void AuthorizeCallbackTrampoline(Discord_ClientResult* result, Discord_String code, Discord_String redirectUri, void* userData)
        {
            var handle = GCHandle.FromIntPtr((nint)userData);
            var callback = (AuthorizationCallbackWrapperDelegate)handle.Target!;

            var resultWrapper = new ClientResult((nint)result);
            string codeStr = Marshal.PtrToStringUTF8(code.ptr, (int)code.size);
            string redirectUriStr = Marshal.PtrToStringUTF8(redirectUri.ptr, (int)redirectUri.size);

            callback(resultWrapper, codeStr, redirectUriStr, (nint)userData);
        }
    }

    public unsafe void GetToken(ulong applicationId, string code, string codeVerifier, string redirectUri, TokenExchangeCallbackWrapperDelegate callback,
                                delegate* unmanaged<void*, void> callback__userDataFree = null, // Discord_FreeFn
                                void* callback__userData = null)
    {
        var handle = GCHandle.Alloc(callback);
        var handlePtr = (void*)GCHandle.ToIntPtr(handle);

        using var codeHdl = DiscordStringHandle.Create(code);
        using var codeVerifierHdl = DiscordStringHandle.Create(codeVerifier);
        using var redirectUriHdl = DiscordStringHandle.Create(redirectUri);

        Functions.Discord_Client_GetToken(NativePointer, applicationId, codeHdl.String, codeVerifierHdl.String, redirectUriHdl.String, &TokenExchangeCallbackTrampoline, callback__userDataFree, callback__userData: handlePtr);

        [UnmanagedCallersOnly]
        static unsafe void TokenExchangeCallbackTrampoline(Discord_ClientResult* result, Discord_String accessToken, Discord_String refreshToken,
                AuthorizationTokenType tokenType, int expiresIn, Discord_String scopes, void* userData)
        {
            var handle = GCHandle.FromIntPtr((nint)userData);
            var callback = (TokenExchangeCallbackWrapperDelegate)handle.Target!;

            var resultWrapper = new ClientResult((nint)result);
            string accessTokenStr = Utils.DiscordStringToString(accessToken);
            string refreshTokenStr = Utils.DiscordStringToString(refreshToken);
            string scopesStr = Utils.DiscordStringToString(scopes);

            callback(resultWrapper, accessTokenStr, refreshTokenStr, tokenType, expiresIn, scopesStr, (nint)userData);
        }
    }

    public unsafe void UpdateToken(AuthorizationTokenType tokenType, string token, UpdateTokenCallbackWrapperDelegate callback,
                                            delegate* unmanaged<void*, void> callback__userDataFree = null, // Discord_FreeFn
                                            void* callback__userData = null)
    {
        var handle = GCHandle.Alloc(callback);
        var handlePtr = (void*)GCHandle.ToIntPtr(handle);

        using var tokenHdl = DiscordStringHandle.Create(token);

        Functions.Discord_Client_UpdateToken(NativePointer, tokenType, tokenHdl.String, &UpdateTokenCallbackTrampoline, callback__userDataFree, callback__userData: handlePtr);
    }

    public unsafe void RefreshToken(ulong applicationId, string token, UpdateTokenCallbackWrapperDelegate callback,
                                    delegate* unmanaged<void*, void> callback__userDataFree = null, // Discord_FreeFn
                                    void* callback__userData = null)
    {
        var handle = GCHandle.Alloc(callback);
        var handlePtr = (void*)GCHandle.ToIntPtr(handle);

        using var tokenHdl = DiscordStringHandle.Create(token);

        Functions.Discord_Client_RefreshToken(NativePointer, applicationId, tokenHdl.String, &UpdateTokenCallbackTrampoline, callback__userDataFree, callback__userData: handlePtr);
    }

    [UnmanagedCallersOnly]
    static unsafe void UpdateTokenCallbackTrampoline(Discord_ClientResult* result, void* userData)
    {
        var handle = GCHandle.FromIntPtr((nint)userData);
        var callback = (UpdateTokenCallbackWrapperDelegate)handle.Target!;

        var resultWrapper = new ClientResult((nint)result);

        callback(resultWrapper, (nint)userData);
    }

    public unsafe AuthorizationCodeVerifier CreateAuthorizationCodeVerifier()
    {
        var verifier = new AuthorizationCodeVerifier();
        Functions.Discord_Client_CreateAuthorizationCodeVerifier(NativePointer, verifier.NativePointer);
        return verifier;
    }

    public unsafe string GetDefaultPresenceScopes()
    {
        Discord_String str;
        Functions.Discord_Client_GetDefaultPresenceScopes(&str);
        return Utils.DiscordStringToString(str);
    }

    public enum Status
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Ready = 3,
        Reconnecting = 4,
        Disconnecting = 5,
        HttpWait = 6,
    };

    public enum Error
    {
        None = 0,
        ConnectionFailed = 1,
        UnexpectedClose = 2,
        ConnectionCanceled = 3,
    };
}
