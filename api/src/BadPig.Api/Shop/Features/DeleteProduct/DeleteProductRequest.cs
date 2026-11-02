namespace BadPig.Shop.Features.DeleteProduct;

/// <summary>Deletion of an article.</summary>
/// <param name="Id">Identifier of the article.</param>
public sealed record DeleteProductRequest(string Id);
