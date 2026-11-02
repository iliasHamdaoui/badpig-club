using BadPig.Shared.Core;

namespace BadPig.Interclubs.Features.ListTeams;

/// <summary>Teams: search (team and captain names), filter on the season, sort (name, season; default season desc then name) and pagination.</summary>

public sealed record ListTeamsRequest : ListQuery
{
    /// <summary>Identifier of a season; empty = all the seasons.</summary>
    public string? SeasonId { get; init; }
}
