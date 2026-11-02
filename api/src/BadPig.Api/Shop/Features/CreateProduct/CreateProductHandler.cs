using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.CreateProduct;

/// <summary>Creates an article, active, without variants.</summary>
internal sealed class CreateProductHandler(IValidator<CreateProductRequest> validator, IDateTimeProvider clock, IShopAudit audit, AppDbContext db)
    : IHandler<CreateProductRequest, CreatedResponse>
{
    public const string AuditAction = "product.created";

    public async Task<Result<CreatedResponse>> HandleAsync(CreateProductRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name!.Trim(),
            Description = request.Description?.Trim() ?? "",
            CreatedAt = clock.UtcNow,
        };
        db.ShopProducts.Add(product);
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync(AuditAction, product.Id, product.Name, null, ct);
        return new CreatedResponse(product.Id.ToString());
    }
}
