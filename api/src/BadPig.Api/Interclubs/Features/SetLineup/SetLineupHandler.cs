using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.SetLineup;

/// <summary>Chooses the players of the matches among the players of the team; only its captain or the board can.</summary>
internal sealed class SetLineupHandler(IValidator<SetLineupRequest> validator, InterclubAccess access, AppDbContext db)
    : IHandler<SetLineupRequest>
{
    public async Task<Result> HandleAsync(SetLineupRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var fixture = await db.InterclubFixtures.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fixture is null)
            return Error.NotFound();

        var team = await db.InterclubTeams.AsNoTracking().FirstAsync(t => t.Id == fixture.TeamId, ct);
        if (!access.CanManageFixtures(team))
            return Error.Forbidden("Réservé au capitaine de l'équipe et au bureau.");

        var chosen = request.Body.Slots!.ToDictionary(s => s.Slot!, s => s.PlayerIds!.Select(Guid.Parse).ToList());
        var roster = (await db.InterclubTeamMembers.AsNoTracking().Where(m => m.TeamId == team.Id).Select(m => m.AccountId).ToListAsync(ct)).ToHashSet();
        if (!chosen.Values.SelectMany(ids => ids).All(roster.Contains))
            return Error.Validation("slots", "Les joueurs doivent faire partie de l'équipe.");

        var rows = await db.InterclubFixtureMatches.Where(m => m.FixtureId == id).ToListAsync(ct);
        foreach (var slot in MatchSlots.All)
        {
            var row = rows.FirstOrDefault(m => m.Slot == slot);
            var players = chosen.GetValueOrDefault(slot);
            if (row is null && players is null)
                continue;

            if (row is null)
            {
                row = new FixtureMatch { FixtureId = id, Slot = slot };
                db.InterclubFixtureMatches.Add(row);
            }

            row.Player1Id = players?[0];
            row.Player2Id = players is { Count: > 1 } ? players[1] : null;
            if (players is null && row.Outcome is null)
                db.InterclubFixtureMatches.Remove(row);
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
