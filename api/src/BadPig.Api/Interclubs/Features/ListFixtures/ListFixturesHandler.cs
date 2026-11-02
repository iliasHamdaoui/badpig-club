using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.ListFixtures;

/// <summary>The calendar of the fixtures, with the name of their team.</summary>
internal sealed class ListFixturesHandler(IValidator<ListFixturesRequest> validator, InterclubAccess access, IDateTimeProvider clock, AppDbContext db)
    : IHandler<ListFixturesRequest, PagedResponse<ListFixturesItem>>
{
    public async Task<Result<PagedResponse<ListFixturesItem>>> HandleAsync(ListFixturesRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var today = SeasonRules.Today(clock.UtcNow);
        var timeOfDay = ClubTime.TimeOfDay(clock.UtcNow);
        var query = db.InterclubFixtures.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(request.TeamId))
        {
            var teamId = Guid.Parse(request.TeamId);
            query = query.Where(f => f.TeamId == teamId);
        }

        if (!string.IsNullOrEmpty(request.SeasonId))
        {
            var seasonId = Guid.Parse(request.SeasonId);
            query = query.Where(f => db.InterclubTeams.Any(t => t.Id == f.TeamId && t.SeasonId == seasonId));
        }

        if (request.When == FixturePeriods.Upcoming)
            query = query.Where(f => f.PlayedOn >= today);
        else if (request.When == FixturePeriods.Past)
            query = query.Where(f => f.PlayedOn < today);
        else if (request.When == FixturePeriods.Live)
            query = query.Where(f => f.PlayedOn == today);

        var rows = await query.Join(db.InterclubTeams, f => f.TeamId, t => t.Id, (f, t) => new { Fixture = f, TeamName = t.Name }).ToListAsync(ct);
        var fixtureIds = rows.Select(r => r.Fixture.Id).ToList();
        var matches = (await db.InterclubFixtureMatches.AsNoTracking().Where(m => fixtureIds.Contains(m.FixtureId)).ToListAsync(ct)).ToLookup(m => m.FixtureId);
        var items = rows.Select(r => ToItem(r.Fixture, r.TeamName, matches[r.Fixture.Id].ToList(), today, timeOfDay)).ToList();
        if (request.When == FixturePeriods.Live)
            items = items.Where(i => i.IsLive).ToList();

        var term = request.SearchTerm();
        if (term is not null)
            items = items.Where(i => i.Opponent.HasText(term) || i.TeamName.HasText(term)).ToList();

        var descending = request.IsDescending(request.When == FixturePeriods.Past);
        var sorted = request.Sort switch
        {
            "team" => items.SortBy(i => i.TeamName, descending, StringComparer.OrdinalIgnoreCase).ThenSort(i => i.Date),
            "opponent" => items.SortBy(i => i.Opponent, descending, StringComparer.OrdinalIgnoreCase).ThenSort(i => i.Date),
            _ => items.SortBy(i => i.Date, descending).ThenSort(i => i.StartTime ?? TimeOnly.MinValue).ThenSort(i => i.TeamName, StringComparer.OrdinalIgnoreCase),
        };
        return sorted.ToPage(request);
    }

    private static ListFixturesItem ToItem(Fixture fixture, string teamName, List<FixtureMatch> matches, DateOnly today, TimeOnly timeOfDay)
    {
        var score = MatchScore.Of(matches);
        var decided = matches.Where(m => m.Outcome is not null).ToList();
        var won = decided.Count(m => m.Outcome == MatchOutcomes.Won);
        return new ListFixturesItem(fixture.Id.ToString(), fixture.TeamId.ToString(), teamName, fixture.PlayedOn, fixture.StartTime, fixture.Opponent, fixture.Venue,
            fixture.Place, score?.Club, score?.Opponent, decided.Count, won, decided.Count - won, LiveFixtures.IsLive(fixture, today, timeOfDay, decided.Count));
    }
}
