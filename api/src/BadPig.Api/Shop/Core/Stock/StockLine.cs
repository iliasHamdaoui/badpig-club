namespace BadPig.Shop.Core;

/// <summary>Some units of one variant, for the stock operations of the orders.</summary>
public sealed record StockLine(Guid VariantId, int Quantity);
