using BadPig.Administration.Core;
using BadPig.Shared.Core;

namespace BadPig.Announcements.Core;

/// <summary>When an announcement ends: by default 90 days after its creation, or the date chosen by the board (Europe/Paris dates).</summary>
public static class AnnouncementExpiry
{
    public const int DefaultDays = 90;

    public static DateOnly TodayAt(DateTimeOffset now) => SeasonRules.Today(now);

    public static DateOnly DefaultEnd(DateOnly today) => today.AddDays(DefaultDays);

    /// <summary>An announcement is over the day after its end date.</summary>
    public static bool IsExpired(DateOnly expiresOn, DateOnly today) => expiresOn < today;

    /// <summary>The error to return when the chosen end date is already past; null when the date is acceptable (or not given).</summary>
    public static Error? Check(DateOnly? expiresOn, DateOnly today) =>
        expiresOn is { } date && IsExpired(date, today)
            ? Error.Validation("expiresOn", "La date de fin ne peut pas être dans le passé.")
            : null;
}
