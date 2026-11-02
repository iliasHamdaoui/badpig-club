using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ListClubSessions;

/// <summary>
/// When the club plays, for everybody (a visitor as well as a member): the weekly schedule, whether there is a session
/// today, and the next ones. The school holidays of the seasons close the club; everything is computed in Paris time.
/// </summary>
internal sealed class ListClubSessionsHandler(IValidator<ListClubSessionsRequest> validator, IDateTimeProvider clock, AppDbContext db)
    : IHandler<ListClubSessionsRequest, ListClubSessionsResponse>
{
    private const int DefaultCount = 3;

    public async Task<Result<ListClubSessionsResponse>> HandleAsync(ListClubSessionsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var today = ClubTime.Today(clock.UtcNow);
        // All the holidays, whatever the season: what matters is the day, and there are only a few of them.
        var holidays = await db.SchoolHolidays.AsNoTracking().ToListAsync(ct);
        var closure = SessionRules.HolidayOn(holidays, today);
        var next = SessionRules.Next(holidays, today, request.Count ?? DefaultCount);
        var weekly = ClubSessions.Weekly.Select(s => new ClubSessionDay(s.Day.ToString().ToLowerInvariant(), s.StartTime, s.EndTime)).ToList();
        return new ListClubSessionsResponse(
            today,
            weekly,
            SessionRules.IsOpenOn(holidays, today),
            closure is null ? null : new ClubClosure(closure.Name, closure.EndsOn),
            next.Select(s => new ClubSessionToCome(s.Date, s.StartTime, s.EndTime, s.Date == today)).ToList());
    }
}
