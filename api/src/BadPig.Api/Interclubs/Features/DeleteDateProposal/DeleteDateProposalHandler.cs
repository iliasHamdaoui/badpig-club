using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.DeleteDateProposal;

/// <summary>Deletes a proposal (its dates and answers go with it); only the captain of its team or the board can.</summary>
internal sealed class DeleteDateProposalHandler(IValidator<DeleteDateProposalRequest> validator, InterclubAccess access, AppDbContext db)
    : IHandler<DeleteDateProposalRequest>
{
    public async Task<Result> HandleAsync(DeleteDateProposalRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var proposal = await db.InterclubDateProposals.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proposal is null)
            return Error.NotFound();

        var team = await db.InterclubTeams.AsNoTracking().FirstAsync(t => t.Id == proposal.TeamId, ct);
        if (!access.CanManageFixtures(team))
            return Error.Forbidden("Réservé au capitaine de l'équipe et au bureau.");

        db.InterclubDateProposals.Remove(proposal);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
