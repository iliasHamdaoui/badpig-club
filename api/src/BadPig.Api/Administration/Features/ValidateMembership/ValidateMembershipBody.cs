namespace BadPig.Administration.Features.ValidateMembership;

/// <summary>JSON body of the request (the account id comes from the route).</summary>
/// <param name="Kind">Kind of membership: <c>internal</c> (licensed by the club) or <c>external</c> (licensed in another club). Default: the kind chosen at the invitation, otherwise <c>internal</c>.</param>
public sealed record ValidateMembershipBody(string? Kind);
