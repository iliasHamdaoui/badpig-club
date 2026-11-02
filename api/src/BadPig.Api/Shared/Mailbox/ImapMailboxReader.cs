using BadPig.Shared.Core;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BadPig.Shared.Mailbox;

/// <summary>
/// Counts the unread messages of the club mailbox over IMAP. The mailbox is opened read-only and only the identifiers of
/// the unread messages are asked for, so no message is ever downloaded, marked as read or kept by the application.
/// The answer is kept for a short while: the board home page must not open a connection on every visit.
/// </summary>
internal sealed class ImapMailboxReader(IOptions<MailboxOptions> options, IDateTimeProvider clock, ILogger<ImapMailboxReader> log) : IMailboxReader
{
    private static readonly TimeSpan HowLongAnAnswerIsKept = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan ServerTimeout = TimeSpan.FromSeconds(8);

    private readonly SemaphoreSlim _oneAtATime = new(1, 1);
    private MailboxStatus? _lastAnswer;

    private static bool IsNotConfigured(MailboxOptions mailbox) =>
        string.IsNullOrWhiteSpace(mailbox.Host) || string.IsNullOrWhiteSpace(mailbox.User);

    private bool IsStillFresh(DateTimeOffset now) => _lastAnswer is not null && now - _lastAnswer.CheckedAt < HowLongAnAnswerIsKept;

    public async Task<MailboxStatus?> ReadStatusAsync(CancellationToken ct)
    {
        var mailbox = options.Value;
        if (IsNotConfigured(mailbox))
            return null;

        if (IsStillFresh(clock.UtcNow))
            return _lastAnswer;

        await _oneAtATime.WaitAsync(ct);
        try
        {
            // Another request may have refreshed the answer while this one was waiting.
            if (IsStillFresh(clock.UtcNow))
                return _lastAnswer;

            var status = await CountUnreadAsync(mailbox, ct);
            if (status is not null)
                _lastAnswer = status;
            return status;
        }
        finally
        {
            _oneAtATime.Release();
        }
    }

    private async Task<MailboxStatus?> CountUnreadAsync(MailboxOptions mailbox, CancellationToken ct)
    {
        try
        {
            using var client = new ImapClient { Timeout = (int)ServerTimeout.TotalMilliseconds };
            await client.ConnectAsync(mailbox.Host, mailbox.Port, SecureSocketOptions.SslOnConnect, ct);
            await client.AuthenticateAsync(mailbox.User, mailbox.Password, ct);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly, ct);
            var unread = await inbox.SearchAsync(SearchQuery.NotSeen, ct);
            await client.DisconnectAsync(true, ct);

            return new MailboxStatus(unread.Count, clock.UtcNow);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            log.LogWarning(e, "Club mailbox unreachable on {Host}", mailbox.Host);
            return null;
        }
    }
}
