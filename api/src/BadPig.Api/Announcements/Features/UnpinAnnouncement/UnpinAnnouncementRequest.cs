namespace BadPig.Announcements.Features.UnpinAnnouncement;

/// <summary>Unpinning of an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record UnpinAnnouncementRequest(string Id);
