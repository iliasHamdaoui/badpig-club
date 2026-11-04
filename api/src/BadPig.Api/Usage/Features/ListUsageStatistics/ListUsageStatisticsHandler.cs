using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Usage.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Usage.Features.ListUsageStatistics;

/// <summary>The pages of the website, from the most opened to the least, with the time spent on each over a period ending today (Paris).</summary>
internal sealed class ListUsageStatisticsHandler(IValidator<ListUsageStatisticsRequest> validator, IDateTimeProvider clock, AppDbContext db)
    : IHandler<ListUsageStatisticsRequest, PagedResponse<ListUsageStatisticsItem>>
{
    public async Task<Result<PagedResponse<ListUsageStatisticsItem>>> HandleAsync(ListUsageStatisticsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var since = UsagePeriods.Start(clock.UtcNow, request.Days);
        var totals = await db.PageUsages.AsNoTracking()
            .Where(u => u.Day >= since)
            .GroupBy(u => u.Path)
            .Select(g => new { Path = g.Key, Views = g.Sum(u => u.Views), Seconds = g.Sum(u => u.Seconds) })
            .ToListAsync(ct);

        var items = totals
            .Select(t => new ListUsageStatisticsItem(t.Path, t.Views, t.Seconds, t.Views > 0 ? (int)(t.Seconds / t.Views) : 0))
            .ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.Path.HasText(term)).ToList();

        var descending = request.IsDescending(defaultDescending: request.Sort is null or "views" or "seconds" or "average");
        var sorted = request.Sort switch
        {
            "path" => items.SortBy(i => i.Path, descending, AlphabeticalComparer.Instance),
            "seconds" => items.SortBy(i => i.TotalSeconds, descending).ThenSort(i => i.Path, AlphabeticalComparer.Instance),
            "average" => items.SortBy(i => i.AverageSeconds, descending).ThenSort(i => i.Path, AlphabeticalComparer.Instance),
            _ => items.SortBy(i => i.Views, descending).ThenSort(i => i.Path, AlphabeticalComparer.Instance),
        };
        return sorted.ToPage(request);
    }
}
