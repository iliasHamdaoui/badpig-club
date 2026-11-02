using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.ListDateProposals;

/// <summary>The proposals of dates, with the count of the answers.</summary>
internal sealed class ListDateProposalsHandler(IValidator<ListDateProposalsRequest> validator, InterclubAccess access, IUserContext currentUser, AppDbContext db)
    : IHandler<ListDateProposalsRequest, PagedResponse<ListDateProposalsItem>>
{
    public async Task<Result<PagedResponse<ListDateProposalsItem>>> HandleAsync(ListDateProposalsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var query = db.InterclubDateProposals.AsNoTracking().Include(p => p.Dates).ThenInclude(d => d.Answers).AsQueryable();
        if (!string.IsNullOrEmpty(request.TeamId))
        {
            var teamId = Guid.Parse(request.TeamId);
            query = query.Where(p => p.TeamId == teamId);
        }

        if (request.Status == ProposalStatuses.Open)
            query = query.Where(p => p.FixtureId == null);
        else if (request.Status == ProposalStatuses.Chosen)
            query = query.Where(p => p.FixtureId != null);

        var teams = await db.InterclubTeams.AsNoTracking().ToDictionaryAsync(t => t.Id, ct);
        var me = Guid.TryParse(currentUser.AccountId, out var accountId) ? accountId : Guid.Empty;
        var myTeams = (await db.InterclubTeamMembers.AsNoTracking().Where(m => m.AccountId == me).Select(m => m.TeamId).ToListAsync(ct)).ToHashSet();
        var items = (await query.ToListAsync(ct)).Where(p => teams.ContainsKey(p.TeamId)).Select(p => ToItem(p, teams[p.TeamId], myTeams.Contains(p.TeamId))).ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.TeamName.HasText(term) || i.Opponent.HasText(term)).ToList();

        var sorted = request.Sort switch
        {
            "team" => items.SortBy(i => i.TeamName, request.IsDescending(), StringComparer.OrdinalIgnoreCase).ThenSort(i => i.CreatedAt),
            _ => items.SortBy(i => i.CreatedAt, request.IsDescending(true)),
        };
        return sorted.ToPage(request);
    }

    private ListDateProposalsItem ToItem(DateProposal proposal, Team team, bool isPlayer) => new(proposal.Id.ToString(), team.Id.ToString(), team.Name, proposal.Opponent,
        proposal.Venue, proposal.Place, proposal.IsOpen, proposal.FixtureId?.ToString(), proposal.IsOpen && isPlayer, access.CanManageFixtures(team), proposal.CreatedAt,
        proposal.Dates.OrderBy(d => d.PlayedOn).ThenBy(d => d.StartTime).Select(ToDate).ToList());

    private ListDateProposalsDate ToDate(ProposedDate date) => new(date.Id.ToString(), date.PlayedOn, date.StartTime,
        date.Answers.Count(a => a.Answer == DateAnswers.Yes), date.Answers.Count(a => a.Answer == DateAnswers.Maybe), date.Answers.Count(a => a.Answer == DateAnswers.No),
        date.Answers.FirstOrDefault(a => a.AccountId.ToString() == currentUser.AccountId)?.Answer);
}
