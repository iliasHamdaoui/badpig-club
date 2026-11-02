namespace BadPig.Announcements.Features.AddAnnouncementComment;

/// <summary>JSON body of a comment.</summary>
/// <param name="Text">Plain text of the comment (1 to 1000 characters, no HTML).</param>
/// <param name="ParentId">Identifier of the comment this one answers (any earlier comment of the announcement, an answer included); empty for a new message. The answer shows a quote of it.</param>
public sealed record AddAnnouncementCommentBody(string? Text, string? ParentId = null);
