using BadPig.Interclubs.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Infrastructure;

internal sealed class SeasonTeams(AppDbContext db) : ISeasonTeams
{
    public Task<bool> ExistInAsync(Guid seasonId, CancellationToken ct) =>
        db.InterclubTeams.AsNoTracking().AnyAsync(t => t.SeasonId == seasonId, ct);
}
