namespace BadPig.Announcements.Features.UpdateAnnouncement;

/// <summary>JSON body of the update of an announcement (the whole content is replaced; published or not does not change).</summary>
/// <param name="Title">Title (1 to 150 characters).</param>
/// <param name="BodyHtml">Rich text as HTML, cleaned by the server. 50,000 characters at most. Required for a text announcement; optional for a poll (empty = no explanation).</param>
/// <param name="Kind">"internal" (club members only) or "public" (any visitor of the site).</param>
/// <param name="ExpiresOn">New last day the announcement is shown (yyyy-MM-dd, Paris date, not in the past). Null = unchanged.</param>
/// <param name="CommentsEnabled">True = members can comment; false = comments are closed (existing comments are kept, hidden from the site). Null = unchanged.</param>
public sealed record UpdateAnnouncementBody(string? Title, string? BodyHtml, string? Kind, DateOnly? ExpiresOn = null, bool? CommentsEnabled = null);
