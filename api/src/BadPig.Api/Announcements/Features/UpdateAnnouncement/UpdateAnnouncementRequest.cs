namespace BadPig.Announcements.Features.UpdateAnnouncement;

/// <summary>Update of an announcement: the route identifier and the new values.</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="Body">New values of the announcement.</param>
public sealed record UpdateAnnouncementRequest(string Id, UpdateAnnouncementBody Body);
