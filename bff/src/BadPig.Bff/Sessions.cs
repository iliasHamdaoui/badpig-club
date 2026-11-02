using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace BadPig.Bff;

/// <summary>
/// Tokens of the signed-in people, kept in the server's memory. The browser cookie only holds a session identifier
/// ("sid"): no token ever leaves the server. When the BFF restarts, sessions are lost and everyone signs in again
/// (without typing anything if the Authentik session is still open).
/// </summary>
public sealed class Sessions(IHttpClientFactory httpFactory, IOptionsMonitor<OpenIdConnectOptions> oidc, ILogger<Sessions> log)
{
    private static readonly TimeSpan IdleTimeout = TimeSpan.FromHours(12);
    private static readonly TimeSpan Margin = TimeSpan.FromSeconds(30);

    private sealed class Session(string accessToken, string? refreshToken, string? idToken, DateTimeOffset expiresAt)
    {
        public string AccessToken = accessToken;
        public string? RefreshToken = refreshToken;
        public string? IdToken = idToken;
        public DateTimeOffset ExpiresAt = expiresAt;
        public DateTimeOffset LastUsedAt = DateTimeOffset.UtcNow;
        public readonly SemaphoreSlim Lock = new(1, 1);
    }

    private readonly ConcurrentDictionary<string, Session> _sessions = new();

    public string Create(string accessToken, string? refreshToken, string? idToken)
    {
        Purge();
        var sid = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
        _sessions[sid] = new Session(accessToken, refreshToken, idToken,
            BadPig.Bff.AccessToken.Parse(accessToken)?.ExpiresAt ?? DateTimeOffset.UtcNow.AddMinutes(1));
        return sid;
    }

    public bool Exists(string sid) => _sessions.ContainsKey(sid);

    public string? IdToken(string sid) => _sessions.TryGetValue(sid, out var s) ? s.IdToken : null;

    public void Remove(string sid) => _sessions.TryRemove(sid, out _);

    /// <summary>
    /// Access token valid for this session, refreshed with Authentik if needed.
    /// Returns null if the session no longer exists or if the refresh is refused (disabled account, expired session):
    /// the session is then removed and the person has to sign in again.
    /// </summary>
    public async Task<string?> ValidAccessToken(string sid, CancellationToken ct)
    {
        if (!_sessions.TryGetValue(sid, out var s))
            return null;

        s.LastUsedAt = DateTimeOffset.UtcNow;
        if (s.ExpiresAt - Margin > DateTimeOffset.UtcNow)
            return s.AccessToken;

        // One refresh at a time per session (simultaneous calls share the result).
        await s.Lock.WaitAsync(ct);
        try
        {
            if (s.ExpiresAt - Margin > DateTimeOffset.UtcNow)
                return s.AccessToken;

            if (s.RefreshToken is null || !await Refresh(s, ct))
            {
                Remove(sid);
                return null;
            }

            return s.AccessToken;
        }
        finally
        {
            s.Lock.Release();
        }
    }

    private async Task<bool> Refresh(Session s, CancellationToken ct)
    {
        try
        {
            var options = oidc.Get("oidc");
            var configuration = await options.ConfigurationManager!.GetConfigurationAsync(ct);
            using var request = new HttpRequestMessage(HttpMethod.Post, configuration.TokenEndpoint)
            {
                // FormUrlEncodedContent announces its length (Content-Length): required by Authentik.
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["refresh_token"] = s.RefreshToken!,
                    ["client_id"] = options.ClientId!,
                    ["client_secret"] = options.ClientSecret ?? "",
                }),
            };
            using var response = await httpFactory.CreateClient("authentik").SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                log.LogInformation("Session refresh refused by Authentik (HTTP {Code})", (int)response.StatusCode);
                return false;
            }

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var root = doc.RootElement;
            var accessToken = root.GetProperty("access_token").GetString()!;
            s.AccessToken = accessToken;
            if (root.TryGetProperty("refresh_token", out var r) && r.ValueKind == JsonValueKind.String)
                s.RefreshToken = r.GetString();

            if (root.TryGetProperty("id_token", out var i) && i.ValueKind == JsonValueKind.String)
                s.IdToken = i.GetString();

            s.ExpiresAt = BadPig.Bff.AccessToken.Parse(accessToken)?.ExpiresAt
                ?? DateTimeOffset.UtcNow.AddSeconds(root.TryGetProperty("expires_in", out var e) && e.TryGetInt32(out var sec) ? sec : 60);
            return true;
        }
        catch (Exception e) when (e is HttpRequestException or JsonException or KeyNotFoundException or TaskCanceledException)
        {
            log.LogWarning(e, "Session refresh failed");
            return false;
        }
    }

    private void Purge()
    {
        var limit = DateTimeOffset.UtcNow - IdleTimeout;
        foreach (var (sid, s) in _sessions)
        {
            if (s.LastUsedAt < limit)
                _sessions.TryRemove(sid, out _);
        }
    }
}
