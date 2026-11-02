namespace BadPig.Orders.Features.ListMyOrders;

/// <summary>One of my orders.</summary>
/// <param name="Id">Order identifier.</param>
/// <param name="Status">pending (waiting for the payment), paid (to be handed over by the board), delivered, cancelled or expired.</param>
/// <param name="TotalCents">Total in euro cents.</param>
/// <param name="CreatedAt">Date of the order.</param>
/// <param name="ItemCount">Number of units ordered.</param>
public sealed record ListMyOrdersItem(string Id, string Status, int TotalCents, DateTimeOffset CreatedAt, int ItemCount);
