namespace BadPig.Administration.Features.GetMailboxStatus;

/// <summary>What waits in the club mailbox, for the board home page.</summary>
/// <param name="IsAvailable">False when no mailbox is configured or when the mail server could not be reached: the site then shows nothing.</param>
/// <param name="UnreadCount">Messages not opened yet; 0 when the mailbox is not available.</param>
/// <param name="WebmailUrl">Address where the mail is read, null when none is configured.</param>
/// <param name="CheckedAt">When the mail server answered; the answer is kept for a minute, so it may be slightly older than the call.</param>
public sealed record GetMailboxStatusResponse(bool IsAvailable, int UnreadCount, string? WebmailUrl, DateTimeOffset? CheckedAt);
