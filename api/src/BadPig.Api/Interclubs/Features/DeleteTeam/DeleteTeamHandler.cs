using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.DeleteTeam;

/// <summary>Deletes a team; its fixtures go with it (cascade).</summary>
internal sealed class DeleteTeamHandler(IValidator<DeleteTeamRequest> validator, AppDbContext db)
    : IHandler<DeleteTeamRequest>
{
    public async Task<Result> HandleAsync(DeleteTeamRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var team = await db.InterclubTeams.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (team is null)
            return Error.NotFound();

        db.InterclubTeams.Remove(team);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
