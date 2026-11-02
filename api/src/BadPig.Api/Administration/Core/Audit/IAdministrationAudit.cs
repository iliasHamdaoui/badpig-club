namespace BadPig.Administration.Core;

/// <summary>
/// Audit of the administrative operations of the club (accounts, board, titles), owned by the Administration module.
/// Each module has its own audit with its own definition (a future module defines its own).
/// If writing fails, the action has already been done: the failure is written to the server logs and never thrown.
/// </summary>
public interface IAdministrationAudit
{
    /// <summary>Action done by the signed-in person (the actor comes from the user context).</summary>
    Task RecordAsync(string action, string? targetId, CancellationToken ct);

    /// <summary>Action done by the application itself (automatic deletion): the actor is "Système".</summary>
    Task RecordBySystemAsync(string action, string? targetId, CancellationToken ct);
}
