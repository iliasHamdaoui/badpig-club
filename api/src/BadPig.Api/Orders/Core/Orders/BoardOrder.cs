namespace BadPig.Orders.Core;

/// <summary>An order as the board reads it.</summary>
public sealed record BoardOrder(
    Guid Id, string BuyerName, string Status, int TotalCents, DateTimeOffset CreatedAt, DateTimeOffset? PaidAt, DateTimeOffset? DeliveredAt, string? DeliveredByName,
    IReadOnlyList<BoardOrderLine> Lines);

/// <summary>One article of an order, as it was sold.</summary>
public sealed record BoardOrderLine(string ProductName, string VariantLabel, int UnitPriceCents, int Quantity);
