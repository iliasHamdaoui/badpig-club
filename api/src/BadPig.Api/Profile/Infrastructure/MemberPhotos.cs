using BadPig.Profile.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Profile.Infrastructure;

/// <summary><see cref="IMemberPhotos"/> on the database: only the dates are read, never the bytes of the pictures.</summary>
internal sealed class MemberPhotos(AppDbContext db) : IMemberPhotos
{
    public async Task<IReadOnlyDictionary<string, long>> GetVersionsAsync(IReadOnlyCollection<string> accountIds, CancellationToken ct)
    {
        var ids = accountIds.Where(id => Guid.TryParse(id, out _)).Select(Guid.Parse).ToList();
        if (ids.Count == 0)
            return new Dictionary<string, long>();

        var rows = await db.MemberPhotos.AsNoTracking()
            .Where(p => ids.Contains(p.AccountId))
            .Select(p => new { p.AccountId, p.UpdatedAt })
            .ToListAsync(ct);
        return rows.ToDictionary(r => r.AccountId.ToString(), r => r.UpdatedAt.ToUnixTimeMilliseconds());
    }
}
