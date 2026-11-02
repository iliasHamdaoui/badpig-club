using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BadPig.Shared.Mailbox;

/// <summary>Registration of the club mailbox: the only place that knows which mail server is read (section "Mailbox").</summary>
public static class MailboxModule
{
    public static IServiceCollection AddClubMailbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MailboxOptions>(configuration.GetSection(MailboxOptions.Section));
        // Singleton: it keeps the last answer so that opening the board home page does not reconnect every time.
        services.AddSingleton<IMailboxReader, ImapMailboxReader>();
        return services;
    }
}
