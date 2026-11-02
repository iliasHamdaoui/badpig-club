using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.DeleteVariant;

/// <summary>Deletes a variant, unless unpaid orders hold some of its stock.</summary>
internal sealed class DeleteVariantHandler(IValidator<DeleteVariantRequest> validator, IShopAudit audit, AppDbContext db)
    : IHandler<DeleteVariantRequest>
{
    public const string AuditAction = "variant.deleted";

    public async Task<Result> HandleAsync(DeleteVariantRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var variant = await db.ProductVariants.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (variant is null)
            return Error.NotFound();

        if (variant.ReservedQuantity > 0)
            return Error.Conflict("Des commandes en attente de paiement réservent cette variante.");

        var productName = await db.ShopProducts.Where(p => p.Id == variant.ProductId).Select(p => p.Name).FirstAsync(ct);
        db.ProductVariants.Remove(variant);
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync(AuditAction, variant.Id, ShopAuditText.Variant(productName, variant.Label), $"stock restant {variant.Quantity}", ct);
        return Result.Success();
    }
}
