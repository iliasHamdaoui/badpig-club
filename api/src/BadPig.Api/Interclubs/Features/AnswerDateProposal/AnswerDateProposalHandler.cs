using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.AnswerDateProposal;

/// <summary>Records or replaces the answers of the signed-in person, a player of the team, while the proposal is open.</summary>
internal sealed class AnswerDateProposalHandler(IValidator<AnswerDateProposalRequest> validator, InterclubAccess access, IUserContext currentUser, IDateTimeProvider clock, AppDbContext db)
    : IHandler<AnswerDateProposalRequest>
{
    public async Task<Result> HandleAsync(AnswerDateProposalRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var id = Guid.Parse(request.Id);
        var proposal = await db.InterclubDateProposals.Include(p => p.Dates).ThenInclude(d => d.Answers).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proposal is null)
            return Error.NotFound();

        var me = Guid.Parse(currentUser.AccountId!);
        if (!await db.InterclubTeamMembers.AnyAsync(m => m.TeamId == proposal.TeamId && m.AccountId == me, ct))
            return Error.Forbidden("Réservé aux joueurs de l'équipe.");

        if (!proposal.IsOpen)
            return Error.Conflict("Une date a déjà été choisie pour cette rencontre.");

        foreach (var entry in request.Body.Answers!)
        {
            var date = proposal.Dates.FirstOrDefault(d => d.Id == Guid.Parse(entry.DateId!));
            if (date is null)
                return Error.Validation("answers", "Cette date ne fait pas partie de la proposition.");

            var existing = date.Answers.FirstOrDefault(a => a.AccountId == me);
            if (existing is null)
            {
                db.InterclubDateAnswers.Add(new DateAnswer { ProposedDateId = date.Id, AccountId = me, Answer = entry.Answer!, AnsweredAt = clock.UtcNow });
            }
            else
            {
                existing.Answer = entry.Answer!;
                existing.AnsweredAt = clock.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
