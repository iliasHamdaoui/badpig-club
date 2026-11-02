using BadPig.Shared.Identity;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.GetDateProposal;

/// <summary>One proposal; the names of those who answered are only given to who manages it.</summary>
internal sealed class GetDateProposalHandler(IValidator<GetDateProposalRequest> validator, InterclubAccess access, IUserContext currentUser, IIdentityProvider identity, AppDbContext db)
    : IHandler<GetDateProposalRequest, GetDateProposalResponse>
{
    public async Task<Result<GetDateProposalResponse>> HandleAsync(GetDateProposalRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var id = Guid.Parse(request.Id);
        var proposal = await db.InterclubDateProposals.AsNoTracking().Include(p => p.Dates).ThenInclude(d => d.Answers).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (proposal is null)
            return Error.NotFound();

        var team = await db.InterclubTeams.AsNoTracking().FirstAsync(t => t.Id == proposal.TeamId, ct);
        var canManage = access.CanManageFixtures(team);
        var me = Guid.TryParse(currentUser.AccountId, out var accountId) ? accountId : Guid.Empty;
        var isPlayer = await db.InterclubTeamMembers.AnyAsync(m => m.TeamId == team.Id && m.AccountId == me, ct);
        var names = canManage ? (await identity.ListAsync(ct)).ToDictionary(a => a.Id, a => a.Name) : [];

        var dates = proposal.Dates.OrderBy(d => d.PlayedOn).ThenBy(d => d.StartTime).Select(d => new GetDateProposalDate(d.Id.ToString(), d.PlayedOn, d.StartTime,
            d.Answers.Count(a => a.Answer == DateAnswers.Yes), d.Answers.Count(a => a.Answer == DateAnswers.Maybe), d.Answers.Count(a => a.Answer == DateAnswers.No),
            d.Answers.FirstOrDefault(a => a.AccountId.ToString() == currentUser.AccountId)?.Answer,
            canManage ? d.Answers.Select(a => new GetDateProposalAnswer(a.AccountId.ToString(), NameOf(a, names), a.Answer)).OrderBy(a => a.Name, AlphabeticalComparer.Instance).ToList() : [])).ToList();
        return new GetDateProposalResponse(proposal.Id.ToString(), team.Id.ToString(), team.Name, proposal.Opponent, proposal.Venue, proposal.Place, proposal.IsOpen,
            proposal.FixtureId?.ToString(), proposal.IsOpen && isPlayer, canManage, dates);
    }

    private static string NameOf(DateAnswer answer, Dictionary<string, string> names) =>
        names.TryGetValue(answer.AccountId.ToString(), out var name) ? name : "Compte supprimé";
}
