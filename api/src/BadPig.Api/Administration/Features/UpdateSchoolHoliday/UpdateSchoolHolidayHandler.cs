using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.UpdateSchoolHoliday;

/// <summary>Changes the name or the dates of a period of school holidays, within its season.</summary>
internal sealed class UpdateSchoolHolidayHandler(IValidator<UpdateSchoolHolidayRequest> validator, AppDbContext db, IAdministrationAudit audit)
    : IHandler<UpdateSchoolHolidayRequest>
{
    public async Task<Result> HandleAsync(UpdateSchoolHolidayRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var seasonId = Guid.Parse(request.SeasonId);
        var id = Guid.Parse(request.Id);
        var holiday = await db.SchoolHolidays.FirstOrDefaultAsync(h => h.Id == id && h.SeasonId == seasonId, ct);
        if (holiday is null)
            return Error.NotFound();

        var season = await db.Seasons.AsNoTracking().FirstAsync(s => s.Id == seasonId, ct);
        var name = request.Body.Name!.Trim();
        var startsOn = request.Body.StartsOn!.Value;
        var endsOn = request.Body.EndsOn!.Value;
        if (HolidayConflicts.OutsideSeason(season, startsOn, endsOn) is { } outside)
            return Error.Validation("startsOn", outside);

        var others = await db.SchoolHolidays.AsNoTracking().Where(h => h.SeasonId == seasonId && h.Id != id).ToListAsync(ct);
        if (HolidayConflicts.Overlap(others, startsOn, endsOn) is { } overlap)
            return Error.Conflict(overlap);

        holiday.Name = name;
        holiday.StartsOn = startsOn;
        holiday.EndsOn = endsOn;
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync($"holiday.updated.{name}", null, ct);
        return Result.Success();
    }
}
