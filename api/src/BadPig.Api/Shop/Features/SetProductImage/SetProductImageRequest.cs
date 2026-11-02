namespace BadPig.Shop.Features.SetProductImage;

/// <summary>Sets the picture of an article.</summary>
/// <param name="Id">Identifier of the article.</param>
/// <param name="File">The image sent (JPEG, PNG or WebP, 3 MB at most).</param>
public sealed record SetProductImageRequest(string Id, UploadedImage? File);
