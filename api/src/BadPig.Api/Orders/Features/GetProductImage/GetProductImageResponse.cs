namespace BadPig.Orders.Features.GetProductImage;

/// <summary>The picture of an article, ready to be sent.</summary>
/// <param name="Content">The bytes of the image.</param>
/// <param name="ContentType">Media type (image/jpeg, image/png or image/webp).</param>
public sealed record GetProductImageResponse(byte[] Content, string ContentType);
