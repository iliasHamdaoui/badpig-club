namespace BadPig.Orders.Features.GetProductImage;

/// <summary>Reads the picture of an article.</summary>
/// <param name="Id">Identifier of the article.</param>
public sealed record GetProductImageRequest(string Id);
