namespace BadPig.Orders.Features.GetMyOrder;

/// <summary>One of my orders.</summary>
/// <param name="Id">Order identifier.</param>
/// <param name="Status">pending (waiting for the payment), paid (to be handed over by the board), delivered, cancelled or expired.</param>
/// <param name="TotalCents">Total in euro cents.</param>
/// <param name="CreatedAt">Date of the order.</param>
/// <param name="ExpiresAt">End of the reservation: unpaid by then, the order expires.</param>
/// <param name="PaidAt">Date of the payment; null if not paid.</param>
/// <param name="DeliveredAt">Date of the handing over; null if not delivered.</param>
/// <param name="CheckoutUrl">Address of the payment page; only while the order is pending, otherwise null.</param>
/// <param name="Lines">The articles ordered.</param>
public sealed record GetMyOrderResponse(
    string Id, string Status, int TotalCents, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, DateTimeOffset? PaidAt, DateTimeOffset? DeliveredAt,
    string? CheckoutUrl, IReadOnlyList<GetMyOrderLine> Lines);
