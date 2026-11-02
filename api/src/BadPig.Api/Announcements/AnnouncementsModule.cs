using BadPig.Announcements.Core;
using BadPig.Announcements.Features.DeleteExpiredAnnouncements;
using BadPig.Announcements.Infrastructure;
using BadPig.Shared.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BadPig.Announcements;

/// <summary>Announcements published by the board to the club members (internal) or to any visitor (public).</summary>
public static class AnnouncementsModule
{
    public static IServiceCollection AddAnnouncementsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AnnouncementLinks>(configuration.GetSection(AnnouncementLinks.Section));
        services.Configure<AnnouncementStorageOptions>(configuration.GetSection(AnnouncementStorageOptions.Section));
        services.AddSingleton<IRichText, HtmlRichText>();
        services.AddSingleton<IAnnouncementFileStore, DiskAnnouncementFileStore>();
        services.AddScoped<AnnouncementViewer>();
        services.AddScoped<IAccountErasure, AnnouncementsEraser>();
        services.AddHostedService<ExpiredAnnouncementCleanup>();
        return services;
    }
}
