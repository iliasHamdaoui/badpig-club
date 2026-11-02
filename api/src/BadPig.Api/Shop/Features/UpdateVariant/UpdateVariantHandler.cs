using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.UpdateVariant;

/// <summary>Changes the label, price, alert threshold and visibility of a variant.</summary>
internal sealed class UpdateVariantHandler(IValidator<UpdateVariantRequest> validator, IShopAudit audit, AppDbContext db)
    : IHandler<UpdateVariantRequest>
{
    public const string AuditAction = "variant.updated";

    public async Task<Result> HandleAsync(UpdateVariantRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var variant = await db.ProductVariants.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (variant is null)
            return Error.NotFound();

        var label = request.Body.Label!.Trim();
        var labels = await db.ProductVariants.Where(v => v.ProductId == variant.ProductId && v.Id != id).Select(v => v.Label).ToListAsync(ct);
        if (labels.Any(l => string.Equals(l, label, StringComparison.CurrentCultureIgnoreCase)))
            return Error.Conflict("Cet article a déjà une variante avec ce libellé.");

        var productName = await db.ShopProducts.Where(p => p.Id == variant.ProductId).Select(p => p.Name).FirstAsync(ct);
        var before = (variant.Label, variant.PriceCents, variant.LowStockThreshold, variant.IsActive);
        variant.Label = label;
        variant.PriceCents = request.Body.PriceCents!.Value;
        variant.LowStockThreshold = request.Body.LowStockThreshold!.Value;
        variant.IsActive = request.Body.IsActive!.Value;
        await db.SaveChangesAsync(ct);
        await db.ProductVariants.ClearIfRecoveredAsync([id], ct);
        var details = ShopAuditText.Changes(
            ("variante", before.Label, variant.Label),
            ("prix", ShopAuditText.Euros(before.PriceCents), ShopAuditText.Euros(variant.PriceCents)),
            ("seuil d'alerte", before.LowStockThreshold.ToString(), variant.LowStockThreshold.ToString()),
            ("proposée aux adhérents", before.IsActive ? "oui" : "non", variant.IsActive ? "oui" : "non"));
        await audit.RecordAsync(AuditAction, variant.Id, ShopAuditText.Variant(productName, variant.Label), details, ct);
        return Result.Success();
    }
}
