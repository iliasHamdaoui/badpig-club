using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ValidateMembership;

/// <summary>
/// A board member confirms that the dues of the current season have been paid (never for oneself). The kind of membership
/// (internal or external) decides the fee, which is copied on the membership so that later changes of the fee do not alter it.
/// </summary>
internal sealed class ValidateMembershipHandler(IValidator<ValidateMembershipRequest> validator, IDateTimeProvider clock,
    IUserContext currentUser, IIdentityProvider identity, AppDbContext db, IEmailSender mail, IAdministrationAudit audit)
    : IHandler<ValidateMembershipRequest>
{
    public async Task<Result> HandleAsync(ValidateMembershipRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var target = await identity.FindAsync(request.Id, ct);
        if (!AccountRules.IsClubMember(target))
            return Error.NotFound();

        if (AccountRules.IsOwnAccount(target, currentUser.AccountId))
            return Error.Forbidden("Vous ne pouvez pas valider votre propre adhésion : demandez à un autre membre du bureau.");

        if (!target.IsActive)
            return Error.Conflict("Ce compte est désactivé.");

        var season = SeasonRules.Current(await db.Seasons.AsNoTracking().ToListAsync(ct), SeasonRules.Today(clock.UtcNow));
        if (season is null)
            return Error.Conflict("Aucune saison n'est ouverte : créez d'abord la saison en cours.");

        var memberId = Guid.Parse(target.Id);
        if (!await db.Memberships.AnyAsync(m => m.MemberId == memberId && m.SeasonId == season.Id, ct))
        {
            var kind = request.Kind
                ?? (await db.Invitations.AsNoTracking().FirstOrDefaultAsync(i => i.TargetId == memberId, ct))?.MembershipKind
                ?? MembershipKinds.Internal;
            db.Memberships.Add(new Membership
            {
                MemberId = memberId,
                SeasonId = season.Id,
                Kind = kind,
                Amount = SeasonRules.FeeFor(season, kind),
                ValidatedAt = clock.UtcNow,
                ValidatedById = Guid.Parse(currentUser.AccountId!),
                ValidatedByName = currentUser.DisplayName,
            });
            await db.SaveChangesAsync(ct);
            await audit.RecordAsync($"membership.validated.{season.Name}", target.Id, ct);
            await mail.SendAsync(target, $"Votre adhésion {season.Name} est validée",
                $"Votre adhésion au club pour la saison {season.Name} a été validée. Merci, et bonne saison !", ct);
        }

        return Result.Success();
    }
}
