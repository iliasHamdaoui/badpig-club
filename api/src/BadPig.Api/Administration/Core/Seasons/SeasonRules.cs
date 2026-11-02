using BadPig.Shared.Core;

namespace BadPig.Administration.Core;

/// <summary>
/// Rules about seasons, without any database access (the callers load the seasons, which are few).
/// Dates are Europe/Paris dates.
/// </summary>
public static class SeasonRules
{
    public const int DefaultGraceDays = 30;
    public const int MaxGraceDays = 120;

    /// <summary>Today's date in Paris (see <see cref="ClubTime"/>).</summary>
    public static DateOnly Today(DateTimeOffset now) => ClubTime.Today(now);

    /// <summary>Default end of a season that starts on <paramref name="startsOn"/>: one year later, minus a day (1 September to 31 August).</summary>
    public static DateOnly DefaultEnd(DateOnly startsOn) => startsOn.AddYears(1).AddDays(-1);

    /// <summary>Default name of a season, e.g. "2026-2027".</summary>
    public static string DefaultName(DateOnly startsOn, DateOnly endsOn) => $"{startsOn.Year}-{endsOn.Year}";

    /// <summary>
    /// The current season: the one that contains the date; between two seasons (or before the board created the next one),
    /// the latest season that has started. Null when no season has started yet.
    /// </summary>
    public static Season? Current(IEnumerable<Season> seasons, DateOnly today)
    {
        var started = seasons.Where(s => s.StartsOn <= today).OrderByDescending(s => s.StartsOn).ToList();
        return started.FirstOrDefault(s => today <= s.EndsOn) ?? started.FirstOrDefault();
    }

    /// <summary>The season just before <paramref name="current"/>, if any.</summary>
    public static Season? Previous(IEnumerable<Season> seasons, Season current) =>
        seasons.Where(s => s.StartsOn < current.StartsOn).OrderByDescending(s => s.StartsOn).FirstOrDefault();

    /// <summary>True when the two periods share at least one day.</summary>
    public static bool Overlaps(DateOnly startsOn, DateOnly endsOn, Season other) => startsOn <= other.EndsOn && other.StartsOn <= endsOn;

    /// <summary>
    /// One of <see cref="MembershipStates"/> for a person who has validated memberships for the given seasons:
    /// valid for the current season; "grace" for the previous season during the first days of the current one.
    /// </summary>
    public static string StateOf(IReadOnlyCollection<Season> seasons, IReadOnlySet<Guid> validatedSeasonIds, DateOnly today)
    {
        var current = Current(seasons, today);
        if (current is null)
            return MembershipStates.ToValidate;

        if (validatedSeasonIds.Contains(current.Id))
            return MembershipStates.Valid;

        var previous = Previous(seasons, current);
        var inGracePeriod = today < current.StartsOn.AddDays(current.GraceDays);
        return previous is not null && validatedSeasonIds.Contains(previous.Id) && inGracePeriod
            ? MembershipStates.Grace
            : MembershipStates.ToValidate;
    }

    /// <summary>Fee of the season for a kind of membership (see <see cref="MembershipKinds"/>).</summary>
    public static decimal FeeFor(Season season, string kind) => kind == MembershipKinds.External ? season.ExternalFee : season.InternalFee;
}
