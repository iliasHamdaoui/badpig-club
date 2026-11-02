using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.CreateDateProposal;

/// <summary>Offers dates for the next fixture of a team; only its captain or the board can.</summary>
internal sealed class CreateDateProposalHandler(IValidator<CreateDateProposalRequest> validator, InterclubAccess access, IDateTimeProvider clock, AppDbContext db)
    : IHandler<CreateDateProposalRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreateDateProposalRequest request, CancellationToken ct)
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
        var proposal = new DateProposal
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            Opponent = body.Opponent!.Trim(),
            Venue = body.Venue!,
            Place = string.IsNullOrWhiteSpace(body.Place) ? null : body.Place.Trim(),
            CreatedAt = clock.UtcNow,
            Dates = body.Dates!.Select(d => new ProposedDate { Id = Guid.NewGuid(), PlayedOn = d.Date!.Value, StartTime = d.StartTime }).ToList(),
        };
        db.InterclubDateProposals.Add(proposal);
        await db.SaveChangesAsync(ct);
        return new CreatedResponse(proposal.Id.ToString());
    }
}
