using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.DisableAccount;

/// <summary>Disables an account. A disabled account is deleted automatically after one month (see DisabledAccountCleanup).</summary>
internal sealed class DisableAccountHandler(IValidator<DisableAccountRequest> validator, IDateTimeProvider clock, 
    IUserContext currentUser, AccountAccess access, IIdentityProvider identity, AppDbContext db, IEmailSender mail,
    IAdministrationAudit audit, ILogger<DisableAccountHandler> log) : IHandler<DisableAccountRequest>
{
    private const string AuditAction = "account.disabled";

    public async Task<Result> HandleAsync(DisableAccountRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var resolved = await access.ResolveManageableAsync(request.Id, ct);
        if (!resolved.IsSuccess)
            return resolved.Errors;

        var target = resolved.Value;
        await identity.SetActiveAsync(target.Id, false, ct);

        // The disabling date is kept if it already exists.
        var accountId = Guid.Parse(target.Id);
        if (!await db.DisabledAccounts.AnyAsync(d => d.AccountId == accountId, ct))
        {
            db.DisabledAccounts.Add(new DisabledAccount { AccountId = accountId, DisabledSince = clock.UtcNow });
            await db.SaveChangesAsync(ct);
        }

        var deletionDate = DisabledAccount.DeletionDate(clock.UtcNow).ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        await mail.SendAsync(target, "Votre compte BadPig a été désactivé",
            "Votre compte a été désactivé par le bureau du club : vous ne pouvez plus vous connecter.\n\n" +
            $"Sans réactivation de sa part, il sera supprimé définitivement le {deletionDate}. Pour toute question, contactez le bureau du club.", ct);
        await audit.RecordAsync(AuditAction, target.Id, ct);
        log.LogInformation("Account {Target}: {Action} by {Actor}", target.Id, AuditAction, currentUser.AccountId);
        return Result.Success();
    }
}
