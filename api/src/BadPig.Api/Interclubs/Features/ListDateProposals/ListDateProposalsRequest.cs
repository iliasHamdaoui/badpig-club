using BadPig.Shared.Core;

namespace BadPig.Interclubs.Features.ListDateProposals;

/// <summary>Proposals of dates: search (team, opponent), filters on the team and on the state, sort (createdAt, team; default createdAt desc) and pagination.</summary>

public sealed record ListDateProposalsRequest : ListQuery
{
    /// <summary>Identifier of a team; empty = all the teams.</summary>
    public string? TeamId { get; init; }

    /// <summary>open (no date chosen yet) or chosen; empty = all.</summary>
    public string? Status { get; init; }
}
