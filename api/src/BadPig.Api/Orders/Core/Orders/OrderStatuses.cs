namespace BadPig.Orders.Core;

/// <summary>States of an order (column <c>status</c>).</summary>
public static class OrderStatuses
{
    /// <summary>Stock held, waiting for the payment.</summary>
    public const string Pending = "pending";

    /// <summary>Paid, to be handed over.</summary>
    public const string Paid = "paid";

    /// <summary>Handed over to the member by the board.</summary>
    public const string Delivered = "delivered";

    /// <summary>Cancelled by the member before the payment.</summary>
    public const string Cancelled = "cancelled";

    /// <summary>Not paid in time: the stock is released.</summary>
    public const string Expired = "expired";

    public static readonly string[] All = [Pending, Paid, Delivered, Cancelled, Expired];
}
