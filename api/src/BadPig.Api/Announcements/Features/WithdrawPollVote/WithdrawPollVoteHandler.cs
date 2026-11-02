using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.WithdrawPollVote;

/// <summary>Removes the answer of the signed-in member to a poll that is still open.</summary>
internal sealed class WithdrawPollVoteHandler(IValidator<WithdrawPollVoteRequest> validator, IUserContext currentUser,
    AnnouncementViewer viewer, AppDbContext db) : IHandler<WithdrawPollVoteRequest>
{
    public async Task<Result> HandleAsync(WithdrawPollVoteRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        if (audience == AnnouncementAudience.Visitor)
            return Error.Forbidden("Réservé aux adhérents.");

        var id = Guid.Parse(request.Id);
        var isReadable = await db.Announcements.VisibleTo(audience, viewer.Today).AnyAsync(a => a.Id == id && a.IsPublished, ct);
        var poll = isReadable ? await db.AnnouncementPolls.AsNoTracking().FirstOrDefaultAsync(p => p.AnnouncementId == id, ct) : null;
        if (poll is null)
            return Error.NotFound();

        if (PollRules.IsClosed(poll.ClosesOn, viewer.Today))
            return PollRules.ClosedError();

        var member = Guid.Parse(currentUser.AccountId!);
        await db.PollVotes.Where(v => v.AnnouncementId == id && v.MemberId == member).ExecuteDeleteAsync(ct);
        return Result.Success();
    }
}
