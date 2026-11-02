using BadPig.Shared.Core;

namespace BadPig.Orders.Features.ListMyOrders;

/// <summary>My orders: filter on the status, sort (createdAt, totalCents; default createdAt desc) and pagination.</summary>

public sealed record ListMyOrdersRequest : ListQuery
{
    /// <summary>pending, paid, delivered, cancelled or expired; empty = all.</summary>
    public string? Status { get; init; }
}
