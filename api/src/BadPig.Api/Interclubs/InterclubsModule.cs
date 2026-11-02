using BadPig.Interclubs.Core;
using BadPig.Interclubs.Infrastructure;
using BadPig.Shared.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BadPig.Interclubs;

/// <summary>The club's teams in the interclub competition: captains, calendar of the fixtures, lineups and scores.</summary>
public static class InterclubsModule
{
    public static IServiceCollection AddInterclubsModule(this IServiceCollection services)
    {
        services.AddScoped<InterclubAccess>();
        services.AddScoped<IAccountErasure, InterclubsEraser>();
        services.AddScoped<ISeasonTeams, SeasonTeams>();
        return services;
    }
}
