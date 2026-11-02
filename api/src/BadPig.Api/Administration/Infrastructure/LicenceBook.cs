using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Infrastructure;

internal sealed class LicenceBook(AppDbContext db, IDateTimeProvider clock) : ILicenceBook
{
    public async Task<string?> GetAsync(string accountId, CancellationToken ct)
    {
        if (!Guid.TryParse(accountId, out var id))
            return null;

        return (await db.MemberLicences.AsNoTracking().FirstOrDefaultAsync(l => l.AccountId == id, ct))?.Number;
    }

    public async Task SetAsync(string accountId, string? number, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        var cleaned = LicenceRules.Clean(number);
        var existing = await db.MemberLicences.FirstOrDefaultAsync(l => l.AccountId == id, ct);
        if (cleaned is null)
        {
            if (existing is not null)
                db.MemberLicences.Remove(existing);
        }
        else if (existing is null)
        {
            db.MemberLicences.Add(new MemberLicence { AccountId = id, Number = cleaned, UpdatedAt = clock.UtcNow });
        }
        else
        {
            existing.Number = cleaned;
            existing.UpdatedAt = clock.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}
