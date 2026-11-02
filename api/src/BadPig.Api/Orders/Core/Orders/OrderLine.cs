namespace BadPig.Orders.Core;

/// <summary>One article of an order, as it was sold.</summary>
public sealed class OrderLine
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }

    /// <summary>The variant bought; no foreign key: the line stays if the variant is deleted later.</summary>
    public Guid? VariantId { get; set; }

    public string ProductName { get; set; } = "";
    public string VariantLabel { get; set; } = "";
    public int UnitPriceCents { get; set; }
    public int Quantity { get; set; }
}
