using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.DeleteFixture;

/// <summary>Deletes a fixture; only the captain of its team or the board can.</summary>
internal sealed class DeleteFixtureHandler(IValidator<DeleteFixtureRequest> validator, InterclubAccess access, AppDbContext db)
    : IHandler<DeleteFixtureRequest>
{
    public async Task<Result> HandleAsync(DeleteFixtureRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var fixture = await db.InterclubFixtures.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fixture is null)
            return Error.NotFound();

        var team = await db.InterclubTeams.AsNoTracking().FirstAsync(t => t.Id == fixture.TeamId, ct);
        if (!access.CanManageFixtures(team))
            return Error.Forbidden("Réservé au capitaine de l'équipe et au bureau.");

        db.InterclubFixtures.Remove(fixture);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
