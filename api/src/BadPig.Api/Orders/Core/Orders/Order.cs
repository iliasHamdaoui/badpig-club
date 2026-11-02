namespace BadPig.Orders.Core;

/// <summary>An order of a member. Prices and names are copied from the catalogue when the order is made: later changes do not touch it.</summary>
public sealed class Order
{
    public Guid Id { get; set; }

    /// <summary>The buyer's account; null once the account is deleted (the order stays for the club's accounts, anonymous).</summary>
    public Guid? BuyerId { get; set; }

    public string BuyerName { get; set; } = "";
    public string Status { get; set; } = OrderStatuses.Pending;
    public int TotalCents { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>End of the reservation of the stock (and of the payment page).</summary>
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? DeliveredAt { get; set; }

    /// <summary>The board member who handed the order over; null once their account is deleted (the name is then replaced too).</summary>
    public Guid? DeliveredById { get; set; }

    public string? DeliveredByName { get; set; }

    /// <summary>Identifier of the payment page at the payment provider.</summary>
    public string? PaymentSessionId { get; set; }

    /// <summary>Address of the payment page.</summary>
    public string? CheckoutUrl { get; set; }

    public List<OrderLine> Lines { get; set; } = [];
}
