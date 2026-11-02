namespace BadPig.Shop.Core;

/// <summary>A variant as the orders need to know it: what it is called, its price and whether it can be ordered.</summary>
/// <param name="Id">Variant identifier.</param>
/// <param name="ProductName">Name of its article.</param>
/// <param name="Label">Label of the variant.</param>
/// <param name="PriceCents">Price of one unit, in euro cents.</param>
/// <param name="IsOffered">False when the article or the variant is hidden by the board.</param>
public sealed record CatalogVariant(Guid Id, string ProductName, string Label, int PriceCents, bool IsOffered);
