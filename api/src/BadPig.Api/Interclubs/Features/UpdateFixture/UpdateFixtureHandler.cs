using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.UpdateFixture;

/// <summary>Changes a fixture; only the captain of its team or the board can.</summary>
internal sealed class UpdateFixtureHandler(IValidator<UpdateFixtureRequest> validator, InterclubAccess access, AppDbContext db)
    : IHandler<UpdateFixtureRequest>
{
    public async Task<Result> HandleAsync(UpdateFixtureRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var fixture = await db.InterclubFixtures.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fixture is null)
            return Error.NotFound();

        var team = await db.InterclubTeams.AsNoTracking().FirstAsync(t => t.Id == fixture.TeamId, ct);
        if (!access.CanManageFixtures(team))
            return Error.Forbidden("Réservé au capitaine de l'équipe et au bureau.");

        var body = request.Body;
        fixture.PlayedOn = body.Date!.Value;
        fixture.StartTime = body.StartTime;
        fixture.Opponent = body.Opponent!.Trim();
        fixture.Venue = body.Venue!;
        fixture.Place = string.IsNullOrWhiteSpace(body.Place) ? null : body.Place.Trim();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
