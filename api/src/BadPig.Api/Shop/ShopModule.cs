using BadPig.Shared.Core;
using BadPig.Shop.Core;
using BadPig.Shop.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BadPig.Shop;

/// <summary>The shop managed by the board: articles with variants, stock and its history, and the orders received. The members order from the Orders module.</summary>
public static class ShopModule
{
    public static IServiceCollection AddShopModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ShopOptions>(configuration.GetSection(ShopOptions.Section));
        services.AddScoped<IShopCatalog, ShopCatalog>();
        services.AddScoped<IStockKeeper, StockKeeper>();
        services.AddScoped<IAccountErasure, ShopEraser>();
        services.AddScoped<IShopAudit, ShopAuditService>();
        return services;
    }
}
