using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.RestockVariant;

/// <summary>Adds units to the stock and writes it in the stock history.</summary>
internal sealed class RestockVariantHandler(IValidator<RestockVariantRequest> validator, IDateTimeProvider clock, IUserContext currentUser, IShopAudit audit, AppDbContext db)
    : IHandler<RestockVariantRequest>
{
    public const string AuditAction = "variant.restocked";

    public async Task<Result> HandleAsync(RestockVariantRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var quantity = request.Body.Quantity!.Value;
        var changed = await db.ProductVariants.Where(v => v.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(v => v.Quantity, v => v.Quantity + quantity), ct);
        if (changed == 0)
            return Error.NotFound();

        await db.ProductVariants.ClearIfRecoveredAsync([id], ct);
        db.StockMovements.Add(new StockMovement
        {
            Id = Guid.NewGuid(), VariantId = id, Change = quantity, Reason = StockReasons.Restock, Note = request.Body.Note?.Trim() ?? "",
            AuthorId = Guid.Parse(currentUser.AccountId!), AuthorName = currentUser.DisplayName, CreatedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        var variant = await db.ProductVariants.AsNoTracking().FirstAsync(v => v.Id == id, ct);
        var productName = await db.ShopProducts.Where(p => p.Id == variant.ProductId).Select(p => p.Name).FirstAsync(ct);
        var note = request.Body.Note?.Trim();
        await audit.RecordAsync(AuditAction, id, ShopAuditText.Variant(productName, variant.Label),
            $"+{quantity} (stock : {variant.Quantity})" + (string.IsNullOrEmpty(note) ? "" : $" — {note}"), ct);
        return Result.Success();
    }
}
