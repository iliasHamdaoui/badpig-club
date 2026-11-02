namespace BadPig.Administration.Features.ListUsers;

public static class AccountStatuses
{
    public const string Active = "active";
    public const string Disabled = "disabled";

    /// <summary>Invited, has not chosen a password yet.</summary>
    public const string Pending = "pending";
}
