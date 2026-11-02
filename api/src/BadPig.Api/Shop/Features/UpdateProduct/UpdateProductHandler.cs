using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.UpdateProduct;

/// <summary>Changes the name, description and visibility of an article.</summary>
internal sealed class UpdateProductHandler(IValidator<UpdateProductRequest> validator, IShopAudit audit, AppDbContext db)
    : IHandler<UpdateProductRequest>
{
    public const string AuditAction = "product.updated";

    public async Task<Result> HandleAsync(UpdateProductRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var product = await db.ShopProducts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return Error.NotFound();

        var before = (product.Name, product.Description, product.IsActive);
        product.Name = request.Body.Name!.Trim();
        product.Description = request.Body.Description?.Trim() ?? "";
        product.IsActive = request.Body.IsActive!.Value;
        await db.SaveChangesAsync(ct);
        var details = ShopAuditText.Changes(
            ("nom", before.Name, product.Name),
            ("description", before.Description, product.Description),
            ("proposé aux adhérents", before.IsActive ? "oui" : "non", product.IsActive ? "oui" : "non"));
        await audit.RecordAsync(AuditAction, product.Id, product.Name, details, ct);
        return Result.Success();
    }
}
