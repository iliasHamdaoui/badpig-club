using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.DeleteSchoolHoliday;

/// <summary>Removes a period of school holidays: the sessions of those days come back.</summary>
internal sealed class DeleteSchoolHolidayHandler(IValidator<DeleteSchoolHolidayRequest> validator, AppDbContext db, IAdministrationAudit audit)
    : IHandler<DeleteSchoolHolidayRequest>
{
    public async Task<Result> HandleAsync(DeleteSchoolHolidayRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var seasonId = Guid.Parse(request.SeasonId);
        var id = Guid.Parse(request.Id);
        var holiday = await db.SchoolHolidays.FirstOrDefaultAsync(h => h.Id == id && h.SeasonId == seasonId, ct);
        if (holiday is null)
            return Error.NotFound();

        db.SchoolHolidays.Remove(holiday);
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync($"holiday.deleted.{holiday.Name}", null, ct);
        return Result.Success();
    }
}
