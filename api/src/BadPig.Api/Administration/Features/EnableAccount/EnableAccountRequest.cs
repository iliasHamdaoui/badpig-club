namespace BadPig.Administration.Features.EnableAccount;

/// <param name="Id">account identifier of the account (from the route).</param>
public sealed record EnableAccountRequest(string Id);
