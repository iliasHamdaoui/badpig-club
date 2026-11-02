namespace BadPig.Shop.Features.RestockVariant;

/// <summary>Restocking of a variant.</summary>
/// <param name="Id">Identifier of the variant.</param>
/// <param name="Body">The units received.</param>
public sealed record RestockVariantRequest(string Id, RestockVariantBody Body);
