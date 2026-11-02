using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.UpdateTeam;

/// <summary>Renames a team or moves it to another season of the club.</summary>
internal sealed class UpdateTeamHandler(IValidator<UpdateTeamRequest> validator, ISeasonReader seasons, AppDbContext db)
    : IHandler<UpdateTeamRequest>
{
    public async Task<Result> HandleAsync(UpdateTeamRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var team = await db.InterclubTeams.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (team is null)
            return Error.NotFound();

        var seasonId = Guid.Parse(request.Body.SeasonId!);
        if (await seasons.FindAsync(seasonId, ct) is null)
            return Error.NotFound("Saison introuvable.");

        var name = request.Body.Name!.Trim();
        if (await db.InterclubTeams.AnyAsync(t => t.Id != id && t.SeasonId == seasonId && t.Name.ToLower() == name.ToLower(), ct))
            return Error.Conflict("Cette équipe existe déjà pour cette saison.");

        team.Name = name;
        team.SeasonId = seasonId;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
