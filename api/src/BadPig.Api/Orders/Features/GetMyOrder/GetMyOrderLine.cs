namespace BadPig.Orders.Features.GetMyOrder;

/// <summary>One article of the order, as it was sold.</summary>
/// <param name="ProductName">Name of the article.</param>
/// <param name="VariantLabel">Variant (type, size...).</param>
/// <param name="UnitPriceCents">Price of one unit in euro cents.</param>
/// <param name="Quantity">Units ordered.</param>
public sealed record GetMyOrderLine(string ProductName, string VariantLabel, int UnitPriceCents, int Quantity);
