namespace BadPig.Announcements.Features.ListAnnouncementComments;

/// <summary>The comment that an answer quotes (chat style).</summary>
/// <param name="Id">Identifier of the quoted comment.</param>
/// <param name="AuthorName">Name of its author; null once it is deleted.</param>
/// <param name="Excerpt">Beginning of its text (120 characters at most); empty once it is deleted.</param>
/// <param name="IsDeleted">True when the quoted comment no longer exists (deleted by its author, the board or with the account).</param>
public sealed record ListAnnouncementCommentsQuote(string Id, string? AuthorName, string Excerpt, bool IsDeleted);
