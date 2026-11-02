namespace BadPig.Administration.Core;

/// <summary>A session that is going to happen, on a given day.</summary>
/// <param name="Date">Day of the session (Paris).</param>
/// <param name="StartTime">When it starts.</param>
/// <param name="EndTime">When it ends.</param>
public sealed record UpcomingSession(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime);

/// <summary>
/// Which sessions happen and which do not, from the weekly schedule and the school holidays.
/// Pure rules: the date of the day is given by the caller (Paris date, see <c>ClubTime</c>).
/// </summary>
public static class SessionRules
{
    /// <summary>How far ahead the next sessions are looked for: enough to cross a long holiday.</summary>
    private const int DaysAhead = 120;

    /// <summary>The holidays that cover a date (both days included).</summary>
    public static SchoolHoliday? HolidayOn(IEnumerable<SchoolHoliday> holidays, DateOnly date) =>
        holidays.FirstOrDefault(h => h.StartsOn <= date && date <= h.EndsOn);

    /// <summary>True when the club plays on that day: a day of the weekly schedule, outside the school holidays.</summary>
    public static bool IsOpenOn(IEnumerable<SchoolHoliday> holidays, DateOnly date) =>
        ClubSessions.Of(date.DayOfWeek) is not null && HolidayOn(holidays, date) is null;

    /// <summary>The session of a day, when the club plays that day; null otherwise.</summary>
    public static UpcomingSession? SessionOn(IEnumerable<SchoolHoliday> holidays, DateOnly date)
    {
        var weekly = ClubSessions.Of(date.DayOfWeek);
        if (weekly is null || HolidayOn(holidays, date) is not null)
            return null;

        return new UpcomingSession(date, weekly.StartTime, weekly.EndTime);
    }

    /// <summary>
    /// The next sessions from <paramref name="from"/> (that day included): the days of the weekly schedule
    /// that no school holiday closes. Fewer than <paramref name="count"/> when the holidays last longer than the search.
    /// </summary>
    public static IReadOnlyList<UpcomingSession> Next(IEnumerable<SchoolHoliday> holidays, DateOnly from, int count)
    {
        var breaks = holidays.ToList();
        var sessions = new List<UpcomingSession>();
        for (var offset = 0; offset < DaysAhead && sessions.Count < count; offset++)
        {
            if (SessionOn(breaks, from.AddDays(offset)) is { } session)
                sessions.Add(session);
        }

        return sessions;
    }
}
