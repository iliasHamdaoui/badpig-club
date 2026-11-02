namespace BadPig.Announcements.Features.GetAnnouncementFile;

/// <summary>The content of a file, ready to be sent.</summary>
/// <param name="Content">Content to stream (closed by the framework once sent).</param>
/// <param name="ContentType">Media type, decided from the extension.</param>
/// <param name="FileName">Name of the file.</param>
/// <param name="IsInline">True for the poster (shown in the page), false for an attachment (downloaded).</param>
public sealed record GetAnnouncementFileResponse(Stream Content, string ContentType, string FileName, bool IsInline);
