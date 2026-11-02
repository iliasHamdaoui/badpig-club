namespace BadPig.Shop.Features.RemoveProductImage;

/// <summary>Removes the picture of an article.</summary>
/// <param name="Id">Identifier of the article.</param>
public sealed record RemoveProductImageRequest(string Id);
