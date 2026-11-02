namespace BadPig.Interclubs.Features.ListFixtures;

/// <summary>A fixture in the calendar.</summary>
/// <param name="Id">Identifier of the fixture.</param>
/// <param name="TeamId">Identifier of the team.</param>
/// <param name="TeamName">Name of the team, e.g. "D4".</param>
/// <param name="Date">Day of the fixture (yyyy-MM-dd).</param>
/// <param name="StartTime">Start time (HH:mm:ss); null when not known.</param>
/// <param name="Opponent">The opposing club.</param>
/// <param name="Venue">home (at the club) or away.</param>
/// <param name="Place">Gym or address; null when not given.</param>
/// <param name="ClubScore">Matches won by the club, final score; null until the seven matches are decided.</param>
/// <param name="OpponentScore">Matches won by the opponent, final score; null until the seven matches are decided.</param>
/// <param name="DecidedMatches">How many of the seven matches have a result (0 to 7).</param>
/// <param name="ClubMatches">Matches won by the club so far (0 when nothing is entered).</param>
/// <param name="OpponentMatches">Matches won by the opponent so far (0 when nothing is entered).</param>
/// <param name="IsLive">True when the fixture is being played right now: it is of today, its matches are not all decided and the time of day is in its window.</param>
public sealed record ListFixturesItem(string Id, string TeamId, string TeamName, DateOnly Date, TimeOnly? StartTime, string Opponent, string Venue, string? Place,
    int? ClubScore, int? OpponentScore, int DecidedMatches, int ClubMatches, int OpponentMatches, bool IsLive);
