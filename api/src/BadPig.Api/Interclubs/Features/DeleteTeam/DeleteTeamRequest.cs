namespace BadPig.Interclubs.Features.DeleteTeam;

/// <summary>Deletion of a team.</summary>
/// <param name="Id">Identifier of the team.</param>
public sealed record DeleteTeamRequest(string Id);
