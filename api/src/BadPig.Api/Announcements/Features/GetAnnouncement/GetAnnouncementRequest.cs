namespace BadPig.Announcements.Features.GetAnnouncement;

/// <summary>Reading of one announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record GetAnnouncementRequest(string Id);
