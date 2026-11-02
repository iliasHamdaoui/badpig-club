using BadPig.Shared.Core;

namespace BadPig.Shop.Core;

/// <summary>What the orders may read of the shop: the offered articles and the variants being ordered. The shop owns its tables.</summary>
public interface IShopCatalog
{
    /// <summary>The offered articles (offered article with at least one offered variant), sorted by name, searched on name and description.</summary>
    Task<PagedResponse<OfferedProduct>> ListOfferedAsync(ListQuery query, CancellationToken ct);

    /// <summary>The picture of an article, or null if there is none. A hidden article has no picture for the members: <paramref name="includeHidden"/> is for the board.</summary>
    Task<ProductImageFile?> GetImageAsync(Guid productId, bool includeHidden, CancellationToken ct);

    /// <summary>The variants with these identifiers, offered or not; the unknown identifiers are missing from the result.</summary>
    Task<IReadOnlyDictionary<Guid, CatalogVariant>> FindVariantsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct);
}
