using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ListSchoolHolidays;

/// <summary>The school holidays of a season, the earliest first.</summary>
internal sealed class ListSchoolHolidaysHandler(IValidator<ListSchoolHolidaysRequest> validator, AppDbContext db)
    : IHandler<ListSchoolHolidaysRequest, PagedResponse<ListSchoolHolidaysItem>>
{
    public async Task<Result<PagedResponse<ListSchoolHolidaysItem>>> HandleAsync(ListSchoolHolidaysRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var seasonId = Guid.Parse(request.SeasonId);
        if (!await db.Seasons.AsNoTracking().AnyAsync(s => s.Id == seasonId, ct))
            return Error.NotFound();

        var holidays = await db.SchoolHolidays.AsNoTracking().Where(h => h.SeasonId == seasonId).ToListAsync(ct);
        var term = request.SearchTerm();
        if (term is not null)
            holidays = holidays.Where(h => h.Name.HasText(term)).ToList();

        var sorted = request.Sort switch
        {
            "name" => holidays.SortBy(h => h.Name, request.IsDescending(), AlphabeticalComparer.Instance),
            _ => holidays.SortBy(h => h.StartsOn, request.IsDescending()),
        };
        return sorted.Select(h => new ListSchoolHolidaysItem(h.Id.ToString(), h.Name, h.StartsOn, h.EndsOn)).ToPage(request);
    }
}
