using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ListSeasons;

/// <summary>The seasons (there is about one per year), searched, sorted and paginated.</summary>
internal sealed class ListSeasonsHandler(IValidator<ListSeasonsRequest> validator, IDateTimeProvider clock, AppDbContext db)
    : IHandler<ListSeasonsRequest, PagedResponse<ListSeasonsItem>>
{
    public async Task<Result<PagedResponse<ListSeasonsItem>>> HandleAsync(ListSeasonsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var seasons = await db.Seasons.AsNoTracking().ToListAsync(ct);
        var current = SeasonRules.Current(seasons, SeasonRules.Today(clock.UtcNow));

        IEnumerable<Season> rows = seasons;
        if (request.SearchTerm() is { } term)
            rows = rows.Where(s => s.Name.HasText(term));

        var text = AlphabeticalComparer.Instance;
        var desc = request.IsDescending(defaultDescending: request.Sort is null or "");
        IOrderedEnumerable<Season>? sorted = request.Sort?.ToLowerInvariant() switch
        {
            null or "" or "startson" => rows.SortBy(s => s.StartsOn, desc),
            "name" => rows.SortBy(s => s.Name, desc, text),
            _ => null,
        };
        if (sorted is null)
            return Error.Validation("sort", "Tri invalide.");

        return sorted.ThenSort(s => s.Name, text)
            .Select(s => new ListSeasonsItem(s.Id.ToString(), s.Name, s.StartsOn, s.EndsOn, s.GraceDays, s.InternalFee, s.ExternalFee, s.Id == current?.Id))
            .ToPage(request);
    }
}
