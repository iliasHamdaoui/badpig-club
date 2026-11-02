using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.SetFixtureResult;

/// <summary>Records how the seven matches of a fixture already played ended (the outcomes are computed); only the captain of its team or the board can.</summary>
internal sealed class SetFixtureResultHandler(IValidator<SetFixtureResultRequest> validator, InterclubAccess access, IDateTimeProvider clock, AppDbContext db)
    : IHandler<SetFixtureResultRequest>
{
    public async Task<Result> HandleAsync(SetFixtureResultRequest request, CancellationToken ct)
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

        if (fixture.PlayedOn > SeasonRules.Today(clock.UtcNow))
            return Error.Conflict("La rencontre n'a pas encore eu lieu.");

        var rows = await db.InterclubFixtureMatches.Where(m => m.FixtureId == id).ToListAsync(ct);
        foreach (var match in request.Body.Matches!)
        {
            var row = rows.FirstOrDefault(m => m.Slot == match.Slot);
            if (row is null)
            {
                row = new FixtureMatch { FixtureId = id, Slot = match.Slot! };
                db.InterclubFixtureMatches.Add(row);
            }

            row.SetResult(match.Ending ?? MatchEndings.Played, match.Withdrawn, (match.Sets ?? []).Select(s => new SetScore(s.Club!.Value, s.Opponent!.Value)).ToList());
        }

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
