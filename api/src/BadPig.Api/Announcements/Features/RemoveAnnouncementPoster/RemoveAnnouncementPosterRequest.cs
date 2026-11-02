namespace BadPig.Announcements.Features.RemoveAnnouncementPoster;

/// <summary>Removal of the poster of an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
public sealed record RemoveAnnouncementPosterRequest(string Id);
