using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Infrastructure;

/// <summary>Erases the data of the Administration module about a deleted account (invitation, disabling date, licence number).</summary>
internal sealed class AdministrationEraser(AppDbContext db) : IAccountErasure
{
    public async Task EraseAsync(string accountId, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        await db.Invitations.Where(i => i.TargetId == id).ExecuteDeleteAsync(ct);
        await db.DisabledAccounts.Where(d => d.AccountId == id).ExecuteDeleteAsync(ct);
        await db.MemberLicences.Where(l => l.AccountId == id).ExecuteDeleteAsync(ct);
    }
}
