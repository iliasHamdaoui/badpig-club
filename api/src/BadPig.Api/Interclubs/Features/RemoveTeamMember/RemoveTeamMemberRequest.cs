namespace BadPig.Interclubs.Features.RemoveTeamMember;

/// <summary>Removal of a player from a team.</summary>
/// <param name="Id">Identifier of the team.</param>
/// <param name="AccountId">Account identifier of the player to remove.</param>
public sealed record RemoveTeamMemberRequest(string Id, string AccountId);
