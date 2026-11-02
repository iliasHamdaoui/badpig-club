using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using Microsoft.Extensions.Logging;

namespace BadPig.Shop.Infrastructure;

internal sealed class ShopAuditService(IDateTimeProvider clock, AppDbContext db, IUserContext user, ILogger<ShopAuditService> log) : IShopAudit
{
    public async Task RecordAsync(string action, Guid? targetId, string? subject, string? details, CancellationToken ct)
    {
        try
        {
            if (!Guid.TryParse(user.AccountId, out var actor))
                throw new InvalidOperationException("Invalid actor identifier");

            db.ShopAuditLog.Add(new ShopAuditEntry
            {
                OccurredAt = clock.UtcNow, ActorId = actor, ActorName = user.DisplayName, Action = action,
                TargetId = targetId, Subject = subject, Details = details,
            });
            await db.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            db.ChangeTracker.Clear(); // a failed entry must not be retried with the next one
            log.LogError(e, "SHOP AUDIT LOG UNAVAILABLE - action {Action} by {Actor} on {Target}", action, user.AccountId, targetId);
        }
    }
}
