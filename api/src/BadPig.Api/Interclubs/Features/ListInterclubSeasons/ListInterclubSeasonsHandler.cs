using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.ListInterclubSeasons;

/// <summary>
/// The seasons the site offers as a filter of the interclubs: those that have teams, plus the current season
/// (even without a team yet, since it is the one selected by default).
/// </summary>
internal sealed class ListInterclubSeasonsHandler(IValidator<ListInterclubSeasonsRequest> validator, InterclubAccess access, ISeasonReader seasons, AppDbContext db)
    : IHandler<ListInterclubSeasonsRequest, PagedResponse<ListInterclubSeasonsItem>>
{
    public async Task<Result<PagedResponse<ListInterclubSeasonsItem>>> HandleAsync(ListInterclubSeasonsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var counts = await db.InterclubTeams.AsNoTracking().GroupBy(t => t.SeasonId)
            .Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(g => g.Key, g => g.Count, ct);
        var current = await seasons.CurrentAsync(ct);
        var items = (await seasons.ListAsync(ct))
            .Where(s => counts.ContainsKey(s.Id) || s.Id == current?.Id)
            .Select(s => (Season: s, Item: new ListInterclubSeasonsItem(s.Id.ToString(), s.Name, s.Id == current?.Id, counts.GetValueOrDefault(s.Id))))
            .ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.Season.Name.HasText(term)).ToList();

        var sorted = request.Sort switch
        {
            "name" => items.SortBy(i => i.Season.Name, request.IsDescending(), StringComparer.OrdinalIgnoreCase),
            _ => items.SortBy(i => i.Season.StartsOn, request.IsDescending(true)),
        };
        return sorted.Select(i => i.Item).ToPage(request);
    }
}
