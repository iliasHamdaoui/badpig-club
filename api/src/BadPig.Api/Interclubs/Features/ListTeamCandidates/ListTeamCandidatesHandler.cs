using BadPig.Shared.Identity;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Administration.Core;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.ListTeamCandidates;

/// <summary>The active club members who are not yet in the team; only its captain or the board can ask.</summary>
internal sealed class ListTeamCandidatesHandler(IValidator<ListTeamCandidatesRequest> validator, InterclubAccess access, IIdentityProvider identity,
    IMembershipReader memberships, AppDbContext db)
    : IHandler<ListTeamCandidatesRequest, PagedResponse<ListTeamCandidatesItem>>
{
    public async Task<Result<PagedResponse<ListTeamCandidatesItem>>> HandleAsync(ListTeamCandidatesRequest request, CancellationToken ct)
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

        // A player belongs to a single team for a given season: whoever already plays in one of them is not a candidate.
        var alreadyPlaying = (await db.InterclubTeamMembers.AsNoTracking()
            .Join(db.InterclubTeams, m => m.TeamId, t => t.Id, (m, t) => new { m.AccountId, t.SeasonId })
            .Where(x => x.SeasonId == team.SeasonId)
            .Select(x => x.AccountId)
            .ToListAsync(ct)).Select(id => id.ToString()).ToHashSet();
        var statuses = await memberships.GetAllAsync(ct);
        var items = (await identity.ListAsync(ct))
            .Where(a => AccountRules.IsClubMember(a) && a.IsActive && !alreadyPlaying.Contains(a.Id)
                && TeamRules.CanPlayInterclubs(statuses.GetValueOrDefault(a.Id)))
            .Select(a => new ListTeamCandidatesItem(a.Id, a.Name))
            .ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.Name.HasText(term)).ToList();

        return items.SortBy(i => i.Name, request.IsDescending(), AlphabeticalComparer.Instance).ToPage(request);
    }
}
