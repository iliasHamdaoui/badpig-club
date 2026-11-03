namespace BadPig.Profile.Features.SetMyPhoto;

/// <summary>A picture sent with the request.</summary>
/// <param name="FileName">Name given by the sender.</param>
/// <param name="Length">Size in bytes.</param>
/// <param name="Content">Content of the file.</param>
public sealed record UploadedPhoto(string FileName, long Length, Stream Content);
