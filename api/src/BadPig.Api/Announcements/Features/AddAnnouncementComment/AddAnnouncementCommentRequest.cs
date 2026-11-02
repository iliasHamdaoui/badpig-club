namespace BadPig.Announcements.Features.AddAnnouncementComment;

/// <summary>A member comments an announcement.</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="Body">The comment.</param>
public sealed record AddAnnouncementCommentRequest(string Id, AddAnnouncementCommentBody Body);
