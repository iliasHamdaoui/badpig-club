namespace BadPig.Interclubs.Features.ListTeams;

/// <summary>A team in the list.</summary>
/// <param name="Id">Identifier of the team.</param>
/// <param name="Name">Name of the team, e.g. "D4".</param>
/// <param name="SeasonId">Identifier of the season the team plays in.</param>
/// <param name="Season">Name of that season, e.g. "2026-2027".</param>
/// <param name="CaptainId">Account identifier of the captain; null when there is none.</param>
/// <param name="CaptainName">Name of the captain; null when there is none.</param>
/// <param name="IsMyTeam">True when the signed-in person is the captain of the team.</param>
/// <param name="MemberCount">Number of players in the team.</param>
/// <param name="IsPlayer">True when the signed-in person is a player of the team.</param>
public sealed record ListTeamsItem(string Id, string Name, string SeasonId, string Season, string? CaptainId, string? CaptainName, bool IsMyTeam, int MemberCount, bool IsPlayer);
