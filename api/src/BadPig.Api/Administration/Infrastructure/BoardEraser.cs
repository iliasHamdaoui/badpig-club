using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Infrastructure;

/// <summary>Erases the board title of a deleted account (right to erasure).</summary>
internal sealed class BoardEraser(AppDbContext db) : IAccountErasure
{
    public async Task EraseAsync(string accountId, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        await db.BoardTitles.Where(b => b.MemberId == id).ExecuteDeleteAsync(ct);
    }
}
