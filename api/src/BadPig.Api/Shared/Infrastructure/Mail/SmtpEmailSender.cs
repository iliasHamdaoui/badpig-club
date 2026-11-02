using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BadPig.Shared.Core;

namespace BadPig.Shared.Infrastructure.Mail;

internal sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> log) : IEmailSender
{
    private static bool HasNoRecipientAddress([System.Diagnostics.CodeAnalysis.NotNullWhen(false)] AccountInfo? to) => to is null || string.IsNullOrWhiteSpace(to.Email);

    private static bool IsSmtpNotConfigured(SmtpOptions smtp) => string.IsNullOrWhiteSpace(smtp.Host);

    public async Task SendAsync(AccountInfo? to, string subject, string body, CancellationToken ct)
    {
        var smtp = options.Value;
        if (HasNoRecipientAddress(to) || IsSmtpNotConfigured(smtp))
            return;

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(smtp.From, smtp.FromName),
                Subject = subject,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Body = $"Bonjour {to.Name},\n\n{body}\n\nLe bureau de BadPig – Badminton Pignan\n\n--\nCe message est automatique, merci de ne pas y répondre.",
                BodyEncoding = System.Text.Encoding.UTF8,
            };
            message.To.Add(new MailAddress(to.Email));
            message.Headers.Add("Auto-Submitted", "auto-generated");

            using var client = new SmtpClient(smtp.Host, smtp.Port) { EnableSsl = false, Timeout = 10_000 };
            await client.SendMailAsync(message, ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            log.LogError(e, "E-mail \"{Subject}\" not sent for account {Target}", subject, to.Id);
        }
    }
}
