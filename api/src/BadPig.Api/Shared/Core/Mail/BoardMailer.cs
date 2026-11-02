using BadPig.Shared.Identity;

namespace BadPig.Shared.Core;

/// <summary>
/// Sends an automatic e-mail to the board: to the club's address when one is configured (a mailbox or a mailing list), otherwise to
/// every active board member (groups Board and Admin), each at their own address. A sending failure never cancels what triggered it.
/// </summary>
public sealed class BoardMailer(IIdentityProvider identity, IEmailSender mail)
{
    public async Task SendAsync(string? clubAddress, string subject, string body, CancellationToken ct)
    {
        foreach (var recipient in await RecipientsAsync(clubAddress, ct))
            await mail.SendAsync(recipient, subject, body, ct);
    }

    private async Task<IReadOnlyList<AccountInfo>> RecipientsAsync(string? clubAddress, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(clubAddress))
            return [new AccountInfo("", "", "le bureau", clubAddress.Trim(), true, [], null)];

        return (await identity.ListAsync(ct)).Where(a => a.IsActive && AccountRules.IsBoardOrAdmin(a)).ToList();
    }
}
