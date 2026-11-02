using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BadPig.Shared.Identity;

/// <summary>Registration of the identity provider: the only place that knows its settings (configuration section "Authentik").</summary>
public static class IdentityModule
{
    /// <summary>Registers <see cref="IIdentityProvider"/> and the client of the provider's API.</summary>
    public static IServiceCollection AddIdentityProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthentikOptions>(configuration.GetSection(AuthentikOptions.Section));

        services.AddHttpClient<AuthentikClient>((sp, http) =>
        {
            var options = sp.GetRequiredService<IOptions<AuthentikOptions>>().Value;
            http.BaseAddress = new Uri(options.ApiBase.TrimEnd('/') + "/");
            http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.ApiToken);
            // The API talks to Authentik through the internal network, but the links written in e-mails (activation,
            // password) must carry the public address: Authentik builds them from the "Host" header.
            http.DefaultRequestHeaders.Host = new Uri(options.Issuer).Host;
            http.DefaultRequestHeaders.Add("X-Forwarded-Proto", "https");
            http.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<IIdentityProvider, AuthentikIdentityProvider>();
        return services;
    }

    /// <summary>Authentication: tokens issued by the identity provider (signature, issuer, audience, lifetime).</summary>
    public static IServiceCollection AddIdentityAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(AuthentikOptions.Section).Get<AuthentikOptions>() ?? new();
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.Authority = options.Issuer;   // public keys are read from the OIDC configuration (JWKS)
                o.MapInboundClaims = false;     // keeps the original claim names: sub, name, email, groups
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "name",
                };
            });
        return services;
    }
}
