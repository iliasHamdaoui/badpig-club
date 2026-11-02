using BadPig.Announcements.Core;

namespace BadPig.Announcements.Features.AddAnnouncementAttachment;

/// <summary>Adds an attachment to an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="File">The file sent (PDF, image, Office or OpenDocument document, text; 10 MB at most).</param>
public sealed record AddAnnouncementAttachmentRequest(string Id, UploadedFile? File);
