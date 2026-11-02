namespace BadPig.Administration.Core;

/// <summary>
/// A validated membership: one row per person and per season.
/// It keeps the kind (internal or external) and the amount of the fee at the validation (no payment method).
/// The rows are deleted with the account (right to erasure); the lasting trace is in the audit log.
/// </summary>
public sealed class Membership
{
    public Guid MemberId { get; set; }
    public Guid SeasonId { get; set; }
    public Season? Season { get; set; }

    /// <summary>One of <see cref="MembershipKinds"/>.</summary>
    public required string Kind { get; set; }

    /// <summary>Fee of the season for this kind of membership, copied when the membership is validated.</summary>
    public decimal Amount { get; set; }

    public DateTimeOffset ValidatedAt { get; set; }
    public Guid ValidatedById { get; set; }
    public required string ValidatedByName { get; set; }
}
