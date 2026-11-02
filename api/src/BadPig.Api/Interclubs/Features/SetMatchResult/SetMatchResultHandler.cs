using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.SetMatchResult;

/// <summary>Records how one match of a fixture ended, so that the captain can enter the results as they come; only the captain of its team or the board can.</summary>
internal sealed class SetMatchResultHandler(IValidator<SetMatchResultRequest> validator, InterclubAccess access, IDateTimeProvider clock, AppDbContext db)
    : IHandler<SetMatchResultRequest>
{
    public async Task<Result> HandleAsync(SetMatchResultRequest request, CancellationToken ct)
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

        var row = await db.InterclubFixtureMatches.FirstOrDefaultAsync(m => m.FixtureId == id && m.Slot == request.Slot, ct);
        if (row is null)
        {
            row = new FixtureMatch { FixtureId = id, Slot = request.Slot };
            db.InterclubFixtureMatches.Add(row);
        }

        var sets = (request.Body.Sets ?? []).Select(s => new SetScore(s.Club!.Value, s.Opponent!.Value)).ToList();
        row.SetResult(request.Body.Ending ?? MatchEndings.Played, request.Body.Withdrawn, sets);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
