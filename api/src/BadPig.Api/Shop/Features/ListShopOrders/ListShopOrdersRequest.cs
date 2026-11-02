using BadPig.Shared.Core;

namespace BadPig.Shop.Features.ListShopOrders;

/// <summary>Orders: search (buyer name), filter on the status, sort (createdAt, totalCents, buyerName; default createdAt desc) and pagination.</summary>

public sealed record ListShopOrdersRequest : ListQuery
{
    /// <summary>pending, paid, delivered, cancelled or expired; empty = all.</summary>
    public string? Status { get; init; }
}
