namespace BadPig.Shop.Features.CreateVariant;

/// <summary>Creation of a variant.</summary>
/// <param name="Id">Identifier of the article.</param>
/// <param name="Body">The variant.</param>
public sealed record CreateVariantRequest(string Id, CreateVariantBody Body);
