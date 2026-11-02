namespace BadPig.Interclubs.Features.ListTeamCandidates;

/// <summary>A person who can be added to the team.</summary>
/// <param name="AccountId">Account identifier of the person.</param>
/// <param name="Name">Name of the person.</param>
public sealed record ListTeamCandidatesItem(string AccountId, string Name);
