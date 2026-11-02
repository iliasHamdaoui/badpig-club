using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Infrastructure;

internal sealed class MembershipReader(AppDbContext db, IDateTimeProvider clock) : IMembershipReader
{
    public async Task<MembershipStatus> GetAsync(string accountId, CancellationToken ct)
    {
        var seasons = await db.Seasons.AsNoTracking().ToListAsync(ct);
        var today = SeasonRules.Today(clock.UtcNow);
        if (!Guid.TryParse(accountId, out var id))
            return MembershipStatus.Of(seasons, [], today);

        var memberships = await db.Memberships.AsNoTracking().Where(m => m.MemberId == id).ToListAsync(ct);
        return MembershipStatus.Of(seasons, memberships, today);
    }
}
