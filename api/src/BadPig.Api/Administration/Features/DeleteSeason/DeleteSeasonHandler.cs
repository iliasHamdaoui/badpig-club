using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Interclubs.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.DeleteSeason;

/// <summary>Deletes a season that has no validated membership (otherwise the statistics would lose data) and no interclub team.</summary>
internal sealed class DeleteSeasonHandler(IValidator<DeleteSeasonRequest> validator, AppDbContext db, ISeasonTeams teams, IAdministrationAudit audit) : IHandler<DeleteSeasonRequest>
{
    public async Task<Result> HandleAsync(DeleteSeasonRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var season = await db.Seasons.FirstOrDefaultAsync(s => s.Id == id, ct);
        if (season is null)
            return Error.NotFound();

        if (await db.Memberships.AnyAsync(m => m.SeasonId == id, ct))
            return Error.Conflict("Cette saison a des adhésions validées : elle ne peut pas être supprimée.");

        if (await teams.ExistInAsync(id, ct))
            return Error.Conflict("Cette saison a des équipes d'interclubs : elle ne peut pas être supprimée.");

        db.Seasons.Remove(season);
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync($"season.deleted.{season.Name}", null, ct);
        return Result.Success();
    }
}
