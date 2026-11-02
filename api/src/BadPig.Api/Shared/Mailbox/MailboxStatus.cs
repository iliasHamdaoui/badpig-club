namespace BadPig.Shared.Mailbox;

/// <summary>What the club mailbox holds right now: only counts, never the messages themselves.</summary>
/// <param name="UnreadCount">Messages not opened yet in the inbox.</param>
/// <param name="CheckedAt">When the mail server answered (the answer is kept for a short while).</param>
public sealed record MailboxStatus(int UnreadCount, DateTimeOffset CheckedAt);
