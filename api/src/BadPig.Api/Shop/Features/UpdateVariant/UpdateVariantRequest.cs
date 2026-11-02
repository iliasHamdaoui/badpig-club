namespace BadPig.Shop.Features.UpdateVariant;

/// <summary>Change of a variant.</summary>
/// <param name="Id">Identifier of the variant.</param>
/// <param name="Body">The new values.</param>
public sealed record UpdateVariantRequest(string Id, UpdateVariantBody Body);
