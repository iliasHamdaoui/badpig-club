using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.CreateSchoolHoliday;

/// <summary>Adds a period of school holidays to a season: inside the season and without overlapping another period.</summary>
internal sealed class CreateSchoolHolidayHandler(IValidator<CreateSchoolHolidayRequest> validator, IDateTimeProvider clock, AppDbContext db, IAdministrationAudit audit)
    : IHandler<CreateSchoolHolidayRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreateSchoolHolidayRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var seasonId = Guid.Parse(request.SeasonId);
        var season = await db.Seasons.AsNoTracking().FirstOrDefaultAsync(s => s.Id == seasonId, ct);
        if (season is null)
            return Error.NotFound();

        var name = request.Body.Name!.Trim();
        var startsOn = request.Body.StartsOn!.Value;
        var endsOn = request.Body.EndsOn!.Value;
        if (HolidayConflicts.OutsideSeason(season, startsOn, endsOn) is { } outside)
            return Error.Validation("startsOn", outside);

        var others = await db.SchoolHolidays.AsNoTracking().Where(h => h.SeasonId == seasonId).ToListAsync(ct);
        if (HolidayConflicts.Overlap(others, startsOn, endsOn) is { } overlap)
            return Error.Conflict(overlap);

        var holiday = new SchoolHoliday { Id = Guid.NewGuid(), SeasonId = seasonId, Name = name, StartsOn = startsOn, EndsOn = endsOn, CreatedAt = clock.UtcNow };
        db.SchoolHolidays.Add(holiday);
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync($"holiday.created.{name}", null, ct);
        return new CreatedResponse(holiday.Id.ToString());
    }
}
