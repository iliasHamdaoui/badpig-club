using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BadPig.Shop.Infrastructure;

internal sealed class StockKeeper(AppDbContext db, BoardMailer boardMailer, IOptions<ShopOptions> options, ILogger<StockKeeper> log) : IStockKeeper
{
    public async Task<Guid?> HoldAsync(IReadOnlyList<StockLine> lines, CancellationToken ct)
    {
        foreach (var line in lines)
        {
            var variantId = line.VariantId;
            var quantity = line.Quantity;
            var held = await db.ProductVariants
                .Where(v => v.Id == variantId && v.IsActive && v.Quantity - v.ReservedQuantity >= quantity)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.ReservedQuantity, v => v.ReservedQuantity + quantity), ct);
            if (held == 0)
                return variantId;
        }

        return null;
    }

    public async Task ReleaseAsync(IReadOnlyList<StockLine> lines, CancellationToken ct)
    {
        foreach (var line in lines)
        {
            var variantId = line.VariantId;
            var quantity = line.Quantity;
            await db.ProductVariants.Where(v => v.Id == variantId)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.ReservedQuantity, v => v.ReservedQuantity - quantity), ct);
        }

        // Units available again: a variant that was low may not be any more.
        await db.ProductVariants.ClearIfRecoveredAsync(lines.Select(l => l.VariantId).ToList(), ct);
    }

    public async Task AlertLowStockAsync(IReadOnlyList<StockLine> lines, CancellationToken ct)
    {
        try
        {
            var ids = lines.Select(l => l.VariantId).Distinct().ToList();
            await db.ProductVariants.ClearIfRecoveredAsync(ids, ct);
            var low = await db.ProductVariants.AsNoTracking()
                .Where(v => ids.Contains(v.Id) && v.LowStockThreshold > 0 && !v.LowStockAlerted && v.Quantity - v.ReservedQuantity <= v.LowStockThreshold)
                .ToListAsync(ct);
            foreach (var variant in low)
                await AlertAsync(variant, ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            log.LogWarning(e, "The low stock alerts could not be sent");
        }
    }

    // The conditional update marks the variant once, even if two payments are settled at the same time: only the one that marks it sends the e-mail.
    private async Task AlertAsync(ProductVariant variant, CancellationToken ct)
    {
        var id = variant.Id;
        var marked = await db.ProductVariants.Where(v => v.Id == id && !v.LowStockAlerted)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.LowStockAlerted, true), ct);
        if (marked == 0)
            return;

        var productName = await db.ShopProducts.AsNoTracking().Where(p => p.Id == variant.ProductId).Select(p => p.Name).FirstAsync(ct);
        await boardMailer.SendAsync(options.Value.NotificationEmail, LowStockNotifications.Subject(productName, variant.Label),
            LowStockNotifications.Body(productName, variant.Label, variant.Available, variant.LowStockThreshold), ct);
    }

    public async Task SellAsync(Guid orderId, IReadOnlyList<StockLine> lines, DateTimeOffset at, CancellationToken ct)
    {
        foreach (var line in lines)
        {
            var variantId = line.VariantId;
            var quantity = line.Quantity;
            var sold = await db.ProductVariants.Where(v => v.Id == variantId)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Quantity, v => v.Quantity - quantity)
                    .SetProperty(v => v.ReservedQuantity, v => v.ReservedQuantity - quantity), ct);
            if (sold == 0)
                continue; // the variant was deleted since the order was made: nothing to write in its history

            db.StockMovements.Add(new StockMovement
            {
                Id = Guid.NewGuid(), VariantId = variantId, Change = -quantity, Reason = StockReasons.Sale, OrderId = orderId, CreatedAt = at,
            });
        }
    }
}
