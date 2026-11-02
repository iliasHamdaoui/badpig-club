using BadPig.Shared.Core;
using BadPig.Shared.Mailbox;
using Microsoft.Extensions.Options;

namespace BadPig.Administration.Features.GetMailboxStatus;

/// <summary>
/// Tells the board whether new mail waits in the club mailbox. Only a count is read: the messages stay on the mail
/// server, which is where the board reads them. An unreachable mailbox is not an error, it is simply not available.
/// </summary>
internal sealed class GetMailboxStatusHandler(IMailboxReader mailbox, IOptions<MailboxOptions> options)
    : IHandler<GetMailboxStatusRequest, GetMailboxStatusResponse>
{
    public async Task<Result<GetMailboxStatusResponse>> HandleAsync(GetMailboxStatusRequest request, CancellationToken ct)
    {
        var status = await mailbox.ReadStatusAsync(ct);
        if (status is null)
            return new GetMailboxStatusResponse(false, 0, null, null);

        var webmail = options.Value.WebmailUrl;
        return new GetMailboxStatusResponse(true, status.UnreadCount, string.IsNullOrWhiteSpace(webmail) ? null : webmail, status.CheckedAt);
    }
}
