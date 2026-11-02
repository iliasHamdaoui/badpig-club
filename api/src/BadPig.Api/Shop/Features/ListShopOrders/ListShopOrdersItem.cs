namespace BadPig.Shop.Features.ListShopOrders;

/// <summary>One order.</summary>
/// <param name="Id">Order identifier.</param>
/// <param name="BuyerName">Name of the buyer when the order was made ("Compte supprimé" once the account is deleted).</param>
/// <param name="Status">pending (waiting for the payment), paid (to hand over), delivered, cancelled or expired.</param>
/// <param name="TotalCents">Total in euro cents.</param>
/// <param name="CreatedAt">Date of the order.</param>
/// <param name="PaidAt">Date of the payment; null if not paid.</param>
/// <param name="DeliveredAt">Date of the handing over; null if not delivered.</param>
/// <param name="DeliveredByName">Name of the board member who handed the order over; null if not delivered ("Ancien membre du bureau" once their account is deleted).</param>
/// <param name="Lines">The articles ordered.</param>
public sealed record ListShopOrdersItem(
    string Id, string BuyerName, string Status, int TotalCents, DateTimeOffset CreatedAt, DateTimeOffset? PaidAt, DateTimeOffset? DeliveredAt, string? DeliveredByName,
    IReadOnlyList<ListShopOrdersLine> Lines);
