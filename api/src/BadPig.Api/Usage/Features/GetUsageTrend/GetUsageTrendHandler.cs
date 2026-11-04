using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Usage.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Usage.Features.GetUsageTrend;

/// <summary>The views and the time spent for each day of the period, all pages together; the days without any visit are present with zeros so that a chart has no hole.</summary>
internal sealed class GetUsageTrendHandler(IValidator<GetUsageTrendRequest> validator, IDateTimeProvider clock, AppDbContext db)
    : IHandler<GetUsageTrendRequest, GetUsageTrendResponse>
{
    public async Task<Result<GetUsageTrendResponse>> HandleAsync(GetUsageTrendRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var today = ClubTime.Today(clock.UtcNow);
        var since = UsagePeriods.Start(clock.UtcNow, request.Days);
        var perDay = (await db.PageUsages.AsNoTracking()
            .Where(u => u.Day >= since)
            .GroupBy(u => u.Day)
            .Select(g => new { Day = g.Key, Views = g.Sum(u => u.Views), Seconds = g.Sum(u => u.Seconds) })
            .ToListAsync(ct))
            .ToDictionary(d => d.Day);

        var days = Enumerable.Range(0, today.DayNumber - since.DayNumber + 1)
            .Select(offset => since.AddDays(offset))
            .Select(day => perDay.TryGetValue(day, out var total) ? new GetUsageTrendDay(day, total.Views, total.Seconds) : new GetUsageTrendDay(day, 0, 0))
            .ToList();
        return new GetUsageTrendResponse(days);
    }
}
