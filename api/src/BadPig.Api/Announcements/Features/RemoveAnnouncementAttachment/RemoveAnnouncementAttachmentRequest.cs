namespace BadPig.Announcements.Features.RemoveAnnouncementAttachment;

/// <summary>Removal of an attachment of an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="FileId">Identifier of the attachment.</param>
public sealed record RemoveAnnouncementAttachmentRequest(string Id, string FileId);
