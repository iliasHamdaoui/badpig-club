namespace BadPig.Announcements.Core;

/// <summary>A file sent by the board with a request (poster or attachment).</summary>
/// <param name="FileName">Name given by the sender.</param>
/// <param name="Length">Size in bytes.</param>
/// <param name="Content">Content of the file.</param>
public sealed record UploadedFile(string FileName, long Length, Stream Content);
