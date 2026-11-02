using BadPig.Orders.Core;
using BadPig.Orders.Features.ReleaseExpiredOrders;
using BadPig.Orders.Infrastructure;
using BadPig.Shared.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BadPig.Orders;

/// <summary>The orders of the members: catalogue, order, online payment, my orders. The board manages them from the Shop module.</summary>
public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // The setting keeps the section name "Shop" (Shop__SiteUrl): it is the address of the site, shared by the whole shop.
        services.Configure<OrderLinks>(configuration.GetSection(OrderLinks.Section));
        services.AddScoped<OrderAccess>();
        services.AddScoped<OrderSettlement>();
        services.AddScoped<IOrderBook, OrderBook>();
        services.AddScoped<IAccountErasure, OrdersEraser>();
        services.AddHostedService<UnpaidOrderCleanup>();
        return services;
    }
}
