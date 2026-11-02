using BadPig.Shared.Identity;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.AssignCaptain;

/// <summary>Sets the captain of a team after checking that the person is an active club member.</summary>
internal sealed class AssignCaptainHandler(IValidator<AssignCaptainRequest> validator, IIdentityProvider identity, AppDbContext db)
    : IHandler<AssignCaptainRequest>
{
    public async Task<Result> HandleAsync(AssignCaptainRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var team = await db.InterclubTeams.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (team is null)
            return Error.NotFound();

        var captain = await identity.FindAsync(request.Body.CaptainId!, ct);
        if (!CanBeCaptain(captain))
            return Error.Validation("captainId", "Le capitaine doit être un membre actif du club.");

        team.CaptainId = Guid.Parse(captain.Id);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static bool CanBeCaptain([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] AccountInfo? account) =>
        AccountRules.IsClubMember(account) && account.IsActive;
}
