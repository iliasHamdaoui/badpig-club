using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Infrastructure;

/// <summary>
/// Erases what the announcements know about a deleted account: its likes, answers and comments are removed and the announcements it wrote stay
/// without author (the club's information is not personal data, the name of the author is).
/// </summary>
internal sealed class AnnouncementsEraser(AppDbContext db) : IAccountErasure
{
    public const string FormerAuthorName = "Ancien membre du bureau";

    public async Task EraseAsync(string accountId, CancellationToken ct)
    {
        var id = Guid.Parse(accountId);
        await db.AnnouncementLikes.Where(l => l.MemberId == id).ExecuteDeleteAsync(ct);
        await db.AnnouncementComments.Where(c => c.AuthorId == id).ExecuteDeleteAsync(ct);
        await db.PollVotes.Where(v => v.MemberId == id).ExecuteDeleteAsync(ct);
        await db.Announcements.Where(a => a.AuthorId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.AuthorId, (Guid?)null).SetProperty(a => a.AuthorName, FormerAuthorName), ct);
    }
}
