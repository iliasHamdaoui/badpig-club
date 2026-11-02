using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ListSeasonStatistics;

/// <summary>Per season: number of validated memberships and fees collected, by kind of membership.</summary>
internal sealed class ListSeasonStatisticsHandler(IValidator<ListSeasonStatisticsRequest> validator, AppDbContext db)
    : IHandler<ListSeasonStatisticsRequest, PagedResponse<ListSeasonStatisticsItem>>
{
    public async Task<Result<PagedResponse<ListSeasonStatisticsItem>>> HandleAsync(ListSeasonStatisticsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var totals = await db.Memberships.AsNoTracking()
            .GroupBy(m => new { m.SeasonId, m.Kind })
            .Select(g => new { g.Key.SeasonId, g.Key.Kind, Count = g.Count(), Revenue = g.Sum(m => m.Amount) })
            .ToListAsync(ct);

        var rows = (await db.Seasons.AsNoTracking().ToListAsync(ct)).Select(s =>
        {
            var mine = totals.Where(t => t.SeasonId == s.Id).ToList();
            var internalRow = mine.FirstOrDefault(t => t.Kind == MembershipKinds.Internal);
            var externalRow = mine.FirstOrDefault(t => t.Kind == MembershipKinds.External);
            return new ListSeasonStatisticsItem(
                s.Id.ToString(), s.Name, s.StartsOn, s.EndsOn,
                internalRow?.Count ?? 0, externalRow?.Count ?? 0, (internalRow?.Count ?? 0) + (externalRow?.Count ?? 0),
                internalRow?.Revenue ?? 0, externalRow?.Revenue ?? 0, (internalRow?.Revenue ?? 0) + (externalRow?.Revenue ?? 0));
        });

        if (request.SearchTerm() is { } term)
            rows = rows.Where(r => r.Name.HasText(term));

        var text = AlphabeticalComparer.Instance;
        var desc = request.IsDescending(defaultDescending: request.Sort is null or "");
        IOrderedEnumerable<ListSeasonStatisticsItem>? sorted = request.Sort?.ToLowerInvariant() switch
        {
            null or "" or "startson" => rows.SortBy(r => r.StartsOn, desc),
            "name" => rows.SortBy(r => r.Name, desc, text),
            "totalcount" => rows.SortBy(r => r.TotalCount, desc),
            "totalrevenue" => rows.SortBy(r => r.TotalRevenue, desc),
            _ => null,
        };
        if (sorted is null)
            return Error.Validation("sort", "Tri invalide.");

        return sorted.ThenSort(r => r.Name, text).ToPage(request);
    }
}
