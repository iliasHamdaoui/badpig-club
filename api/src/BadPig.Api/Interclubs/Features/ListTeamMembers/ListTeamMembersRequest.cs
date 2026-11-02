using BadPig.Shared.Core;

namespace BadPig.Interclubs.Features.ListTeamMembers;

/// <summary>Players of a team: search (name), sort (name; default name) and pagination.</summary>

public sealed record ListTeamMembersRequest : ListQuery
{
    /// <summary>Identifier of the team (taken from the route).</summary>
    public string Id { get; init; } = "";
}
