namespace BadPig.Administration.Features.CancelMembership;

/// <param name="Id">account identifier of the member (from the route).</param>
public sealed record CancelMembershipRequest(string Id);
