using BadPig.Orders.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BadPig.Orders.Features.ReleaseExpiredOrders;

/// <summary>
/// Background task: every minute, settles the orders whose time is over (the payment provider is asked first, a late payment is not lost):
/// the unpaid ones expire and their stock is released. The lists also do it when they are read, so the stock is right at any time.
/// </summary>
public sealed class UnpaidOrderCleanup(IServiceScopeFactory scopes, ILogger<UnpaidOrderCleanup> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        try { await Task.Delay(TimeSpan.FromSeconds(30), stop); } catch (OperationCanceledException) { return; }

        while (!stop.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<OrderSettlement>().ReleaseExpiredAsync(stop);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                log.LogError(e, "Release of the expired orders failed (will retry at the next pass)");
            }

            try { await Task.Delay(TimeSpan.FromMinutes(1), stop); } catch (OperationCanceledException) { return; }
        }
    }
}
