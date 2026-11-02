namespace BadPig.Administration.Features.DeleteAccount;

/// <param name="Id">account identifier of the account (from the route).</param>
public sealed record DeleteAccountRequest(string Id);
