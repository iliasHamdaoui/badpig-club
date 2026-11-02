namespace BadPig.Shared.Core;

/// <summary>An account of the identity provider, as seen by every module. Id = account identifier.</summary>
public sealed record AccountInfo(
    string Id,
    string Username,
    string Name,
    string Email,
    bool IsActive,
    string[] Groups,
    DateTimeOffset? LastLogin);
