namespace BadPig.Interclubs.Features.GetFixture;

/// <summary>A fixture.</summary>
/// <param name="Id">Identifier of the fixture.</param>
/// <param name="TeamId">Identifier of the team.</param>
/// <param name="TeamName">Name of the team, e.g. "D4".</param>
/// <param name="Season">Name of the season of the team, e.g. "2026-2027".</param>
/// <param name="CaptainId">Account identifier of the captain of the team; null when there is none.</param>
/// <param name="CaptainName">Name of the captain; null when there is none.</param>
/// <param name="Date">Day of the fixture (yyyy-MM-dd).</param>
/// <param name="StartTime">Start time (HH:mm:ss); null when not known.</param>
/// <param name="Opponent">The opposing club.</param>
/// <param name="Venue">home (at the club) or away.</param>
/// <param name="Place">Gym or address; null when not given.</param>
/// <param name="ClubScore">Matches won by the club; null until the result is entered (the seven matches).</param>
/// <param name="OpponentScore">Matches won by the opponent; null until the result is entered.</param>
/// <param name="Matches">The seven matches in the usual order (SD, MX1, MX2, DD, DH, SH1, SH2), with their players and outcome.</param>
/// <param name="CanManage">True when the signed-in person can change or delete the fixture (captain of the team, or board).</param>
public sealed record GetFixtureResponse(string Id, string TeamId, string TeamName, string Season, string? CaptainId, string? CaptainName, DateOnly Date, TimeOnly? StartTime, string Opponent, string Venue, string? Place, int? ClubScore, int? OpponentScore, IReadOnlyList<GetFixtureMatch> Matches, bool CanManage);
