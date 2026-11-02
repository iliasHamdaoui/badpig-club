namespace BadPig.Shop.Core;

/// <summary>Why the stock of a variant changed (column <c>reason</c> of the stock history).</summary>
public static class StockReasons
{
    /// <summary>Units added by the board (delivery from the supplier, first stock...).</summary>
    public const string Restock = "restock";

    /// <summary>The board set the real quantity after counting the shelves.</summary>
    public const string Correction = "correction";

    /// <summary>Units sold: the order is paid.</summary>
    public const string Sale = "sale";
}
