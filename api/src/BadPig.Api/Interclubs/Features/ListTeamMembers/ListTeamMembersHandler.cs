using BadPig.Shared.Identity;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.ListTeamMembers;

/// <summary>The players of a team, with their names.</summary>
internal sealed class ListTeamMembersHandler(IValidator<ListTeamMembersRequest> validator, InterclubAccess access, IIdentityProvider identity, AppDbContext db)
    : IHandler<ListTeamMembersRequest, PagedResponse<ListTeamMembersItem>>
{
    public async Task<Result<PagedResponse<ListTeamMembersItem>>> HandleAsync(ListTeamMembersRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var teamId = Guid.Parse(request.Id);
        var team = await db.InterclubTeams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == teamId, ct);
        if (team is null)
            return Error.NotFound();

        var names = (await identity.ListAsync(ct)).ToDictionary(a => a.Id, a => a.Name);
        var members = await db.InterclubTeamMembers.AsNoTracking().Where(m => m.TeamId == teamId).Select(m => m.AccountId).ToListAsync(ct);
        var items = members.Select(id => new ListTeamMembersItem(id.ToString(), names.GetValueOrDefault(id.ToString(), "Compte supprimé"), id == team.CaptainId)).ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.Name.HasText(term)).ToList();

        return items.SortBy(i => i.Name, request.IsDescending(), AlphabeticalComparer.Instance).ToPage(request);
    }
}
