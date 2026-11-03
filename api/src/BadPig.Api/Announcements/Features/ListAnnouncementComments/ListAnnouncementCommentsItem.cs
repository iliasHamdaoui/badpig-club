namespace BadPig.Announcements.Features.ListAnnouncementComments;

/// <summary>One comment of the conversation.</summary>
/// <param name="Id">Comment identifier.</param>
/// <param name="AuthorId">Account identifier of the author (their picture is served at <c>/members/{authorId}/photo?v={authorPhotoVersion}</c>).</param>
/// <param name="AuthorName">Name of the author when the comment was written.</param>
/// <param name="AuthorPhotoVersion">Version of the author's profile picture, or null when they have none.</param>
/// <param name="Text">Plain text of the comment.</param>
/// <param name="CreatedAt">Date of the comment.</param>
/// <param name="IsMine">True when the signed-in person wrote the comment.</param>
/// <param name="CanDelete">True when the signed-in person can delete it (its author, or a board member). The messages that quote it stay, showing "deleted".</param>
/// <param name="ReplyTo">The comment this one answers; null for a message of its own.</param>
public sealed record ListAnnouncementCommentsItem(
    string Id, string AuthorId, string AuthorName, long? AuthorPhotoVersion, string Text, DateTimeOffset CreatedAt, bool IsMine, bool CanDelete, ListAnnouncementCommentsQuote? ReplyTo);
