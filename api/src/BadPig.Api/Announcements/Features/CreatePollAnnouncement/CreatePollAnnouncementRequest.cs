namespace BadPig.Announcements.Features.CreatePollAnnouncement;

/// <summary>JSON body of the creation of a poll. A poll is an announcement of its own; it starts as a draft (not published).</summary>
/// <param name="Title">The question (1 to 150 characters).</param>
/// <param name="BodyHtml">Optional explanation as rich text (cleaned by the server, 50,000 characters at most). Empty = none.</param>
/// <param name="Kind">"internal" (club members only) or "public" (any visitor can see the poll, only members answer).</param>
/// <param name="Options">The choices (2 to 10, 1 to 100 characters each, all different).</param>
/// <param name="AllowMultiple">True = a member can tick several choices; false = exactly one.</param>
/// <param name="IsAnonymous">True = the application never shows who answered what (not even to the board).</param>
/// <param name="ClosesOn">Last day the answers can be given or changed (yyyy-MM-dd, Paris date, not in the past). The results are shown from the next day.</param>
/// <param name="ExpiresOn">Last day the poll is shown (yyyy-MM-dd, Paris date). Default: 90 days from today, or the closing date if later. It cannot be before the closing date.</param>
/// <param name="CommentsEnabled">True = members can comment the poll (default); false = comments are closed.</param>
public sealed record CreatePollAnnouncementRequest(
    string? Title, string? BodyHtml, string? Kind, string[]? Options, bool AllowMultiple, bool IsAnonymous, DateOnly? ClosesOn, DateOnly? ExpiresOn = null, bool? CommentsEnabled = null);
