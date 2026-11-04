using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Usage.Features.RecordPageVisit;

/// <summary>
/// Adds what a person did with a page to the totals of the day. The person is not recorded: there is only a counter per day and per page,
/// so the statistics cannot tell who looked at what. The addition is made by the database in one statement, so two reports never overwrite each other.
/// </summary>
internal sealed class RecordPageVisitHandler(IValidator<RecordPageVisitRequest> validator, IDateTimeProvider clock, AppDbContext db)
    : IHandler<RecordPageVisitRequest>
{
    public async Task<Result> HandleAsync(RecordPageVisitRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var day = ClubTime.Today(clock.UtcNow);
        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO page_usage (day, path, views, seconds) VALUES ({day}, {request.Path!}, {request.Views}, {(long)request.Seconds})
            ON CONFLICT (day, path) DO UPDATE SET views = page_usage.views + EXCLUDED.views, seconds = page_usage.seconds + EXCLUDED.seconds
            """, ct);
        return Result.Success();
    }
}
