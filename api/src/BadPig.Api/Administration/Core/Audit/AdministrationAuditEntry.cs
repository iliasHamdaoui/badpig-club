namespace BadPig.Administration.Core;

/// <summary>
/// One administrative action of the board. Only account identifiers (UUID) are kept for the people concerned:
/// no name or e-mail, so that deleting an account (right to erasure) leaves no personal data here.
/// Only the name of the board member who acts is kept (proof of who did what).
/// The table is immutable: a database trigger refuses any update or delete.
/// </summary>
public sealed class AdministrationAuditEntry
{
    public long Id { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public Guid ActorId { get; set; }
    public required string ActorName { get; set; }

    /// <summary>Action code, e.g. "account.disabled" (see AccountAuditActions and BoardAuditActions).</summary>
    public required string Action { get; set; }

    public Guid? TargetId { get; set; }
}
