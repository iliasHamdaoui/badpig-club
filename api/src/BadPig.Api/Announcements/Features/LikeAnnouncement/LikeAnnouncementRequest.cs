namespace BadPig.Announcements.Features.LikeAnnouncement;

/// <summary>A member likes an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record LikeAnnouncementRequest(string Id);
