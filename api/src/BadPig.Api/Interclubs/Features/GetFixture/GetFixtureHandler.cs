using BadPig.Administration.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.GetFixture;

/// <summary>One fixture, with its team and what the signed-in person can do with it.</summary>
internal sealed class GetFixtureHandler(IValidator<GetFixtureRequest> validator, InterclubAccess access, IIdentityProvider identity, ISeasonReader seasons, AppDbContext db)
    : IHandler<GetFixtureRequest, GetFixtureResponse>
{
    public async Task<Result<GetFixtureResponse>> HandleAsync(GetFixtureRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanViewAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var id = Guid.Parse(request.Id);
        var fixture = await db.InterclubFixtures.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fixture is null)
            return Error.NotFound();

        var team = await db.InterclubTeams.AsNoTracking().FirstAsync(t => t.Id == fixture.TeamId, ct);
        var season = await seasons.FindAsync(team.SeasonId, ct);
        var names = (await identity.ListAsync(ct)).ToDictionary(a => a.Id, a => a.Name);
        var rows = await db.InterclubFixtureMatches.AsNoTracking().Where(m => m.FixtureId == id).ToListAsync(ct);
        var score = MatchScore.Of(rows);
        var matches = MatchSlots.All.Select(slot => ToMatch(slot, rows.FirstOrDefault(m => m.Slot == slot), names)).ToList();
        return new GetFixtureResponse(fixture.Id.ToString(), team.Id.ToString(), team.Name, season?.Name ?? "", team.CaptainId?.ToString(),
            team.CaptainId is { } captainId ? names.GetValueOrDefault(captainId.ToString()) : null,
            fixture.PlayedOn, fixture.StartTime, fixture.Opponent, fixture.Venue, fixture.Place, score?.Club, score?.Opponent, matches, access.CanManageFixtures(team));
    }

    private static GetFixtureMatch ToMatch(string slot, FixtureMatch? row, Dictionary<string, string> names) =>
        new(slot, (row?.Players ?? []).Select(id => new GetFixturePlayer(id.ToString(), names.GetValueOrDefault(id.ToString(), "Compte supprimé"))).ToList(), (row?.Sets ?? []).Select(s => new GetFixtureSet(s.Club, s.Opponent)).ToList(), row?.Ending, row?.Withdrawn, row?.Outcome);
}
