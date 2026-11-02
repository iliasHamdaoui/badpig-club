namespace BadPig.Announcements.Features.UnpublishAnnouncement;

/// <summary>Withdrawal of an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record UnpublishAnnouncementRequest(string Id);
