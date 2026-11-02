namespace BadPig.Administration.Features.ValidateMembership;

/// <param name="Id">account identifier of the member (from the route).</param>
/// <param name="Kind">Kind of membership (<c>internal</c> or <c>external</c>), or null for the default.</param>
public sealed record ValidateMembershipRequest(string Id, string? Kind);
