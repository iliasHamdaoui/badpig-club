using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.ChooseProposedDate;

/// <summary>Creates the fixture from the chosen date and closes the proposal.</summary>
internal sealed class ChooseProposedDateHandler(IValidator<ChooseProposedDateRequest> validator, InterclubAccess access, IDateTimeProvider clock, AppDbContext db)
    : IHandler<ChooseProposedDateRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(ChooseProposedDateRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var proposal = await db.InterclubDateProposals.Include(p => p.Dates).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proposal is null)
            return Error.NotFound();

        var team = await db.InterclubTeams.AsNoTracking().FirstAsync(t => t.Id == proposal.TeamId, ct);
        if (!access.CanManageFixtures(team))
            return Error.Forbidden("Réservé au capitaine de l'équipe et au bureau.");

        if (!proposal.IsOpen)
            return Error.Conflict("Une date a déjà été choisie pour cette rencontre.");

        var dateId = Guid.Parse(request.Body.DateId!);
        var date = proposal.Dates.FirstOrDefault(d => d.Id == dateId);
        if (date is null)
            return Error.Validation("dateId", "Cette date ne fait pas partie de la proposition.");

        var fixture = new Fixture
        {
            Id = Guid.NewGuid(),
            TeamId = proposal.TeamId,
            PlayedOn = date.PlayedOn,
            StartTime = date.StartTime,
            Opponent = proposal.Opponent,
            Venue = proposal.Venue,
            Place = proposal.Place,
            CreatedAt = clock.UtcNow,
        };
        db.InterclubFixtures.Add(fixture);
        proposal.FixtureId = fixture.Id;
        await db.SaveChangesAsync(ct);
        return new CreatedResponse(fixture.Id.ToString());
    }
}
