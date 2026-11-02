using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BadPig.Announcements.Features.DeleteExpiredAnnouncements;

/// <summary>
/// Background task: every 6 hours, deletes the announcements whose end date has passed (drafts included),
/// with their files and likes.
/// </summary>
public sealed class ExpiredAnnouncementCleanup(IServiceScopeFactory scopes, ILogger<ExpiredAnnouncementCleanup> log) : BackgroundService
{
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
                log.LogError(e, "Cleanup of expired announcements failed (will retry at the next pass)");
            }

            try { await Task.Delay(TimeSpan.FromHours(6), stop); } catch (OperationCanceledException) { return; }
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var files = scope.ServiceProvider.GetRequiredService<IAnnouncementFileStore>();
        var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var today = AnnouncementExpiry.TodayAt(clock.UtcNow);
        var expired = await db.Announcements.Include(a => a.Files).Where(a => a.ExpiresOn < today).ToListAsync(ct);
        foreach (var announcement in expired)
        {
            var storageKeys = announcement.Files.Select(f => f.StorageKey).ToList();
            db.Announcements.Remove(announcement); // files and likes follow (cascade)
            await db.SaveChangesAsync(ct);
            foreach (var key in storageKeys)
                await files.DeleteAsync(key, ct);

            log.LogInformation("Announcement {Id}: deleted automatically (end date {ExpiresOn} passed)", announcement.Id, announcement.ExpiresOn);
        }
    }
}
