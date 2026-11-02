using BadPig.Administration.Core;
using BadPig.Shared.Core;
using Microsoft.Extensions.Logging;
using BadPig.Shared.Persistence;

namespace BadPig.Administration.Infrastructure;

internal sealed class AdministrationAuditService(IDateTimeProvider clock, AppDbContext db, IUserContext user, ILogger<AdministrationAuditService> log) : IAdministrationAudit
{
    public Task RecordAsync(string action, string? targetId, CancellationToken ct) =>
        WriteAsync(user.AccountId, user.DisplayName, action, targetId, ct);

    public Task RecordBySystemAsync(string action, string? targetId, CancellationToken ct) =>
        WriteAsync(Guid.Empty.ToString(), "Système", action, targetId, ct);

    private async Task WriteAsync(string? actorId, string actorName, string action, string? targetId, CancellationToken ct)
    {
        try
        {
            if (!Guid.TryParse(actorId, out var actor))
                throw new InvalidOperationException("Invalid actor identifier");

            db.AdministrationAuditLog.Add(new AdministrationAuditEntry
            {
                OccurredAt = clock.UtcNow,
                ActorId = actor,
                ActorName = actorName,
                Action = action,
                TargetId = Guid.TryParse(targetId, out var target) ? target : null,
            });
            await db.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            db.ChangeTracker.Clear(); // a failed entry must not be retried with the next one
            log.LogError(e, "AUDIT LOG UNAVAILABLE - action {Action} by {Actor} on {Target}", action, actorId, targetId);
        }
    }
}
