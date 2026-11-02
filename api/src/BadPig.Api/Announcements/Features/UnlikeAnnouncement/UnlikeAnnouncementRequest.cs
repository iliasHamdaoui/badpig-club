namespace BadPig.Announcements.Features.UnlikeAnnouncement;

/// <summary>A member takes back a like.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record UnlikeAnnouncementRequest(string Id);
