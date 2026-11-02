namespace BadPig.Announcements.Features.GetAnnouncement;

/// <summary>One announcement with its whole content.</summary>
/// <param name="Id">Announcement identifier.</param>
/// <param name="Title">Title.</param>
/// <param name="BodyHtml">Rich text as safe HTML (already cleaned by the server).</param>
/// <param name="Kind">"internal" (club members only) or "public" (any visitor).</param>
/// <param name="Form">"text" (a written announcement) or "poll" (a standalone poll: the title is the question, the text an optional explanation).</param>
/// <param name="IsPublished">True when published; only the board ever sees false.</param>
/// <param name="IsPinned">True when the announcement is pinned at the top of the list.</param>
/// <param name="PublishedAt">Date of the publication; null for a draft.</param>
/// <param name="UpdatedAt">Date of the last change of the content.</param>
/// <param name="Files">The poster and the attachments.</param>
/// <param name="ExpiresOn">Last day the announcement is shown (yyyy-MM-dd).</param>
/// <param name="IsExpired">True when the end date has passed; only the board still sees such an announcement, until it is deleted automatically.</param>
/// <param name="CommentsEnabled">True when members can comment the announcement (the comments are then listed by the comments route); false when comments are closed.</param>
/// <param name="Poll">The poll data; filled when the form is "poll", null otherwise.</param>
/// <param name="LikeCount">Number of likes; null for a visitor (likes are for the members).</param>
/// <param name="LikedByMe">True when the signed-in member liked it.</param>
/// <param name="AuthorName">Name of the author; only filled for the board, null for everybody else.</param>
public sealed record GetAnnouncementResponse(
    string Id,
    string Title,
    string BodyHtml,
    string Kind,
    string Form,
    bool IsPublished,
    bool IsPinned,
    DateTimeOffset? PublishedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<GetAnnouncementFileItem> Files,
    DateOnly ExpiresOn,
    bool IsExpired,
    bool CommentsEnabled,
    GetAnnouncementPoll? Poll,
    int? LikeCount,
    bool LikedByMe,
    string? AuthorName);
