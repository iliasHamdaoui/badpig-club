using BadPig.Shared.Core;

namespace BadPig.Interclubs.Features.ListFixtures;

/// <summary>Fixtures: search (opponent, team), filters on the season, the team and the period, sort (date, team, opponent; default date, descending for the past ones) and pagination.</summary>

public sealed record ListFixturesRequest : ListQuery
{
    /// <summary>Identifier of a season; empty = all the seasons.</summary>
    public string? SeasonId { get; init; }

    /// <summary>Identifier of a team; empty = all the teams.</summary>
    public string? TeamId { get; init; }

    /// <summary>upcoming (today or later), past (before today) or live (being played right now); empty = all.</summary>
    public string? When { get; init; }
}
