using BadPig.Shared.Identity;

namespace BadPig.Shared.Core;

/// <summary>
/// Sends an automatic e-mail to the board: to the club's address when one is configured (a mailbox or a mailing list), otherwise to
/// every active board member who has signed in at least once (groups Board and Admin), each at their own address.
/// A sending failure never cancels what triggered it.
/// </summary>
public sealed class BoardMailer(IIdentityProvider identity, IEmailSender mail)
{
    // A board member who has never signed in is left out: their address is not confirmed yet, and these e-mails
    // carry club business (who ordered what, what is left in stock).
    private static bool CanReceiveBoardMail(AccountInfo account) =>
        account.IsActive && AccountRules.HasCompletedSignup(account) && AccountRules.IsBoardOrAdmin(account);

    public async Task SendAsync(string? clubAddress, string subject, string body, CancellationToken ct)
    {
        foreach (var recipient in await RecipientsAsync(clubAddress, ct))
            await mail.SendAsync(recipient, subject, body, ct);
    }

    private async Task<IReadOnlyList<AccountInfo>> RecipientsAsync(string? clubAddress, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(clubAddress))
            return [new AccountInfo("", "", "le bureau", clubAddress.Trim(), true, [], null)];

        return (await identity.ListAsync(ct)).Where(CanReceiveBoardMail).ToList();
    }
}
