using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Infrastructure;

internal sealed class ShopCatalog(AppDbContext db) : IShopCatalog
{
    public async Task<PagedResponse<OfferedProduct>> ListOfferedAsync(ListQuery query, CancellationToken ct)
    {
        var products = db.ShopProducts.AsNoTracking().Include(p => p.Variants).Where(p => p.IsActive && p.Variants.Any(v => v.IsActive));
        if (query.SearchTerm() is { } term)
            products = products.Where(p => p.Name.ToLower().Contains(term) || p.Description.ToLower().Contains(term));

        var ordered = query.IsDescending() ? products.OrderByDescending(p => p.Name) : products.OrderBy(p => p.Name);
        var page = await ordered.ThenBy(p => p.Id).ToPageAsync(query, ct);
        var ids = page.Items.Select(p => p.Id).ToList();
        var versions = (await db.ProductImages.AsNoTracking().Where(i => ids.Contains(i.ProductId)).Select(i => new { i.ProductId, i.UpdatedAt }).ToListAsync(ct))
            .ToDictionary(i => i.ProductId, i => i.UpdatedAt.ToUnixTimeMilliseconds());
        return page.Map(p => new OfferedProduct(p.Id, p.Name, p.Description, versions.TryGetValue(p.Id, out var version) ? version : null,
            p.Variants.Where(v => v.IsActive).OrderBy(v => v.CreatedAt).ThenBy(v => v.Label, StringComparer.CurrentCultureIgnoreCase).ThenBy(v => v.Id)
                .Select(v => new OfferedVariant(v.Id, v.Label, v.PriceCents, v.Available)).ToList()));
    }

    public async Task<ProductImageFile?> GetImageAsync(Guid productId, bool includeHidden, CancellationToken ct)
    {
        var image = await db.ProductImages.AsNoTracking()
            .Where(i => i.ProductId == productId && (includeHidden || db.ShopProducts.Any(p => p.Id == productId && p.IsActive)))
            .FirstOrDefaultAsync(ct);
        return image is null ? null : new ProductImageFile(image.ContentType, image.Content);
    }

    public async Task<IReadOnlyDictionary<Guid, CatalogVariant>> FindVariantsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        var variants = await db.ProductVariants.AsNoTracking().Where(v => ids.Contains(v.Id)).ToListAsync(ct);
        var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
        var products = await db.ShopProducts.AsNoTracking().Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);
        return variants.ToDictionary(v => v.Id, v => new CatalogVariant(v.Id, products[v.ProductId].Name, v.Label, v.PriceCents, v.IsActive && products[v.ProductId].IsActive));
    }
}
