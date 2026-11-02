namespace BadPig.Interclubs.Features.CreateTeam;

/// <summary>JSON body of the creation of a team.</summary>
/// <param name="Name">Name of the team (1 to 30 characters), e.g. "D4".</param>
/// <param name="SeasonId">Identifier of the season the team plays in.</param>
public sealed record CreateTeamRequest(string? Name, string? SeasonId);
