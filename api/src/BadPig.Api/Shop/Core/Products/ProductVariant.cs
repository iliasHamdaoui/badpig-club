namespace BadPig.Shop.Core;

/// <summary>
/// One way to buy a product: a type of shuttlecocks, a size and a gender of shirt... with its own price and stock.
/// <see cref="Quantity"/> is what is on the shelves; <see cref="ReservedQuantity"/> is held for orders waiting for their payment.
/// </summary>
public sealed class ProductVariant
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string Label { get; set; } = "";

    /// <summary>Price of one unit, in euro cents.</summary>
    public int PriceCents { get; set; }

    /// <summary>Units in stock, the reserved ones included.</summary>
    public int Quantity { get; set; }

    /// <summary>Units held for the orders that are not paid yet.</summary>
    public int ReservedQuantity { get; set; }

    /// <summary>The board is warned when the available stock falls to this number or below (0 = no alert).</summary>
    public int LowStockThreshold { get; set; }

    /// <summary>True once the board was e-mailed that this variant is low; back to false when the stock is above the threshold again.</summary>
    public bool LowStockAlerted { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Units a member can still order.</summary>
    public int Available => Quantity - ReservedQuantity;

    public bool IsLowStock => LowStockThreshold > 0 && Available <= LowStockThreshold;
}
