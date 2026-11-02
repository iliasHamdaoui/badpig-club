using BadPig.Administration.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.ListTeams;

/// <summary>The teams of the club, with their season and the name of their captain.</summary>
internal sealed class ListTeamsHandler(IValidator<ListTeamsRequest> validator, InterclubAccess access, IUserContext currentUser, IIdentityProvider identity,
    ISeasonReader seasons, AppDbContext db)
    : IHandler<ListTeamsRequest, PagedResponse<ListTeamsItem>>
{
    public async Task<Result<PagedResponse<ListTeamsItem>>> HandleAsync(ListTeamsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var teams = await db.InterclubTeams.AsNoTracking().ToListAsync(ct);
        if (!string.IsNullOrEmpty(request.SeasonId))
        {
            var seasonId = Guid.Parse(request.SeasonId);
            teams = teams.Where(t => t.SeasonId == seasonId).ToList();
        }

        var seasonsById = (await seasons.ListAsync(ct)).ToDictionary(s => s.Id);
        var counts = await db.InterclubTeamMembers.AsNoTracking().GroupBy(m => m.TeamId).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(g => g.Key, g => g.Count, ct);
        var me = Guid.TryParse(currentUser.AccountId, out var accountId) ? accountId : Guid.Empty;
        var myTeams = (await db.InterclubTeamMembers.AsNoTracking().Where(m => m.AccountId == me).Select(m => m.TeamId).ToListAsync(ct)).ToHashSet();
        var names = (await identity.ListAsync(ct)).ToDictionary(a => a.Id, a => a.Name);
        var items = teams.Select(t => (Team: t, Season: seasonsById.GetValueOrDefault(t.SeasonId), CaptainName: CaptainNameOf(t, names))).ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.Team.Name.HasText(term) || i.CaptainName.HasText(term)).ToList();

        var sorted = request.Sort switch
        {
            "name" => items.SortBy(i => i.Team.Name, request.IsDescending(), StringComparer.OrdinalIgnoreCase).ThenSort(i => StartOf(i.Season)),
            "season" => items.SortBy(i => StartOf(i.Season), request.IsDescending(true)).ThenSort(i => i.Team.Name, StringComparer.OrdinalIgnoreCase),
            _ => items.SortBy(i => StartOf(i.Season), true).ThenSort(i => i.Team.Name, StringComparer.OrdinalIgnoreCase),
        };
        return sorted.Select(i => new ListTeamsItem(i.Team.Id.ToString(), i.Team.Name, i.Team.SeasonId.ToString(), i.Season?.Name ?? "",
            i.Team.CaptainId?.ToString(), i.CaptainName, access.IsCaptainOf(i.Team), counts.GetValueOrDefault(i.Team.Id), myTeams.Contains(i.Team.Id))).ToPage(request);
    }

    /// <summary>A team whose season has disappeared sorts last.</summary>
    private static DateOnly StartOf(SeasonInfo? season) => season?.StartsOn ?? DateOnly.MinValue;

    private static string? CaptainNameOf(Team team, Dictionary<string, string> names) =>
        team.CaptainId is { } id && names.TryGetValue(id.ToString(), out var name) ? name : null;
}
