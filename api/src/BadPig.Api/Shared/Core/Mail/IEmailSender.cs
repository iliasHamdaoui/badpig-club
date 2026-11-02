namespace BadPig.Shared.Core;

/// <summary>
/// Sends an e-mail to the person concerned by a decision. A sending failure never cancels the action: it is only
/// written to the server logs. The texts of the e-mails are in French (they are read by the club members).
/// </summary>
public interface IEmailSender
{
    /// <summary>Does nothing if the recipient is unknown or has no address, or if no mail server is configured.</summary>
    Task SendAsync(AccountInfo? to, string subject, string body, CancellationToken ct);
}
