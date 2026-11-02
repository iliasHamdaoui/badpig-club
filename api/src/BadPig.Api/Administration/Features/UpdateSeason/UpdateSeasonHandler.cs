using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.UpdateSeason;

/// <summary>
/// Changes the dates, the grace period and the fees of a season. Memberships already validated keep the amount
/// they were validated with.
/// </summary>
internal sealed class UpdateSeasonHandler(IValidator<UpdateSeasonRequest> validator, AppDbContext db, IAdministrationAudit audit) : IHandler<UpdateSeasonRequest>
{
    public async Task<Result> HandleAsync(UpdateSeasonRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (season is null)
            return Error.NotFound();

        var body = request.Body;
        var name = body.Name!.Trim();
        var conflict = SeasonConflicts.Find(await db.Seasons.AsNoTracking().Where(s => s.Id != id).ToListAsync(ct), name, body.StartsOn!.Value, body.EndsOn!.Value);
        if (conflict is not null)
            return Error.Conflict(conflict);

        season.Name = name;
        season.StartsOn = body.StartsOn.Value;
        season.EndsOn = body.EndsOn.Value;
        season.GraceDays = body.GraceDays!.Value;
        season.InternalFee = body.InternalFee!.Value;
        season.ExternalFee = body.ExternalFee!.Value;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Error.Conflict($"Une saison « {name} » existe déjà.");
        }

        await audit.RecordAsync($"season.updated.{name}", null, ct);
        return Result.Success();
    }
}
