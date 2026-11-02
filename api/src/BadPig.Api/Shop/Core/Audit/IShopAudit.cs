namespace BadPig.Shop.Core;

/// <summary>
/// Audit of the board's actions on the shop, owned by the Shop module (each module has its own audit).
/// If writing fails, the action has already been done: the failure is written to the server logs and never thrown.
/// </summary>
public interface IShopAudit
{
    /// <summary>Action done by the signed-in person (the actor comes from the user context).</summary>
    Task RecordAsync(string action, Guid? targetId, string? subject, string? details, CancellationToken ct);
}
