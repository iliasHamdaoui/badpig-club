namespace BadPig.Shop.Features.UpdateVariant;

/// <summary>JSON body of the change of a variant.</summary>
/// <param name="Label">Label (1 to 100 characters), unique in the article.</param>
/// <param name="PriceCents">Price of one unit in euro cents (1 to 100,000).</param>
/// <param name="LowStockThreshold">Alert threshold (0 = no alert).</param>
/// <param name="IsActive">True = offered to the members; false = hidden.</param>
public sealed record UpdateVariantBody(string? Label, int? PriceCents, int? LowStockThreshold, bool? IsActive);
