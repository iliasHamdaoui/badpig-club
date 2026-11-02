using BadPig.Shared.Core;

namespace BadPig.Interclubs.Features.ListTeamCandidates;

/// <summary>People who can join a team: search (name), sort (name; default name) and pagination.</summary>

public sealed record ListTeamCandidatesRequest : ListQuery
{
    /// <summary>Identifier of the team (taken from the route).</summary>
    public string Id { get; init; } = "";
}
