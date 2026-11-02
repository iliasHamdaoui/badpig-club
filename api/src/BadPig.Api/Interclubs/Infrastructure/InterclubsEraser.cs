using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Infrastructure;

/// <summary>Erases what the interclubs know about a deleted account: the person is no longer the captain nor a player of a team, they no longer appear in the lineups, and their answers to the proposed dates disappear.</summary>
internal sealed class InterclubsEraser(AppDbContext db) : IAccountErasure
{
    public async Task EraseAsync(string accountId, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        await db.InterclubTeams.Where(t => t.CaptainId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.CaptainId, (Guid?)null), ct);
        await db.InterclubFixtureMatches.Where(m => m.Player1Id == id).ExecuteUpdateAsync(s => s.SetProperty(m => m.Player1Id, (Guid?)null), ct);
        await db.InterclubFixtureMatches.Where(m => m.Player2Id == id).ExecuteUpdateAsync(s => s.SetProperty(m => m.Player2Id, (Guid?)null), ct);
        await db.InterclubTeamMembers.Where(m => m.AccountId == id).ExecuteDeleteAsync(ct);
        await db.InterclubDateAnswers.Where(a => a.AccountId == id).ExecuteDeleteAsync(ct);
    }
}
