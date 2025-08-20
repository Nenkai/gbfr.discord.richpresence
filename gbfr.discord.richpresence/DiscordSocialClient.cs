using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DiscordSocialSDKWrapper;
using DiscordSocialSDKWrapper.Types;

using Reloaded.Mod.Interfaces;

namespace gbfr.discord.richpresence;

public class DiscordSocialClient
{
    private Client _client;
    private AuthorizationCodeVerifier _codeVerifier;

    private ILogger _logger;

    private ulong APPLICATION_ID = 1405660886399586304;

    public DiscordSocialClient(ILogger logger)
    {
        _logger = logger;
    }

    public unsafe void Initialize()
    {
        _client = new Client();
        _client.AddLogCallback(LogCallback, minSeverity: LoggingSeverity.Info);
        _client.SetStatusChangedCallback(StatusChangedCallback);

        if (false)
        {
            if (false)
            {
                // If the access token expired, generate one with the refresh token.
                _client.RefreshToken(APPLICATION_ID, "refresh token goes here", OnUpdateTokenResult);
            }
            else
            {
                // If we have a valid access token, use it.
                _client.UpdateToken(AuthorizationTokenType.Bearer, "access token goes here", OnUpdateTokenResult);
            }
        }
        else
        {
            // We don't have an access or refresh token, prompt user to authorize on the discord UI
            _codeVerifier = _client.CreateAuthorizationCodeVerifier();

            AuthorizationArgs args = new();
            args.SetClientId(APPLICATION_ID);
            args.SetScopes(_client.GetDefaultPresenceScopes());
            args.SetCodeChallenge(_codeVerifier.Challenge());

            _client.Authorize(args, AuthorizeCallback);
        }

        var thread = new Thread(RunDiscordCallbacks) { IsBackground = true };
        thread.Start();
    }

    private void RunDiscordCallbacks()
    {
        while (true)
        {
            Functions.Discord_RunCallbacks();
            Thread.Sleep(10);
        }
    }

    public unsafe void AuthorizeCallback(ClientResult result, string code, string redirectUri, nint userData)
    {
        if (result.Successful())
        {
            _client.GetToken(APPLICATION_ID, code, _codeVerifier.Verifier(), redirectUri, OnGetTokenResult);
        }
    }

    public unsafe void OnGetTokenResult(ClientResult result, string accessToken, string refreshToken,
            AuthorizationTokenType tokenType, int expiresIn, string scopes, nint userData)
    {
        File.WriteAllLines("token.txt", 
        [
            accessToken,
            refreshToken,
            expiresIn.ToString(),
            scopes
         ]);
        _client.UpdateToken(AuthorizationTokenType.Bearer, accessToken, OnUpdateTokenResult);
    }

    private void OnUpdateTokenResult(ClientResult result, nint userData)
    {
        if (result.Successful())
        {
            _client.Connect();
        }
    }

    public void LogCallback(string message, LoggingSeverity severity, nint userdata)
    {
        System.Drawing.Color color = severity switch
        {
            LoggingSeverity.Verbose => System.Drawing.Color.Gray,
            LoggingSeverity.Info => System.Drawing.Color.White,
            LoggingSeverity.Warning => System.Drawing.Color.Yellow,
            LoggingSeverity.Error => System.Drawing.Color.Red,
        };

        _logger.Write($"[DiscordSocialSDK] {message}", color);
        
    }

    public unsafe void StatusChangedCallback(Client.Status status, Client.Error error, int errorDetail, nint userData)
    {
        if (status == Client.Status.Ready)
        {
            _logger.WriteLine("[DiscordSocialSDK] Discord client is ready.");
        }
    }
}
