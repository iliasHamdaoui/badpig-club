using BadPig.Administration.Core;
using BadPig.Administration.Features.DeleteDisabledAccounts;
using BadPig.Administration.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BadPig.Shared.Core;

namespace BadPig.Administration;

/// <summary>Administrative operations of the club done by the board: accounts life cycle, board composition and titles, journal.</summary>
public static class AdministrationModule
{
    public static IServiceCollection AddAdministrationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAdministrationAudit, AdministrationAuditService>();
        services.AddScoped<IBoardInfo, BoardInfo>();
        services.AddScoped<AccountDeletedNotification>();
        services.AddScoped<TitleRemovedNotification>();
        services.AddScoped<AccountAccess>();
        services.AddScoped<IMembershipReader, MembershipReader>();
        services.AddScoped<ISeasonReader, SeasonReader>();
        services.AddScoped<ILicenceBook, LicenceBook>();
        services.AddScoped<IAccountErasure, MembershipEraser>();
        services.AddScoped<IAccountErasure, AdministrationEraser>();
        services.AddScoped<IAccountErasure, BoardEraser>();
        services.AddHostedService<DisabledAccountCleanup>();
        return services;
    }
}
