namespace BadPig.Shop.Features.UpdateProduct;

/// <summary>Change of an article.</summary>
/// <param name="Id">Identifier of the article.</param>
/// <param name="Body">The new values.</param>
public sealed record UpdateProductRequest(string Id, UpdateProductBody Body);
