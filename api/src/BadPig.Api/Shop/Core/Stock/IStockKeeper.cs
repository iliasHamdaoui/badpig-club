namespace BadPig.Shop.Core;

/// <summary>
/// The stock operations the orders need. The stock is never changed by reading then writing but by conditional or relative updates,
/// so two orders at the same time can never sell the same unit. These calls use the current database transaction and the movements
/// they write are saved by the caller's <c>SaveChangesAsync</c>.
/// </summary>
public interface IStockKeeper
{
    /// <summary>Holds the units for an order waiting for its payment. Returns the first variant that cannot be held (not offered or not enough available), or null when all are held; then the caller must undo the transaction.</summary>
    Task<Guid?> HoldAsync(IReadOnlyList<StockLine> lines, CancellationToken ct);

    /// <summary>The order will not be paid: the held units become available again.</summary>
    Task ReleaseAsync(IReadOnlyList<StockLine> lines, CancellationToken ct);

    /// <summary>
    /// After a sale (once the sale is saved): e-mails the board, once, for each of these variants that is now at or under its alert threshold.
    /// It writes nothing more for a variant already reported, until its stock is back above the threshold. A failure to send is only logged.
    /// </summary>
    Task AlertLowStockAsync(IReadOnlyList<StockLine> lines, CancellationToken ct);

    /// <summary>The order is paid: the held units leave the stock, with a sale in the stock history.</summary>
    Task SellAsync(Guid orderId, IReadOnlyList<StockLine> lines, DateTimeOffset at, CancellationToken ct);
}
