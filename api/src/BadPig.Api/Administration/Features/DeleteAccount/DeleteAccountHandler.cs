using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.DeleteAccount;

/// <summary>Permanent deletion of an account by the board (or an admin for board and admin accounts).</summary>
internal sealed class DeleteAccountHandler(IValidator<DeleteAccountRequest> validator, 
    IUserContext currentUser, AccountAccess access, AccountDeletionService deletion, AccountDeletedNotification notification,
    IAdministrationAudit audit, ILogger<DeleteAccountHandler> log) : IHandler<DeleteAccountRequest>
{
    private const string AuditAction = "account.deleted";

    public async Task<Result> HandleAsync(DeleteAccountRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var resolved = await access.ResolveManageableAsync(request.Id, ct);
        if (!resolved.IsSuccess)
            return resolved.Errors;

        var target = resolved.Value;
        await notification.SendAsync(target, automatic: false, ct); // before deleting: afterwards the e-mail address no longer exists
        await deletion.DeleteAsync(target.Id, ct);
        await audit.RecordAsync(AuditAction, target.Id, ct);
        log.LogInformation("Account {Target}: {Action} by {Actor}", target.Id, AuditAction, currentUser.AccountId);
        return Result.Success();
    }
}
