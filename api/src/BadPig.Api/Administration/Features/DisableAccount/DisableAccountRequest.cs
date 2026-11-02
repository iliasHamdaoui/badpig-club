namespace BadPig.Administration.Features.DisableAccount;

/// <param name="Id">account identifier of the account (from the route).</param>
public sealed record DisableAccountRequest(string Id);
