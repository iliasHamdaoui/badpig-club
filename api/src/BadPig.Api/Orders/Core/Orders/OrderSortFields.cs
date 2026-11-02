namespace BadPig.Orders.Core;

/// <summary>The fields the list of orders can be sorted on (as written in the query string).</summary>
public static class OrderSortFields
{
    public const string CreatedAt = "createdAt";
    public const string TotalCents = "totalCents";
    public const string BuyerName = "buyerName";

    public static readonly string[] All = [CreatedAt, TotalCents, BuyerName];
}
