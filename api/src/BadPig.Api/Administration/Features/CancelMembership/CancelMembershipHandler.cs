using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.CancelMembership;

/// <summary>Cancels the validation of the current season (never for oneself).</summary>
internal sealed class CancelMembershipHandler(IValidator<CancelMembershipRequest> validator, IDateTimeProvider clock, IUserContext currentUser, IIdentityProvider identity, AppDbContext db, IAdministrationAudit audit)
    : IHandler<CancelMembershipRequest>
{
    public async Task<Result> HandleAsync(CancelMembershipRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var target = await identity.FindAsync(request.Id, ct);
        if (!AccountRules.IsClubMember(target))
            return Error.NotFound();

        if (AccountRules.IsOwnAccount(target, currentUser.AccountId))
            return Error.Forbidden("Vous ne pouvez pas valider votre propre adhésion : demandez à un autre membre du bureau.");

        var memberId = Guid.Parse(target.Id);
        var season = SeasonRules.Current(await db.Seasons.AsNoTracking().ToListAsync(ct), SeasonRules.Today(clock.UtcNow));
        if (season is null)
            return Error.Conflict("Aucune saison n'est ouverte : créez d'abord la saison en cours.");

        var deleted = await db.Memberships.Where(m => m.MemberId == memberId && m.SeasonId == season.Id).ExecuteDeleteAsync(ct);
        if (deleted > 0)
            await audit.RecordAsync($"membership.cancelled.{season.Name}", target.Id, ct);

        return Result.Success();
    }
}
