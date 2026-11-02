using BadPig.Shared.Identity;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.AddTeamMember;

/// <summary>Adds an active club member to the players of a team; only its captain or the board can.</summary>
internal sealed class AddTeamMemberHandler(IValidator<AddTeamMemberRequest> validator, InterclubAccess access, IIdentityProvider identity, IDateTimeProvider clock, AppDbContext db)
    : IHandler<AddTeamMemberRequest>
{
    public async Task<Result> HandleAsync(AddTeamMemberRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var teamId = Guid.Parse(request.Id);
        var team = await db.InterclubTeams.AsNoTracking().FirstOrDefaultAsync(t => t.Id == teamId, ct);
        if (team is null)
            return Error.NotFound();

        if (!access.CanManageFixtures(team))
            return Error.Forbidden("Réservé au capitaine de l'équipe et au bureau.");

        var person = await identity.FindAsync(request.AccountId, ct);
        if (!CanPlay(person))
            return Error.Validation("accountId", "Le joueur doit être un membre actif du club.");

        var accountId = Guid.Parse(person.Id);
        if (await db.InterclubTeamMembers.AnyAsync(m => m.TeamId == teamId && m.AccountId == accountId, ct))
            return Result.Success();

        db.InterclubTeamMembers.Add(new TeamMember { TeamId = teamId, AccountId = accountId, AddedAt = clock.UtcNow });
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static bool CanPlay([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] AccountInfo? account) =>
        AccountRules.IsClubMember(account) && account.IsActive;
}
