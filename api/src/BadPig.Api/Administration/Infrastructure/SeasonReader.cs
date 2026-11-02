using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Infrastructure;

internal sealed class SeasonReader(AppDbContext db, IDateTimeProvider clock) : ISeasonReader
{
    public async Task<IReadOnlyList<SeasonInfo>> ListAsync(CancellationToken ct) =>
        (await db.Seasons.AsNoTracking().OrderByDescending(s => s.StartsOn).ToListAsync(ct)).Select(Info).ToList();

    public async Task<SeasonInfo?> CurrentAsync(CancellationToken ct)
    {
        var seasons = await db.Seasons.AsNoTracking().ToListAsync(ct);
        var current = SeasonRules.Current(seasons, SeasonRules.Today(clock.UtcNow));
        return current is null ? null : Info(current);
    }

    public async Task<SeasonInfo?> FindAsync(Guid id, CancellationToken ct)
    {
        var season = await db.Seasons.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id, ct);
        return season is null ? null : Info(season);
    }

    private static SeasonInfo Info(Season season) => new(season.Id, season.Name, season.StartsOn, season.EndsOn);
}
