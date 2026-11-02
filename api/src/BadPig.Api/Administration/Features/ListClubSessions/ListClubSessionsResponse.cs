namespace BadPig.Administration.Features.ListClubSessions;

/// <summary>One day of the weekly schedule of the club.</summary>
/// <param name="Day">Day of the week: monday, tuesday, wednesday, thursday, friday, saturday or sunday.</param>
/// <param name="StartTime">When the session starts (HH:mm:ss, Paris).</param>
/// <param name="EndTime">When the session ends (HH:mm:ss, Paris).</param>
public sealed record ClubSessionDay(string Day, TimeOnly StartTime, TimeOnly EndTime);

/// <summary>A session to come.</summary>
/// <param name="Date">Day of the session (yyyy-MM-dd, Paris).</param>
/// <param name="StartTime">When it starts (HH:mm:ss).</param>
/// <param name="EndTime">When it ends (HH:mm:ss).</param>
/// <param name="IsToday">True when this session is today.</param>
public sealed record ClubSessionToCome(DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, bool IsToday);

/// <summary>The school holidays the club is in right now.</summary>
/// <param name="Name">Name of the period, e.g. "Vacances de la Toussaint".</param>
/// <param name="EndsOn">Last day without session (included).</param>
public sealed record ClubClosure(string Name, DateOnly EndsOn);

/// <summary>When the club plays, and what happens today.</summary>
/// <param name="Today">Today's date in Paris.</param>
/// <param name="Weekly">The weekly schedule: the days the club plays and their hours.</param>
/// <param name="IsOpenToday">True when there is a session today (a day of the schedule, outside the school holidays).</param>
/// <param name="Closure">The school holidays being lived through; null when the club is open.</param>
/// <param name="Next">The next sessions, today included, the earliest first; empty when the holidays last beyond the search.</param>
public sealed record ListClubSessionsResponse(DateOnly Today, IReadOnlyList<ClubSessionDay> Weekly, bool IsOpenToday, ClubClosure? Closure, IReadOnlyList<ClubSessionToCome> Next);
