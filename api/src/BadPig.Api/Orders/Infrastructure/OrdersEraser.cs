using BadPig.Orders.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Orders.Infrastructure;

/// <summary>
/// Erases what the orders know about a deleted account. The orders paid are the club's accounting records, so they stay but become anonymous;
/// an order waiting for its payment is cancelled and its stock released.
/// </summary>
internal sealed class OrdersEraser(AppDbContext db, OrderSettlement settlement) : IAccountErasure
{
    public const string DeletedAccountName = "Compte supprimé";
    public const string FormerBoardMemberName = "Ancien membre du bureau";

    public async Task EraseAsync(string accountId, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        var pending = await db.Orders.AsNoTracking().Where(o => o.BuyerId == id && o.Status == OrderStatuses.Pending).Select(o => o.Id).ToListAsync(ct);
        foreach (var orderId in pending)
            await settlement.ReleaseAsync(orderId, OrderStatuses.Cancelled, ct);

        await db.Orders.Where(o => o.BuyerId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.BuyerId, (Guid?)null).SetProperty(o => o.BuyerName, DeletedAccountName)
                .SetProperty(o => o.CheckoutUrl, (string?)null), ct);

        // A board member who handed orders over: the orders keep the date, not the name.
        await db.Orders.Where(o => o.DeliveredById == id)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.DeliveredById, (Guid?)null).SetProperty(o => o.DeliveredByName, FormerBoardMemberName), ct);
    }
}
