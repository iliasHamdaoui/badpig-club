namespace BadPig.Shop.Features.CreateVariant;

/// <summary>JSON body of the creation of a variant.</summary>
/// <param name="Label">Label (1 to 100 characters), unique in the article, e.g. "Vitesse 77" or "M - Homme".</param>
/// <param name="PriceCents">Price of one unit in euro cents (1 to 100,000).</param>
/// <param name="Quantity">Units in stock now (default 0).</param>
/// <param name="LowStockThreshold">The board is warned when the available stock falls to this number or below (default 0 = no alert).</param>
public sealed record CreateVariantBody(string? Label, int? PriceCents, int? Quantity = null, int? LowStockThreshold = null);
