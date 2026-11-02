namespace BadPig.Announcements.Features.CreateAnnouncement;

/// <summary>JSON body of the creation of an announcement. The announcement starts as a draft (not published).</summary>
/// <param name="Title">Title (1 to 150 characters).</param>
/// <param name="BodyHtml">Rich text as HTML (paragraphs, titles, lists, bold, italic, links, emojis). It is cleaned by the server: scripts and unsafe markup are removed. 50,000 characters at most.</param>
/// <param name="Kind">"internal" (club members only) or "public" (any visitor of the site).</param>
/// <param name="ExpiresOn">Last day the announcement is shown (yyyy-MM-dd, Paris date, not in the past). Default: 90 days from today. After it, the announcement is hidden and then deleted automatically.</param>
/// <param name="CommentsEnabled">True = members can comment (default); false = comments are closed.</param>
public sealed record CreateAnnouncementRequest(string? Title, string? BodyHtml, string? Kind, DateOnly? ExpiresOn = null, bool? CommentsEnabled = null);
