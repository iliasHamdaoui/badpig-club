using System.Text.Json;

namespace BadPig.Bff;

/// <summary>
/// Lecture (sans vérification de signature) du contenu d'un jeton d'accès reçu directement d'Authentik.
/// Sert uniquement à afficher le nom et les groupes ; c'est l'API qui valide réellement chaque jeton.
/// </summary>
public static class Jeton
{
    public sealed record Contenu(string? Sub, string? Nom, string[] Groupes, DateTimeOffset? Expire);

    public static Contenu? Lire(string jeton)
    {
        var parties = jeton.Split('.');
        if (parties.Length != 3)
        {
            return null;
        }

        try
        {
            var base64 = parties[1].Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            using var doc = JsonDocument.Parse(Convert.FromBase64String(base64));
            var racine = doc.RootElement;

            string? Texte(string nom) => racine.TryGetProperty(nom, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

            var groupes = racine.TryGetProperty("groups", out var g) && g.ValueKind == JsonValueKind.Array
                ? g.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!).ToArray()
                : [];
            DateTimeOffset? expire = racine.TryGetProperty("exp", out var e) && e.TryGetInt64(out var secondes)
                ? DateTimeOffset.FromUnixTimeSeconds(secondes)
                : null;
            return new Contenu(Texte("sub"), Texte("name") ?? Texte("preferred_username"), groupes, expire);
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }
}
