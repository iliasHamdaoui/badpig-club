namespace BadPig.Shop.Features.ListProducts;

/// <summary>One variant of an article, with its stock.</summary>
/// <param name="Id">Variant identifier.</param>
/// <param name="Label">Label, e.g. "Vitesse 77" or "M - Homme".</param>
/// <param name="PriceCents">Price of one unit, in euro cents.</param>
/// <param name="Quantity">Units in stock (the reserved ones included).</param>
/// <param name="Reserved">Units held for orders waiting for their payment.</param>
/// <param name="Available">Units a member can still order (quantity minus reserved).</param>
/// <param name="LowStockThreshold">Alert threshold (0 = no alert).</param>
/// <param name="IsLowStock">True when available is at or under a threshold above 0.</param>
/// <param name="IsActive">False = hidden from the members.</param>
public sealed record ListProductsVariant(
    string Id, string Label, int PriceCents, int Quantity, int Reserved, int Available, int LowStockThreshold, bool IsLowStock, bool IsActive);
