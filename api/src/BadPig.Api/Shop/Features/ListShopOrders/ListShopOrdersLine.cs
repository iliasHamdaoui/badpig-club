namespace BadPig.Shop.Features.ListShopOrders;

/// <summary>One article of an order, as it was sold.</summary>
/// <param name="ProductName">Name of the article.</param>
/// <param name="VariantLabel">Variant (type, size...).</param>
/// <param name="UnitPriceCents">Price of one unit in euro cents.</param>
/// <param name="Quantity">Units ordered.</param>
public sealed record ListShopOrdersLine(string ProductName, string VariantLabel, int UnitPriceCents, int Quantity);
