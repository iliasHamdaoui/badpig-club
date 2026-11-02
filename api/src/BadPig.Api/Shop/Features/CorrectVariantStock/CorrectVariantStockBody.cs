namespace BadPig.Shop.Features.CorrectVariantStock;

/// <summary>JSON body of a stock correction.</summary>
/// <param name="Quantity">Units really in stock (0 to 100,000).</param>
/// <param name="Note">Why the stock was wrong (required, 200 characters at most).</param>
public sealed record CorrectVariantStockBody(int? Quantity, string? Note);
