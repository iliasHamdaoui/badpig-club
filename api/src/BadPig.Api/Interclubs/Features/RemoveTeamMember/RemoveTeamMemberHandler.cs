using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.RemoveTeamMember;

/// <summary>Removes a player from a team and withdraws their answers to the proposals still open.</summary>
internal sealed class RemoveTeamMemberHandler(IValidator<RemoveTeamMemberRequest> validator, InterclubAccess access, AppDbContext db)
    : IHandler<RemoveTeamMemberRequest>
{
    public async Task<Result> HandleAsync(RemoveTeamMemberRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var teamId = Guid.Parse(request.Id);
        var team = await db.InterclubTeams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == teamId, ct);
        if (team is null)
            return Error.NotFound();

        if (!access.CanManageFixtures(team))
            return Error.Forbidden("Réservé au capitaine de l'équipe et au bureau.");

        var accountId = Guid.Parse(request.AccountId);
        var member = await db.InterclubTeamMembers.FirstOrDefaultAsync(m => m.TeamId == teamId && m.AccountId == accountId, ct);
        if (member is null)
            return Error.NotFound();

        var openDates = db.InterclubDateProposals.Where(p => p.TeamId == teamId && p.FixtureId == null).SelectMany(p => p.Dates).Select(d => d.Id);
        await db.InterclubDateAnswers.Where(a => a.AccountId == accountId && openDates.Contains(a.ProposedDateId)).ExecuteDeleteAsync(ct);
        db.InterclubTeamMembers.Remove(member);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
