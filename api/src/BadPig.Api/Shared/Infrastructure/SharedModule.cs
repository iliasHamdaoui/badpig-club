using BadPig.Shared.Infrastructure.Mail;
using FluentValidation;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BadPig.Shared.Core;

namespace BadPig.Shared.Infrastructure;

public static class SharedModule
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Base");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Missing configuration: ConnectionStrings__Base");

        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
        services.AddValidatorsFromAssembly(typeof(SharedModule).Assembly, includeInternalTypes: true);
        AddHandlers(services);
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddHttpContextAccessor();
        services.AddScoped<AccountDeletionService>();
        services.AddScoped<BoardMailer>();
        services.AddScoped<IUserContext, HttpUserContext>();
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.Section));
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }

    /// <summary>
    /// Registers every <see cref="IHandler{TRequest,TResponse}"/> and <see cref="IHandler{TRequest}"/> of the application: adding a feature needs no change here.
    /// </summary>
    private static void AddHandlers(IServiceCollection services)
    {
        var handlers = typeof(SharedModule).Assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false });
        foreach (var type in handlers)
        {
            // IHandler<TRequest, TResponse> (a value) and IHandler<TRequest> (nothing: 204).
            foreach (var contract in type.GetInterfaces().Where(i => i.IsGenericType
                && (i.GetGenericTypeDefinition() == typeof(IHandler<,>) || i.GetGenericTypeDefinition() == typeof(IHandler<>))))
            {
                services.AddScoped(contract, type);
            }
        }
    }
}
