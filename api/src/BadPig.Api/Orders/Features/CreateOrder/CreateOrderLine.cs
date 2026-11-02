namespace BadPig.Orders.Features.CreateOrder;

/// <summary>One article of the order.</summary>
/// <param name="VariantId">Identifier of the variant, from the catalogue.</param>
/// <param name="Quantity">Units (1 to 10).</param>
public sealed record CreateOrderLine(string? VariantId, int? Quantity);
