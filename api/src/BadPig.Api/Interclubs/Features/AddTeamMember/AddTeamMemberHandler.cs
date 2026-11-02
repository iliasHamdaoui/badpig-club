using BadPig.Shared.Identity;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Administration.Core;
using BadPig.Interclubs.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Interclubs.Features.AddTeamMember;

/// <summary>Adds an active club member to the players of a team; only its captain or the board can.</summary>
internal sealed class AddTeamMemberHandler(IValidator<AddTeamMemberRequest> validator, InterclubAccess access, IIdentityProvider identity,
    IMembershipReader memberships, IDateTimeProvider clock, AppDbContext db)
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

        if (!TeamRules.CanPlayInterclubs(await memberships.GetAsync(person.Id, ct)))
            return Error.Validation("accountId", "Un licencié dans un autre club ne peut pas jouer en interclubs pour BadPig.");

        var accountId = Guid.Parse(person.Id);
        if (await db.InterclubTeamMembers.AnyAsync(m => m.TeamId == teamId && m.AccountId == accountId, ct))
            return Result.Success();

        if (await PlaysInAnotherTeamThisSeasonAsync(accountId, team, ct))
            return Error.Conflict("Cette personne joue déjà dans une autre équipe de la saison.");

        db.InterclubTeamMembers.Add(new TeamMember { TeamId = teamId, AccountId = accountId, AddedAt = clock.UtcNow });
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static bool CanPlay([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] AccountInfo? account) =>
        AccountRules.IsClubMember(account) && account.IsActive;

    /// <summary>A player belongs to a single team for a given season: the club does not field the same person twice.</summary>
    private async Task<bool> PlaysInAnotherTeamThisSeasonAsync(Guid accountId, Team team, CancellationToken ct) =>
        await db.InterclubTeamMembers
            .Join(db.InterclubTeams, m => m.TeamId, t => t.Id, (m, t) => new { m.AccountId, TeamId = t.Id, t.SeasonId })
            .AnyAsync(x => x.AccountId == accountId && x.SeasonId == team.SeasonId && x.TeamId != team.Id, ct);
}
