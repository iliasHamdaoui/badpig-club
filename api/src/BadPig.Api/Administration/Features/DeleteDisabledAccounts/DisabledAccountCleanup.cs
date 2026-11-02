using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using BadPig.Shared.Persistence;

namespace BadPig.Administration.Features.DeleteDisabledAccounts;

/// <summary>
/// Background task: every 6 hours, deletes the accounts that have been disabled for more than a month.
/// Board and admin accounts are never deleted automatically.
/// An account that was already disabled before this rule existed starts its month at the first pass.
/// </summary>
public sealed class DisabledAccountCleanup(IServiceScopeFactory scopes, ILogger<DisabledAccountCleanup> log) : BackgroundService
{
    private const string AuditAction = "account.deleted_automatically";

    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        try { await Task.Delay(TimeSpan.FromMinutes(1), stop); } catch (OperationCanceledException) { return; }

        while (!stop.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stop);
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                log.LogError(e, "Cleanup of disabled accounts failed (will retry at the next pass)");
            }

            try { await Task.Delay(TimeSpan.FromHours(6), stop); } catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var identity = scope.ServiceProvider.GetRequiredService<IIdentityProvider>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var deletion = scope.ServiceProvider.GetRequiredService<AccountDeletionService>();
        var notification = scope.ServiceProvider.GetRequiredService<AccountDeletedNotification>();
        var audit = scope.ServiceProvider.GetRequiredService<IAdministrationAudit>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var accounts = await identity.ListAsync(ct);
        var noted = (await db.DisabledAccounts.AsNoTracking().ToListAsync(ct))
            .ToDictionary(d => d.AccountId.ToString(), d => d.DisabledSince);
        var now = clock.UtcNow;

        // Housekeeping: a disabled account without a date starts its countdown; a date without a disabled account is erased.
        foreach (var a in accounts.Where(a => !a.IsActive && !noted.ContainsKey(a.Id) && AccountRules.IsClubMember(a)))
        {
            db.DisabledAccounts.Add(new DisabledAccount { AccountId = Guid.Parse(a.Id), DisabledSince = now });
            noted[a.Id] = now;
        }

        var inactive = accounts.Where(a => !a.IsActive).Select(a => a.Id).ToHashSet();
        await db.SaveChangesAsync(ct);
        foreach (var id in noted.Keys.Where(id => !inactive.Contains(id)).Select(Guid.Parse))
            await db.DisabledAccounts.Where(d => d.AccountId == id).ExecuteDeleteAsync(ct);

        foreach (var a in accounts.Where(a => !a.IsActive && noted.ContainsKey(a.Id)))
        {
            var info = a;
            if (AccountRules.IsBoardOrAdmin(info) || IsNotDueYet(noted[a.Id], now))
                continue;

            await notification.SendAsync(info, automatic: true, ct); // before deleting: afterwards the e-mail address no longer exists
            await deletion.DeleteAsync(a.Id, ct);
            await audit.RecordBySystemAsync(AuditAction, a.Id, ct);
            log.LogInformation("Account {Target}: deleted automatically (disabled for more than a month)", a.Id);
        }
    }

    private static bool IsNotDueYet(DateTimeOffset disabledSince, DateTimeOffset now) => DisabledAccount.DeletionDate(disabledSince) > now;
}
