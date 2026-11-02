namespace BadPig.Shared.Core;

/// <summary>
/// The club lives in Paris: the dates and the times it compares or shows are Paris ones, whatever the clock of the server.
/// </summary>
public static class ClubTime
{
    private static readonly TimeZoneInfo Paris = FindTimeZone();

    private static TimeZoneInfo FindTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
    }

    /// <summary>The date and the time in Paris.</summary>
    public static DateTime Now(DateTimeOffset utcNow) => TimeZoneInfo.ConvertTime(utcNow, Paris).DateTime;

    /// <summary>Today's date in Paris.</summary>
    public static DateOnly Today(DateTimeOffset utcNow) => DateOnly.FromDateTime(Now(utcNow));

    /// <summary>The time of day in Paris.</summary>
    public static TimeOnly TimeOfDay(DateTimeOffset utcNow) => TimeOnly.FromDateTime(Now(utcNow));
}
