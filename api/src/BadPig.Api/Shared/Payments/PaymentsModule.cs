using BadPig.Shared.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BadPig.Shared.Payments;

/// <summary>Registration of the payment provider: the only place that knows its settings (configuration section "Stripe").</summary>
public static class PaymentsModule
{
    public static IServiceCollection AddPaymentProvider(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.Section));
        services.AddHttpClient<IPaymentProvider, StripePaymentProvider>(http => http.Timeout = TimeSpan.FromSeconds(15));
        return services;
    }
}
