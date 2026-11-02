namespace BadPig.Shop.Core;

/// <summary>One line of the stock history of a variant: what changed, why, who did it.</summary>
public sealed class StockMovement
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }

    /// <summary>Units added (positive) or removed (negative).</summary>
    public int Change { get; set; }

    public string Reason { get; set; } = "";
    public string Note { get; set; } = "";

    /// <summary>The order that was paid, for a sale.</summary>
    public Guid? OrderId { get; set; }

    /// <summary>The board member who changed the stock; null for a sale or once the account is deleted.</summary>
    public Guid? AuthorId { get; set; }

    public string AuthorName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}
