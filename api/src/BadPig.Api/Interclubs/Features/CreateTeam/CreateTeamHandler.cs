using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.CreateTeam;

/// <summary>Creates a team without captain, in a season of the club; the name is unique within a season.</summary>
internal sealed class CreateTeamHandler(IValidator<CreateTeamRequest> validator, ISeasonReader seasons, IDateTimeProvider clock, AppDbContext db)
    : IHandler<CreateTeamRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreateTeamRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var seasonId = Guid.Parse(request.SeasonId!);
        if (await seasons.FindAsync(seasonId, ct) is null)
            return Error.NotFound("Saison introuvable.");

        var name = request.Name!.Trim();
        if (await db.InterclubTeams.AnyAsync(t => t.SeasonId == seasonId && t.Name.ToLower() == name.ToLower(), ct))
            return Error.Conflict("Cette équipe existe déjà pour cette saison.");

        var team = new Team { Id = Guid.NewGuid(), Name = name, SeasonId = seasonId, CreatedAt = clock.UtcNow };
        db.InterclubTeams.Add(team);
        await db.SaveChangesAsync(ct);
        return new CreatedResponse(team.Id.ToString());
    }
}
