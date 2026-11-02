namespace BadPig.Administration.Features.ResendInvitation;

/// <param name="Id">account identifier of the account (from the route).</param>
public sealed record ResendInvitationRequest(string Id);
