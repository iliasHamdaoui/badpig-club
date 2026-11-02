using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.RemoveProductImage;

/// <summary>Removes the picture of an article (nothing to do if it has none).</summary>
internal sealed class RemoveProductImageHandler(IValidator<RemoveProductImageRequest> validator, IShopAudit audit, AppDbContext db)
    : IHandler<RemoveProductImageRequest>
{
    public const string AuditAction = "product.image_removed";

    public async Task<Result> HandleAsync(RemoveProductImageRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var product = await db.ShopProducts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return Error.NotFound();

        var removed = await db.ProductImages.Where(i => i.ProductId == id).ExecuteDeleteAsync(ct);
        if (removed > 0)
            await audit.RecordAsync(AuditAction, id, product.Name, null, ct);

        return Result.Success();
    }
}
