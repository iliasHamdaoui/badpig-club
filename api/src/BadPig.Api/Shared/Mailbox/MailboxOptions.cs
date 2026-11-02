namespace BadPig.Shared.Mailbox;

/// <summary>
/// Settings of the club mailbox (configuration section "Mailbox"). The password is a secret: it lives in the server's
/// .env file, never in Git. An empty host or user turns the feature off and the site then shows nothing.
/// </summary>
public sealed class MailboxOptions
{
    public const string Section = "Mailbox";

    /// <summary>IMAP server of the mailbox (for example the provider's one). Empty: the feature is off.</summary>
    public string Host { get; set; } = "";

    public int Port { get; set; } = 993;

    /// <summary>Full address used to sign in, for example contact@badpig.fr.</summary>
    public string User { get; set; } = "";

    public string Password { get; set; } = "";

    /// <summary>Webmail address the board is sent to to read the mail; empty: no link is shown.</summary>
    public string WebmailUrl { get; set; } = "";
}
