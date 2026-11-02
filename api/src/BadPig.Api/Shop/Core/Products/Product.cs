namespace BadPig.Shop.Core;

/// <summary>An article the club sells (a tube of shuttlecocks, a club shirt...). Its variants carry the price and the stock.</summary>
public sealed class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";

    /// <summary>False = hidden from the catalogue of the members (the board still sees it).</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
    public List<ProductVariant> Variants { get; set; } = [];
}
