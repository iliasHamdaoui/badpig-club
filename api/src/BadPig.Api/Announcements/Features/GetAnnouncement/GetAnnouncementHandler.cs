using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.GetAnnouncement;

/// <summary>One announcement, if the reader is allowed to see it.</summary>
internal sealed class GetAnnouncementHandler(IValidator<GetAnnouncementRequest> validator, IUserContext currentUser,
    AnnouncementViewer viewer, AppDbContext db, IIdentityProvider identity) : IHandler<GetAnnouncementRequest, GetAnnouncementResponse>
{
    public async Task<Result<GetAnnouncementResponse>> HandleAsync(GetAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.AsNoTracking().Include(a => a.Files).VisibleTo(audience, viewer.Today).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        var isMember = audience != AnnouncementAudience.Visitor;
        var likeCount = isMember ? await db.AnnouncementLikes.CountAsync(l => l.AnnouncementId == id, ct) : (int?)null;
        var likedByMe = isMember && await db.AnnouncementLikes.AnyAsync(l => l.AnnouncementId == id && l.MemberId == Guid.Parse(currentUser.AccountId!), ct);
        var files = announcement.Files.OrderBy(f => f.CreatedAt)
            .Select(f => new GetAnnouncementFileItem(f.Id.ToString(), f.FileName, f.ContentType, f.SizeBytes, f.Role == AnnouncementFileRoles.Poster))
            .ToList();

        return new GetAnnouncementResponse(
            announcement.Id.ToString(), announcement.Title, announcement.BodyHtml, announcement.Kind, announcement.Form, announcement.IsPublished, announcement.IsPinned,
            announcement.PublishedAt, announcement.UpdatedAt, files, announcement.ExpiresOn, AnnouncementExpiry.IsExpired(announcement.ExpiresOn, viewer.Today), announcement.CommentsEnabled, await PollOfAsync(announcement, audience, ct), likeCount, likedByMe,
            audience == AnnouncementAudience.Board ? announcement.AuthorName : null);
    }
    private async Task<GetAnnouncementPoll?> PollOfAsync(Announcement announcement, AnnouncementAudience audience, CancellationToken ct)
    {
        var poll = await db.AnnouncementPolls.AsNoTracking().Include(p => p.Options).FirstOrDefaultAsync(p => p.AnnouncementId == announcement.Id, ct);
        if (poll is null)
            return null;

        var isClosed = PollRules.IsClosed(poll.ClosesOn, viewer.Today);
        var options = poll.Options.OrderBy(o => o.Position).ToList();
        if (audience == AnnouncementAudience.Visitor)
            return new GetAnnouncementPoll(poll.AllowMultiple, poll.IsAnonymous, poll.ClosesOn, isClosed, false, false, null, [],
                options.Select(o => new GetAnnouncementPollOption(o.Id.ToString(), o.Label, null, null)).ToList());

        var votes = await db.PollVotes.AsNoTracking().Where(v => v.AnnouncementId == announcement.Id).ToListAsync(ct);
        var me = Guid.Parse(currentUser.AccountId!);
        var showResults = AreResultsVisible(isClosed, audience);
        var names = showResults && CanSeeWhoAnswered(poll, audience) ? await NamesOfAsync(votes, ct) : [];
        return new GetAnnouncementPoll(
            poll.AllowMultiple, poll.IsAnonymous, poll.ClosesOn, isClosed, showResults,
            CanVote: announcement.IsPublished && !isClosed,
            ParticipantCount: votes.Select(v => v.MemberId).Distinct().Count(),
            MyOptionIds: votes.Where(v => v.MemberId == me).Select(v => v.OptionId.ToString()).ToList(),
            Options: options.Select(o => new GetAnnouncementPollOption(
                o.Id.ToString(), o.Label,
                showResults ? votes.Count(v => v.OptionId == o.Id) : null,
                showResults && CanSeeWhoAnswered(poll, audience) ? VotersOf(o, votes, names) : null)).ToList());
    }

    // Members see the results once the poll is closed; the board sees them at any time (the answers can still change before the closing date).
    private static bool AreResultsVisible(bool isClosed, AnnouncementAudience audience) => isClosed || audience == AnnouncementAudience.Board;

    // Names are only shown to the board, and never for an anonymous poll.
    private static bool CanSeeWhoAnswered(AnnouncementPoll poll, AnnouncementAudience audience) => audience == AnnouncementAudience.Board && !poll.IsAnonymous;

    private static List<string> VotersOf(PollOption option, List<PollVote> votes, Dictionary<Guid, string> names) =>
        votes.Where(v => v.OptionId == option.Id).Select(v => names.GetValueOrDefault(v.MemberId, "Ancien membre")).Order().ToList();

    private async Task<Dictionary<Guid, string>> NamesOfAsync(List<PollVote> votes, CancellationToken ct)
    {
        var ids = votes.Select(v => v.MemberId.ToString()).ToHashSet();
        var accounts = await identity.ListAsync(ct);
        return accounts.Where(a => ids.Contains(a.Id)).ToDictionary(a => Guid.Parse(a.Id), a => a.Name);
    }
}
