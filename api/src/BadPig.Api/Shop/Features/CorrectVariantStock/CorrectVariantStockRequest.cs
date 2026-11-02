namespace BadPig.Shop.Features.CorrectVariantStock;

/// <summary>Correction of the stock of a variant.</summary>
/// <param name="Id">Identifier of the variant.</param>
/// <param name="Body">The counted quantity and why.</param>
public sealed record CorrectVariantStockRequest(string Id, CorrectVariantStockBody Body);
