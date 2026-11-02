namespace BadPig.Interclubs.Features.CreateFixture;

/// <summary>JSON body of the creation of a fixture.</summary>
/// <param name="Date">Day of the fixture (yyyy-MM-dd).</param>
/// <param name="StartTime">Start time (HH:mm); optional.</param>
/// <param name="Opponent">The opposing club (1 to 100 characters).</param>
/// <param name="Venue">home (at the club) or away.</param>
/// <param name="Place">Gym or address (200 characters at most); optional.</param>
public sealed record CreateFixtureBody(DateOnly? Date, TimeOnly? StartTime, string? Opponent, string? Venue, string? Place = null);
