namespace BadPig.Interclubs.Features.UpdateTeam;

/// <summary>Change of a team.</summary>
/// <param name="Id">Identifier of the team.</param>
/// <param name="Body">The new values.</param>
public sealed record UpdateTeamRequest(string Id, UpdateTeamBody Body);
