using BadPig.Shared.Core;

namespace BadPig.Shop.Features.ListProducts;

/// <summary>Articles: search (names), filter on low stock, sort (name, createdAt; default name) and pagination.</summary>

public sealed record ListProductsRequest : ListQuery
{
    /// <summary>True = only the articles with a variant at or under its alert threshold.</summary>
    public bool? LowStock { get; init; }
}
