using System.Text.Json.Serialization;
using BadPig.Shared.Core;

namespace BadPig.Shared.Identity;

// Authentik responses (useful fields only).
internal sealed class AuthentikList
{
    [JsonPropertyName("results")] public List<AuthentikUser> Results { get; set; } = [];
}

internal sealed class AuthentikUser
{
    [JsonPropertyName("pk")] public int Pk { get; set; }
    [JsonPropertyName("uuid")] public string Uuid { get; set; } = "";
    [JsonPropertyName("username")] public string Username { get; set; } = "";
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("is_active")] public bool IsActive { get; set; }
    [JsonPropertyName("last_login")] public DateTimeOffset? LastLogin { get; set; }
    [JsonPropertyName("groups_obj")] public List<AuthentikGroup>? GroupsObj { get; set; }

    public AccountInfo ToAccountInfo() =>
        new(Uuid, Username, Name, Email, IsActive, (GroupsObj ?? []).Select(g => g.Name).ToArray(), LastLogin);
}

internal sealed class AuthentikGroup
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}
