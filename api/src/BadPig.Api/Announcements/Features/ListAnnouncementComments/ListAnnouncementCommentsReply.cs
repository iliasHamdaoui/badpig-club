namespace BadPig.Announcements.Features.ListAnnouncementComments;

/// <summary>One answer to a comment (oldest first, under its comment).</summary>
/// <param name="Id">Answer identifier.</param>
/// <param name="AuthorName">Name of the author when the answer was written.</param>
/// <param name="Text">Plain text of the answer.</param>
/// <param name="CreatedAt">Date of the answer.</param>
/// <param name="IsMine">True when the signed-in person wrote the answer.</param>
/// <param name="CanDelete">True when the signed-in person can delete it (its author, or a board member).</param>
public sealed record ListAnnouncementCommentsReply(string Id, string AuthorName, string Text, DateTimeOffset CreatedAt, bool IsMine, bool CanDelete);
