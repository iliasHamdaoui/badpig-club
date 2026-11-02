using BadPig.Orders.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Orders.Infrastructure;

internal sealed class OrderBook(AppDbContext db, IDateTimeProvider clock, IUserContext currentUser, OrderSettlement settlement) : IOrderBook
{
    public async Task<PagedResponse<BoardOrder>> ListAsync(string? status, ListQuery query, CancellationToken ct)
    {
        await settlement.ReleaseExpiredAsync(ct);

        var orders = db.Orders.AsNoTracking().Include(o => o.Lines).AsQueryable();
        if (!string.IsNullOrEmpty(status))
            orders = orders.Where(o => o.Status == status);

        if (query.SearchTerm() is { } term)
            orders = orders.Where(o => o.BuyerName.ToLower().Contains(term));

        var descending = query.IsDescending(defaultDescending: string.IsNullOrEmpty(query.Sort));
        var ordered = query.Sort?.ToLowerInvariant() switch
        {
            "totalcents" => descending ? orders.OrderByDescending(o => o.TotalCents) : orders.OrderBy(o => o.TotalCents),
            "buyername" => descending ? orders.OrderByDescending(o => o.BuyerName) : orders.OrderBy(o => o.BuyerName),
            _ => descending ? orders.OrderByDescending(o => o.CreatedAt) : orders.OrderBy(o => o.CreatedAt),
        };
        var page = await ordered.ThenBy(o => o.Id).ToPageAsync(query, ct);
        return page.Map(o => new BoardOrder(o.Id, o.BuyerName, o.Status, o.TotalCents, o.CreatedAt, o.PaidAt, o.DeliveredAt, o.DeliveredByName,
            o.Lines.OrderBy(l => l.ProductName).ThenBy(l => l.VariantLabel)
                .Select(l => new BoardOrderLine(l.ProductName, l.VariantLabel, l.UnitPriceCents, l.Quantity)).ToList()));
    }

    public async Task<Result> MarkDeliveredAsync(Guid orderId, CancellationToken ct)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
            return Error.NotFound();

        if (!OrderRules.IsToHandOver(order))
            return Error.Conflict("Seule une commande payée peut être remise.");

        var now = clock.UtcNow;
        var changed = await db.Orders.Where(o => o.Id == orderId && o.Status == OrderStatuses.Paid)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatuses.Delivered).SetProperty(o => o.DeliveredAt, (DateTimeOffset?)now)
                .SetProperty(o => o.DeliveredById, Guid.TryParse(currentUser.AccountId, out var accountId) ? accountId : (Guid?)null)
                .SetProperty(o => o.DeliveredByName, currentUser.DisplayName), ct);
        if (changed == 0)
            return Error.Conflict("Seule une commande payée peut être remise.");

        return Result.Success();
    }
}
