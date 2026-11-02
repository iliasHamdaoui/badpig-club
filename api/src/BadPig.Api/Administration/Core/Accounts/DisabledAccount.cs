namespace BadPig.Administration.Core;

/// <summary>
/// Disabling date of each disabled account: an account disabled for more than a month is deleted automatically.
/// The row disappears when the account is re-enabled or deleted.
/// </summary>
public sealed class DisabledAccount
{
    public Guid AccountId { get; set; }
    public DateTimeOffset DisabledSince { get; set; }

    /// <summary>Suspension delay after which a disabled account is deleted.</summary>
    public static DateTimeOffset DeletionDate(DateTimeOffset disabledSince) => disabledSince.AddMonths(1);
}
