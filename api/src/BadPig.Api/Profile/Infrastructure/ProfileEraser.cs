using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Profile.Infrastructure;

/// <summary>Erases what the Profile module keeps about a deleted account: its profile picture.</summary>
internal sealed class ProfileEraser(AppDbContext db) : IAccountErasure
{
    public async Task EraseAsync(string accountId, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        await db.MemberPhotos.Where(p => p.AccountId == id).ExecuteDeleteAsync(ct);
    }
}
