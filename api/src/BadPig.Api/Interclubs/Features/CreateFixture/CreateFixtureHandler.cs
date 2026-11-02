using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.CreateFixture;

/// <summary>Adds a fixture to a team; only its captain or the board can.</summary>
internal sealed class CreateFixtureHandler(IValidator<CreateFixtureRequest> validator, InterclubAccess access, IDateTimeProvider clock, AppDbContext db)
    : IHandler<CreateFixtureRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreateFixtureRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var teamId = Guid.Parse(request.TeamId);
        var team = await db.InterclubTeams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == teamId, ct);
        if (team is null)
            return Error.NotFound();

        if (!access.CanManageFixtures(team))
            return Error.Forbidden("Réservé au capitaine de l'équipe et au bureau.");

        var body = request.Body;
        var fixture = new Fixture
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            PlayedOn = body.Date!.Value,
            StartTime = body.StartTime,
            Opponent = body.Opponent!.Trim(),
            Venue = body.Venue!,
            Place = string.IsNullOrWhiteSpace(body.Place) ? null : body.Place.Trim(),
            CreatedAt = clock.UtcNow,
        };
        db.InterclubFixtures.Add(fixture);
        await db.SaveChangesAsync(ct);
        return new CreatedResponse(fixture.Id.ToString());
    }
}
