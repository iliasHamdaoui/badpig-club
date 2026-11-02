namespace BadPig.Administration.Core;

/// <summary>One weekly session of the club: a day of the week and its hours.</summary>
/// <param name="Day">Day of the week the club plays.</param>
/// <param name="StartTime">When the session starts.</param>
/// <param name="EndTime">When the session ends.</param>
public sealed record ClubSession(DayOfWeek Day, TimeOnly StartTime, TimeOnly EndTime);

/// <summary>
/// When the club plays: the same three evenings every week, except during the school holidays of the season.
/// The hours are the same for the three days; they are written here once, for the site and for the e-mails.
/// </summary>
public static class ClubSessions
{
    /// <summary>The weekly schedule of the club (Paris time).</summary>
    public static readonly IReadOnlyList<ClubSession> Weekly =
    [
        new(DayOfWeek.Tuesday, new TimeOnly(20, 0), new TimeOnly(23, 0)),
        new(DayOfWeek.Wednesday, new TimeOnly(20, 0), new TimeOnly(23, 0)),
        new(DayOfWeek.Friday, new TimeOnly(20, 0), new TimeOnly(23, 0)),
    ];

    /// <summary>The weekly session of a day, when the club plays that day.</summary>
    public static ClubSession? Of(DayOfWeek day) => Weekly.FirstOrDefault(s => s.Day == day);
}
