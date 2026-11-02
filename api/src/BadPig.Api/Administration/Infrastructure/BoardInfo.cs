using BadPig.Administration.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Infrastructure;

internal sealed class BoardInfo(AppDbContext db) : IBoardInfo
{
    public async Task<BoardTitleInfo?> GetTitleAsync(string accountId, CancellationToken ct)
    {
        if (!Guid.TryParse(accountId, out var id))
            return null;

        var row = await db.BoardTitles.AsNoTracking().Include(b => b.Title).FirstOrDefaultAsync(b => b.MemberId == id, ct);
        return row?.Title is null ? null : new BoardTitleInfo(row.Title.Id.ToString(), row.Title.Label);
    }
}
