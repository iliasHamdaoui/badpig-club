using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Payments;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BadPig.Orders.Core;

/// <summary>
/// The moments that change the stock after an order is made: it is paid (the held units are sold), cancelled or expired (the held units
/// are released). Shared by the features that learn about it (payment notification, the buyer looking at the order, the buyer cancelling,
/// the clean-up of the unpaid orders). Every step is a conditional update of the order first, so two calls at the same time
/// (the notification and the buyer's page) settle an order only once.
/// </summary>
public sealed class OrderSettlement(AppDbContext db, IDateTimeProvider clock, IPaymentProvider payments, IStockKeeper stock, BoardMailer boardMailer, IIdentityProvider identity,
    IEmailSender mail, IOptions<OrderLinks> links, ILogger<OrderSettlement> log)
{
    /// <summary>The order is paid: the held units leave the stock (a sale in the history) and the buyer is told. False if it was no longer waiting for payment.</summary>
    public async Task<bool> MarkPaidAsync(Guid orderId, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var now = clock.UtcNow;
        var changed = await db.Orders.Where(o => o.Id == orderId && o.Status == OrderStatuses.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatuses.Paid).SetProperty(o => o.PaidAt, (DateTimeOffset?)now), ct);
        if (changed == 0)
            return false;

        var lines = await db.OrderLines.AsNoTracking().Where(l => l.OrderId == orderId).ToListAsync(ct);
        await stock.SellAsync(orderId, StockLines(lines), now, ct);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        await NotifyPaidAsync(orderId, lines, ct);
        await stock.AlertLowStockAsync(StockLines(lines), ct);
        return true;
    }

    /// <summary>The order will not be paid: the held units go back to the available stock. False if it was no longer waiting for payment.</summary>
    public async Task<bool> ReleaseAsync(Guid orderId, string newStatus, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var changed = await db.Orders.Where(o => o.Id == orderId && o.Status == OrderStatuses.Pending)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, newStatus), ct);
        if (changed == 0)
            return false;

        var lines = await db.OrderLines.AsNoTracking().Where(l => l.OrderId == orderId).ToListAsync(ct);
        await stock.ReleaseAsync(StockLines(lines), ct);

        await transaction.CommitAsync(ct);
        return true;
    }

    /// <summary>
    /// Settles the orders whose time is over. The payment provider is asked first: a payment made just before the end, whose notification
    /// is late, is not lost. If the provider cannot be asked, the order is left for the next time.
    /// </summary>
    public async Task ReleaseExpiredAsync(CancellationToken ct)
    {
        var now = clock.UtcNow;
        var expired = await db.Orders.AsNoTracking().Where(o => o.Status == OrderStatuses.Pending && o.ExpiresAt <= now)
            .Select(o => new { o.Id, o.PaymentSessionId }).ToListAsync(ct);
        foreach (var order in expired)
        {
            PaymentState? state = PaymentState.Expired;
            if (order.PaymentSessionId is not null)
                state = await payments.GetSessionStateAsync(order.PaymentSessionId, ct);

            if (state is null)
                continue;

            if (state == PaymentState.Paid)
                await MarkPaidAsync(order.Id, ct);
            else
                await ReleaseAsync(order.Id, OrderStatuses.Expired, ct);
        }
    }

    // The lines whose variant still exists (an order line keeps no link to a deleted variant).
    private static List<StockLine> StockLines(IEnumerable<OrderLine> lines) =>
        lines.Where(l => l.VariantId is not null).Select(l => new StockLine(l.VariantId!.Value, l.Quantity)).ToList();

    // A failure to send an e-mail never cancels the payment, and the buyer's e-mail does not depend on the board's.
    private async Task NotifyPaidAsync(Guid orderId, List<OrderLine> lines, CancellationToken ct)
    {
        Order order;
        try
        {
            order = await db.Orders.AsNoTracking().FirstAsync(o => o.Id == orderId, ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            log.LogWarning(e, "The payment e-mails of order {OrderId} could not be prepared", orderId);
            return;
        }

        await TryAsync(() => NotifyBuyerAsync(order, lines, ct), orderId);
        await TryAsync(() => NotifyBoardAsync(order, lines, ct), orderId);
    }

    private async Task NotifyBuyerAsync(Order order, List<OrderLine> lines, CancellationToken ct)
    {
        if (order.BuyerId is not { } buyerId)
            return;

        var buyer = await identity.FindAsync(buyerId.ToString(), ct);
        if (buyer is not { IsActive: true })
            return;

        await mail.SendAsync(buyer, OrderNotifications.PaidSubject, OrderNotifications.PaidBody(lines, order.TotalCents, links.Value.OrderPage(order.Id)), ct);
    }

    // The board is told of every paid order.
    private Task NotifyBoardAsync(Order order, List<OrderLine> lines, CancellationToken ct) =>
        boardMailer.SendAsync(links.Value.NotificationEmail, OrderNotifications.BoardSubject(order.BuyerName),
            OrderNotifications.BoardBody(order.BuyerName, lines, order.TotalCents), ct);

    private async Task TryAsync(Func<Task> send, Guid orderId)
    {
        try
        {
            await send();
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            log.LogWarning(e, "A payment e-mail of order {OrderId} could not be sent", orderId);
        }
    }
}
