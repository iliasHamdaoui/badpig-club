using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.EnableAccount;

/// <summary>Re-enables a disabled account: no more countdown to the automatic deletion.</summary>
internal sealed class EnableAccountHandler(IValidator<EnableAccountRequest> validator, 
    IUserContext currentUser, AccountAccess access, IIdentityProvider identity, AppDbContext db, IEmailSender mail,
    IAdministrationAudit audit, ILogger<EnableAccountHandler> log) : IHandler<EnableAccountRequest>
{
    private const string AuditAction = "account.reenabled";

    public async Task<Result> HandleAsync(EnableAccountRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var resolved = await access.ResolveManageableAsync(request.Id, ct);
        if (!resolved.IsSuccess)
            return resolved.Errors;

        var target = resolved.Value;
        await identity.SetActiveAsync(target.Id, true, ct);
        var accountId = Guid.Parse(target.Id);
        await db.DisabledAccounts.Where(d => d.AccountId == accountId).ExecuteDeleteAsync(ct);
        await mail.SendAsync(target, "Votre compte BadPig a été réactivé",
            "Votre compte a été réactivé : vous pouvez de nouveau vous connecter au site du club.", ct);
        await audit.RecordAsync(AuditAction, target.Id, ct);
        log.LogInformation("Account {Target}: {Action} by {Actor}", target.Id, AuditAction, currentUser.AccountId);
        return Result.Success();
    }
}
