using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.RemoveCaptain;

/// <summary>Clears the captain of a team.</summary>
internal sealed class RemoveCaptainHandler(IValidator<RemoveCaptainRequest> validator, AppDbContext db)
    : IHandler<RemoveCaptainRequest>
{
    public async Task<Result> HandleAsync(RemoveCaptainRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var team = await db.InterclubTeams.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (team is null)
            return Error.NotFound();

        team.CaptainId = null;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
