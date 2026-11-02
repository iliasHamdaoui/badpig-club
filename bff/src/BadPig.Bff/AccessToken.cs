using System.Text.Json;

namespace BadPig.Bff;

/// <summary>
/// Reads the content of an access token received directly from Authentik (the signature is NOT verified).
/// Only used to display the name and groups; the API really validates every token.
/// </summary>
public static class AccessToken
{
    public sealed record Payload(string? Sub, string? Name, string[] Groups, DateTimeOffset? ExpiresAt);

    public static Payload? Parse(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
            return null;

        try
        {
            var base64 = parts[1].Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            using var doc = JsonDocument.Parse(Convert.FromBase64String(base64));
            var root = doc.RootElement;

            string? Text(string name) => root.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

            var groups = root.TryGetProperty("groups", out var g) && g.ValueKind == JsonValueKind.Array
                ? g.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!).ToArray()
                : [];
            DateTimeOffset? expiresAt = root.TryGetProperty("exp", out var e) && e.TryGetInt64(out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds)
                : null;
            return new Payload(Text("sub"), Text("name") ?? Text("preferred_username"), groups, expiresAt);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }
}
