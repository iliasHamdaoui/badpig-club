namespace BadPig.Shop.Features.SetProductImage;

/// <summary>A picture sent by the board with a request.</summary>
/// <param name="FileName">Name given by the sender.</param>
/// <param name="Length">Size in bytes.</param>
/// <param name="Content">Content of the file.</param>
public sealed record UploadedImage(string FileName, long Length, Stream Content);
