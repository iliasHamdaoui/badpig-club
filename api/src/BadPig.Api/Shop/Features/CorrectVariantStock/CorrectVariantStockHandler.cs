using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.CorrectVariantStock;

/// <summary>Sets the quantity in stock after a count and writes the difference in the stock history.</summary>
internal sealed class CorrectVariantStockHandler(IValidator<CorrectVariantStockRequest> validator, IDateTimeProvider clock, IUserContext currentUser, IShopAudit audit, AppDbContext db)
    : IHandler<CorrectVariantStockRequest>
{
    public const string AuditAction = "variant.stock_corrected";

    public async Task<Result> HandleAsync(CorrectVariantStockRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var variant = await db.ProductVariants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
        if (variant is null)
            return Error.NotFound();

        var counted = request.Body.Quantity!.Value;
        if (counted < variant.ReservedQuantity)
            return Error.Conflict("Le stock ne peut pas être inférieur aux unités réservées par des commandes en attente.");

        var difference = counted - variant.Quantity;
        if (difference == 0)
            return Result.Success();

        // Conditional update: refused if a sale or another correction changed the stock since it was read.
        var previous = variant.Quantity;
        var changed = await db.ProductVariants.Where(v => v.Id == id && v.Quantity == previous && v.ReservedQuantity <= counted)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.Quantity, counted), ct);
        if (changed == 0)
            return Error.Conflict("Le stock vient de changer, réessayez.");

        await db.ProductVariants.ClearIfRecoveredAsync([id], ct);
        db.StockMovements.Add(new StockMovement
        {
            Id = Guid.NewGuid(), VariantId = id, Change = difference, Reason = StockReasons.Correction, Note = request.Body.Note!.Trim(),
            AuthorId = Guid.Parse(currentUser.AccountId!), AuthorName = currentUser.DisplayName, CreatedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        var productName = await db.ShopProducts.Where(p => p.Id == variant.ProductId).Select(p => p.Name).FirstAsync(ct);
        await audit.RecordAsync(AuditAction, id, ShopAuditText.Variant(productName, variant.Label),
            $"{previous} → {counted} ({(difference > 0 ? "+" : "")}{difference}) — {request.Body.Note!.Trim()}", ct);
        return Result.Success();
    }
}
