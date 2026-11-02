namespace BadPig.Interclubs.Features.UpdateTeam;

/// <summary>JSON body of the change of a team.</summary>
/// <param name="Name">Name of the team (1 to 30 characters).</param>
/// <param name="SeasonId">Identifier of the season the team plays in.</param>
public sealed record UpdateTeamBody(string? Name, string? SeasonId);
