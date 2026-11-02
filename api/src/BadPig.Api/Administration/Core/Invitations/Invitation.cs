namespace BadPig.Administration.Core;

/// <summary>
/// Who invited each person, and the kind of membership chosen at the invitation.
/// The row is deleted with the account (right to erasure); the lasting trace of the invitation stays in the audit log.
/// </summary>
public sealed class Invitation
{
    public Guid TargetId { get; set; }
    public Guid InvitedById { get; set; }
    public required string InvitedByName { get; set; }

    /// <summary>One of <see cref="MembershipKinds"/>: the default kind when the board validates the membership.</summary>
    public string MembershipKind { get; set; } = MembershipKinds.Internal;

    public DateTimeOffset CreatedAt { get; set; }
}
