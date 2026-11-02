using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.CreateVariant;

/// <summary>Adds a variant to an article; the initial stock is written in the stock history.</summary>
internal sealed class CreateVariantHandler(IValidator<CreateVariantRequest> validator, IDateTimeProvider clock, IUserContext currentUser, IShopAudit audit, AppDbContext db)
    : IHandler<CreateVariantRequest, CreatedResponse>
{
    public const string AuditAction = "variant.created";

    public async Task<Result<CreatedResponse>> HandleAsync(CreateVariantRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var productId = Guid.Parse(request.Id);
        var productName = await db.ShopProducts.Where(p => p.Id == productId).Select(p => p.Name).FirstOrDefaultAsync(ct);
        if (productName is null)
            return Error.NotFound();

        var label = request.Body.Label!.Trim();
        var labels = await db.ProductVariants.Where(v => v.ProductId == productId).Select(v => v.Label).ToListAsync(ct);
        if (labels.Any(l => string.Equals(l, label, StringComparison.CurrentCultureIgnoreCase)))
            return Error.Conflict("Cet article a déjà une variante avec ce libellé.");

        var now = clock.UtcNow;
        var quantity = request.Body.Quantity ?? 0;
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Label = label,
            PriceCents = request.Body.PriceCents!.Value,
            Quantity = quantity,
            LowStockThreshold = request.Body.LowStockThreshold ?? 0,
            CreatedAt = now,
        };
        db.ProductVariants.Add(variant);
        if (quantity > 0)
        {
            db.StockMovements.Add(new StockMovement
            {
                Id = Guid.NewGuid(), VariantId = variant.Id, Change = quantity, Reason = StockReasons.Restock, Note = "Stock initial",
                AuthorId = Guid.Parse(currentUser.AccountId!), AuthorName = currentUser.DisplayName, CreatedAt = now,
            });
        }

        await db.SaveChangesAsync(ct);
        await audit.RecordAsync(AuditAction, variant.Id, ShopAuditText.Variant(productName, label),
            $"prix {ShopAuditText.Euros(variant.PriceCents)}, stock initial {quantity}, seuil d'alerte {variant.LowStockThreshold}", ct);
        return new CreatedResponse(variant.Id.ToString());
    }
}
