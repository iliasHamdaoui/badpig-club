using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using Microsoft.Extensions.Logging;

namespace BadPig.Profile.Features.DeleteMyAccount;

/// <summary>Deletion of OWN account (permanent). The target account comes from the token. The board's journal keeps a trace.</summary>
internal sealed class DeleteMyAccountHandler(
    IUserContext currentUser, IIdentityProvider identity, AccountDeletionService deletion, IAdministrationAudit audit,
    ILogger<DeleteMyAccountHandler> log) : IHandler<DeleteMyAccountRequest>
{
    private const string AuditAction = "account.deleted_by_owner";

    public async Task<Result> HandleAsync(DeleteMyAccountRequest request, CancellationToken ct)
    {
        var sub = currentUser.AccountId;
        var me = sub is null ? null : await identity.FindAsync(sub, ct);
        if (me is null)
            return Error.NotFound();

        // The last active administrator cannot delete themselves: nobody could manage the site any more.
        if (me.IsInGroup(ClubGroup.Admin))
        {
            var admins = (await identity.ListAsync(ct)).Count(u => u.IsActive && u.IsInGroup(ClubGroup.Admin));
            if (admins <= 1)
                return Error.Conflict("Vous êtes le dernier administrateur : désignez-en un autre avant de supprimer votre compte.");
        }

        await deletion.DeleteAsync(me.Id, ct);
        await audit.RecordAsync(AuditAction, me.Id, ct);
        log.LogInformation("Account {Target}: deleted by its owner", me.Id);
        return Result.Success();
    }
}
