namespace BadPig.Shop.Features.ListProducts;

/// <summary>One article of the shop.</summary>
/// <param name="Id">Article identifier.</param>
/// <param name="Name">Name.</param>
/// <param name="Description">Description.</param>
/// <param name="IsActive">False = hidden from the members.</param>
/// <param name="HasLowStock">True when a variant is at or under its alert threshold.</param>
/// <param name="CreatedAt">Creation date.</param>
/// <param name="ImageVersion">Version of the picture of the article (its address is <c>/shop/products/{id}/image?v=version</c>); null without a picture.</param>
/// <param name="Variants">Its variants.</param>
public sealed record ListProductsItem(
    string Id, string Name, string Description, bool IsActive, bool HasLowStock, DateTimeOffset CreatedAt, long? ImageVersion,
    IReadOnlyList<ListProductsVariant> Variants);
