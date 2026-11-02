namespace BadPig.Orders.Core;

public static class OrderRules
{
    /// <summary>How long the stock is held for an order. The payment page lasts as long (the provider requires at least 30 minutes).</summary>
    public static readonly TimeSpan Reservation = TimeSpan.FromMinutes(35);

    public const int MaxLines = 20;
    public const int MaxQuantityPerLine = 10;

    public static bool IsWaitingForPayment(Order order) => order.Status == OrderStatuses.Pending;

    public static bool IsToHandOver(Order order) => order.Status == OrderStatuses.Paid;
}
