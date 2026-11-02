using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Infrastructure;

/// <summary>Erases what the shop knows about a deleted account: the stock history keeps what happened, without the name of the board member.</summary>
internal sealed class ShopEraser(AppDbContext db) : IAccountErasure
{
    public const string FormerBoardMemberName = "Ancien membre du bureau";

    public async Task EraseAsync(string accountId, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        await db.StockMovements.Where(m => m.AuthorId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(m => m.AuthorId, (Guid?)null).SetProperty(m => m.AuthorName, FormerBoardMemberName), ct);
    }
}
