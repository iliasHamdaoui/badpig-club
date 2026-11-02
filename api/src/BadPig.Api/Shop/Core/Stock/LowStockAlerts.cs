using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Core;

/// <summary>Keeps the "the board was warned" mark of a variant right: it is cleared as soon as the stock is above the threshold again.</summary>
public static class LowStockAlerts
{
    /// <summary>Clears the mark if the variant is no longer low (restock, correction, released order, new threshold). Nothing happens while it is still low.</summary>
    public static Task ClearIfRecoveredAsync(this IQueryable<ProductVariant> variants, IReadOnlyCollection<Guid> ids, CancellationToken ct) =>
        variants.Where(v => ids.Contains(v.Id) && v.LowStockAlerted && (v.LowStockThreshold == 0 || v.Quantity - v.ReservedQuantity > v.LowStockThreshold))
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.LowStockAlerted, false), ct);
}
