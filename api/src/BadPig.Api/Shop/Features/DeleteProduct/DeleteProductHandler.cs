using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.DeleteProduct;

/// <summary>Deletes an article and its variants, unless unpaid orders hold some of its stock.</summary>
internal sealed class DeleteProductHandler(IValidator<DeleteProductRequest> validator, IShopAudit audit, AppDbContext db)
    : IHandler<DeleteProductRequest>
{
    public const string AuditAction = "product.deleted";

    public async Task<Result> HandleAsync(DeleteProductRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var product = await db.ShopProducts.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return Error.NotFound();

        if (product.Variants.Any(v => v.ReservedQuantity > 0))
            return Error.Conflict("Des commandes en attente de paiement réservent cet article.");

        db.ShopProducts.Remove(product);
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync(AuditAction, product.Id, product.Name, null, ct);
        return Result.Success();
    }
}
