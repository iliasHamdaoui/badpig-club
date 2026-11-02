namespace BadPig.Shop.Core;

/// <summary>The picture of an article, ready to be sent.</summary>
public sealed record ProductImageFile(string ContentType, byte[] Content);
