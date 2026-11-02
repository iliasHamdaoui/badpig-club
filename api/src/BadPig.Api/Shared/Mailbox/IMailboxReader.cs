namespace BadPig.Shared.Mailbox;

/// <summary>
/// Reads how much new mail is waiting in the club mailbox. It only ever counts: no message, no sender and no subject
/// leaves the mail server, so nothing personal enters the application. Changing mail provider = replacing this folder.
/// </summary>
public interface IMailboxReader
{
    /// <summary>Null when no mailbox is configured, or when the mail server cannot be reached (the failure is logged).</summary>
    Task<MailboxStatus?> ReadStatusAsync(CancellationToken ct);
}
