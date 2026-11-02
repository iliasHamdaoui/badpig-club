namespace BadPig.Administration.Features.ListAuditLog;

/// <summary>One line of the audit log, as sent to the site.</summary>
/// <param name="Id">Line number (increasing).</param>
/// <param name="OccurredAt">Date and time of the action (UTC).</param>
/// <param name="Actor">Name of the person who did the action.</param>
/// <param name="Action">Action code (for example <c>account.disabled</c>, <c>membership.validated.2026-2027</c>, <c>title.assigned.Président</c>).</param>
/// <param name="TargetId">Account identifier of the concerned person, or null.</param>
/// <param name="TargetName">Name of the concerned person, or null when the account was deleted (no copy is kept).</param>
public sealed record ListAuditLogItem(
    long Id,
    DateTimeOffset OccurredAt,
    string Actor,
    string Action,
    string? TargetId,
    string? TargetName);
