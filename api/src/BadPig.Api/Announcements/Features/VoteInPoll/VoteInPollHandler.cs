using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.VoteInPoll;

/// <summary>A club member answers the poll of a published announcement they can read; a new answer replaces the previous one until the poll closes.</summary>
internal sealed class VoteInPollHandler(IValidator<VoteInPollRequest> validator, IDateTimeProvider clock, IUserContext currentUser,
    AnnouncementViewer viewer, AppDbContext db) : IHandler<VoteInPollRequest>
{
    public async Task<Result> HandleAsync(VoteInPollRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        if (audience == AnnouncementAudience.Visitor)
            return Error.Forbidden("Réservé aux adhérents.");

        var id = Guid.Parse(request.Id);
        var poll = await FindPollOfReadableAnnouncementAsync(id, audience, ct);
        if (poll is null)
            return Error.NotFound();

        if (PollRules.IsClosed(poll.ClosesOn, viewer.Today))
            return PollRules.ClosedError();

        var chosen = request.Body.OptionIds!.Select(Guid.Parse).ToList();
        if (!AreOptionsOfThePoll(poll, chosen))
            return Error.Validation("optionIds", "Choix inconnu.");

        if (!poll.AllowMultiple && chosen.Count != 1)
            return Error.Validation("optionIds", "Ce sondage n'accepte qu'une seule réponse.");

        var member = Guid.Parse(currentUser.AccountId!);
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            await db.PollVotes.Where(v => v.AnnouncementId == id && v.MemberId == member).ExecuteDeleteAsync(ct);
            db.PollVotes.AddRange(chosen.Select(option => new PollVote { OptionId = option, AnnouncementId = id, MemberId = member, VotedAt = clock.UtcNow }));
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Two answers of the same member at the same time (primary key).
            return Error.Conflict("Votre réponse a été enregistrée en même temps depuis un autre écran : recommencez.");
        }

        return Result.Success();
    }

    private static bool AreOptionsOfThePoll(AnnouncementPoll poll, List<Guid> chosen) => chosen.All(c => poll.Options.Any(o => o.Id == c));

    private async Task<AnnouncementPoll?> FindPollOfReadableAnnouncementAsync(Guid id, AnnouncementAudience audience, CancellationToken ct)
    {
        var isReadable = await db.Announcements.VisibleTo(audience, viewer.Today).AnyAsync(a => a.Id == id && a.IsPublished, ct);
        return isReadable ? await db.AnnouncementPolls.Include(p => p.Options).FirstOrDefaultAsync(p => p.AnnouncementId == id, ct) : null;
    }
}
