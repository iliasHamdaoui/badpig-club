namespace BadPig.Announcements.Features.DeleteAnnouncementComment;

/// <summary>Deletion of a comment.</summary>
/// <param name="Id">Identifier of the announcement.</param>
/// <param name="CommentId">Identifier of the comment.</param>
public sealed record DeleteAnnouncementCommentRequest(string Id, string CommentId);
