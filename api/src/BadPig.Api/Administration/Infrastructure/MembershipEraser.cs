// Erases the memberships of a deleted account (right to erasure).
using Microsoft.EntityFrameworkCore;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;

namespace BadPig.Administration.Infrastructure;

internal sealed class MembershipEraser(AppDbContext db) : IAccountErasure
{
    public async Task EraseAsync(string accountId, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        await db.Memberships.Where(m => m.MemberId == id).ExecuteDeleteAsync(ct);
    }
}
