namespace BadPig.Announcements.Features.DeleteAnnouncement;

/// <summary>Deletion of an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record DeleteAnnouncementRequest(string Id);
