namespace BadPig.Interclubs.Features.AddTeamMember;

/// <summary>Addition of a player to a team.</summary>
/// <param name="Id">Identifier of the team.</param>
/// <param name="AccountId">Account identifier of the person to add.</param>
public sealed record AddTeamMemberRequest(string Id, string AccountId);
