namespace BadPig.Administration.Core;

/// <summary>
/// Membership of a person: State is one of <see cref="MembershipStates"/>, the rest describes the latest validated season,
/// and MemberSince is the day the person joined the club (their first validated membership).
/// </summary>
public sealed record MembershipStatus(
    string State, string? SeasonName, string? Kind, decimal? Amount, DateTimeOffset? ValidatedAt, string? ValidatedByName,
    DateTimeOffset? MemberSince = null)
{
    public static MembershipStatus None { get; } = new(MembershipStates.ToValidate, null, null, null, null, null);

    /// <summary>Status of a person from the seasons and from their own validated memberships.</summary>
    public static MembershipStatus Of(IReadOnlyCollection<Season> seasons, IReadOnlyCollection<Membership> memberships, DateOnly today)
    {
        var state = SeasonRules.StateOf(seasons, memberships.Select(m => m.SeasonId).ToHashSet(), today);
        var starts = seasons.ToDictionary(s => s.Id);
        var known = memberships.Where(m => starts.ContainsKey(m.SeasonId)).ToList();
        var latest = known.OrderByDescending(m => starts[m.SeasonId].StartsOn).FirstOrDefault();
        if (latest is null)
            return None with { State = state };

        // The club counts someone as a member since their first validated season.
        var since = known.Min(m => m.ValidatedAt);
        return new MembershipStatus(state, starts[latest.SeasonId].Name, latest.Kind, latest.Amount, latest.ValidatedAt, latest.ValidatedByName, since);
    }
}
