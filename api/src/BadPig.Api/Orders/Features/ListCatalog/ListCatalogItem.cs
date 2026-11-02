namespace BadPig.Orders.Features.ListCatalog;

/// <summary>One article for sale.</summary>
/// <param name="Id">Article identifier.</param>
/// <param name="Name">Name.</param>
/// <param name="Description">Description.</param>
/// <param name="ImageVersion">Version of the picture of the article (its address is <c>/shop/products/{id}/image?v=version</c>); null without a picture.</param>
/// <param name="Variants">The ways to buy it.</param>
public sealed record ListCatalogItem(string Id, string Name, string Description, long? ImageVersion, IReadOnlyList<ListCatalogVariant> Variants);
