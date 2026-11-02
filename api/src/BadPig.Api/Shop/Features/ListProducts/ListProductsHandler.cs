using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.ListProducts;

/// <summary>The articles with their variants and stock, for the board.</summary>
internal sealed class ListProductsHandler(IValidator<ListProductsRequest> validator, AppDbContext db)
    : IHandler<ListProductsRequest, PagedResponse<ListProductsItem>>
{
    public async Task<Result<PagedResponse<ListProductsItem>>> HandleAsync(ListProductsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var query = db.ShopProducts.AsNoTracking().Include(p => p.Variants).AsQueryable();
        if (request.SearchTerm() is { } term)
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.Variants.Any(v => v.Label.ToLower().Contains(term)));

        if (request.LowStock == true)
            query = query.Where(p => p.Variants.Any(v => v.LowStockThreshold > 0 && v.Quantity - v.ReservedQuantity <= v.LowStockThreshold));

        var ordered = request.Sort?.ToLowerInvariant() == "createdat"
            ? (request.IsDescending() ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt))
            : (request.IsDescending() ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name));
        var page = await ordered.ThenBy(p => p.Id).ToPageAsync(request, ct);
        var ids = page.Items.Select(p => p.Id).ToList();
        var versions = (await db.ProductImages.AsNoTracking().Where(i => ids.Contains(i.ProductId)).Select(i => new { i.ProductId, i.UpdatedAt }).ToListAsync(ct))
            .ToDictionary(i => i.ProductId, i => i.UpdatedAt.ToUnixTimeMilliseconds());
        return page.Map(p => ToItem(p, versions.TryGetValue(p.Id, out var version) ? version : null));
    }

    private static ListProductsItem ToItem(Product product, long? imageVersion)
    {
        var variants = product.Variants.OrderBy(v => v.CreatedAt).ThenBy(v => v.Label, StringComparer.CurrentCultureIgnoreCase).ThenBy(v => v.Id)
            .Select(v => new ListProductsVariant(v.Id.ToString(), v.Label, v.PriceCents, v.Quantity, v.ReservedQuantity, v.Available,
                v.LowStockThreshold, v.IsLowStock, v.IsActive))
            .ToList();
        return new ListProductsItem(product.Id.ToString(), product.Name, product.Description, product.IsActive,
            variants.Any(v => v.IsLowStock), product.CreatedAt, imageVersion, variants);
    }
}
