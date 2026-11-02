using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.ChangePollDeadline;

/// <summary>Moves the closing date of a poll that is still open (earlier or later, not in the past).</summary>
internal sealed class ChangePollDeadlineHandler(IValidator<ChangePollDeadlineRequest> validator, IDateTimeProvider clock, AppDbContext db)
    : IHandler<ChangePollDeadlineRequest>
{
    public async Task<Result> HandleAsync(ChangePollDeadlineRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var poll = await db.AnnouncementPolls.FirstOrDefaultAsync(p => p.AnnouncementId == id, ct);
        if (poll is null)
            return Error.NotFound();

        var today = AnnouncementExpiry.TodayAt(clock.UtcNow);
        if (PollRules.IsClosed(poll.ClosesOn, today))
            return PollRules.ClosedError();

        if (PollRules.CheckClosingDate(request.Body.ClosesOn, today) is { } dateError)
            return dateError;

        var closesOn = request.Body.ClosesOn!.Value;
        poll.ClosesOn = closesOn;
        var announcement = await db.Announcements.FirstAsync(a => a.Id == id, ct);
        if (announcement.ExpiresOn < closesOn)
            announcement.ExpiresOn = closesOn;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
