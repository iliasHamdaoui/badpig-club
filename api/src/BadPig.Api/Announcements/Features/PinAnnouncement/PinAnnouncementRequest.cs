namespace BadPig.Announcements.Features.PinAnnouncement;

/// <summary>Pinning of an announcement at the top of the list.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record PinAnnouncementRequest(string Id);
