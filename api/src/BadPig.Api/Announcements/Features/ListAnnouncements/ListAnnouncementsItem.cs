namespace BadPig.Announcements.Features.ListAnnouncements;

/// <summary>One announcement of the list.</summary>
/// <param name="Id">Announcement identifier.</param>
/// <param name="Title">Title.</param>
/// <param name="Summary">Plain text excerpt of the content.</param>
/// <param name="Kind">"internal" (club members only) or "public" (any visitor).</param>
/// <param name="Form">"text" (a written announcement) or "poll" (a standalone poll).</param>
/// <param name="IsPublished">True when published; only the board ever sees false.</param>
/// <param name="IsPinned">True when the announcement is pinned at the top of the list.</param>
/// <param name="PublishedAt">Date of the publication; null for a draft.</param>
/// <param name="PosterId">Identifier of the poster file (to download from the file route), or null.</param>
/// <param name="AttachmentCount">Number of attachments.</param>
/// <param name="ExpiresOn">Last day the announcement is shown (yyyy-MM-dd).</param>
/// <param name="IsExpired">True when the end date has passed; only the board still sees such an announcement.</param>
/// <param name="LikeCount">Number of likes; null for a visitor (likes are for the members).</param>
/// <param name="LikedByMe">True when the signed-in member liked it.</param>
/// <param name="AuthorName">Name of the author; only filled for the board, null for everybody else.</param>
/// <param name="PreviewHtml">Beginning of the content with its formatting (titles, lists, emphasis), without links; empty when there is no text.</param>
/// <param name="HasMore">True when the content goes on after the preview (the card then offers to read more).</param>
public sealed record ListAnnouncementsItem(
    string Id,
    string Title,
    string Summary,
    string Kind,
    string Form,
    bool IsPublished,
    bool IsPinned,
    DateTimeOffset? PublishedAt,
    string? PosterId,
    int AttachmentCount,
    DateOnly ExpiresOn,
    bool IsExpired,
    int? LikeCount,
    bool LikedByMe,
    string? AuthorName,
    string PreviewHtml,
    bool HasMore);
