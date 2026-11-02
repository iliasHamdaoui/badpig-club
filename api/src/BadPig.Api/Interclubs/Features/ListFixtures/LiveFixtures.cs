using BadPig.Interclubs.Core;

namespace BadPig.Interclubs.Features.ListFixtures;

/// <summary>
/// When a fixture is being played: it is of today (in Paris), its seven matches are not all decided yet, and the time of day is in its
/// window, which opens a quarter of an hour before the start and lasts five hours. A fixture without a start time counts all day long.
/// </summary>
internal static class LiveFixtures
{
    private static readonly TimeSpan OpensBeforeStart = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan Lasts = TimeSpan.FromHours(5);

    public static bool IsLive(Fixture fixture, DateOnly today, TimeOnly now, int decidedMatches)
    {
        if (fixture.PlayedOn != today || decidedMatches == MatchSlots.All.Length)
            return false;

        if (fixture.StartTime is null)
            return true;

        var start = fixture.StartTime.Value;
        return now.IsBetween(start.Add(-OpensBeforeStart), start.Add(Lasts));
    }
}
