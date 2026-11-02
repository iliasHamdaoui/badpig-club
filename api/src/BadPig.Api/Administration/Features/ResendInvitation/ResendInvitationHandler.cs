using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.ResendInvitation;

/// <summary>New sending of the link (lost or expired link) for a person who is still pending.</summary>
internal sealed class ResendInvitationHandler(IValidator<ResendInvitationRequest> validator, 
    AccountAccess access, IIdentityProvider identity, IAdministrationAudit audit, ILogger<ResendInvitationHandler> log)
    : IHandler<ResendInvitationRequest>
{
    private const string AuditAction = "invitation.resent";

    public async Task<Result> HandleAsync(ResendInvitationRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var resolved = await access.ResolveManageableAsync(request.Id, ct);
        if (!resolved.IsSuccess)
            return resolved.Errors;

        var target = resolved.Value;
        if (!identity.CanSendSignupLink)
            return Error.Unexpected("Invitations non configurées.");

        if (!IsWaitingForSignup(target))
            return Error.Conflict("Cette personne n'est pas en attente d'inscription.");

        await audit.RecordAsync(AuditAction, target.Id, ct);
        return await SignupLink.SendAsync(identity, target, log, ct);
    }

    /// <summary>
    /// Waiting for signup = an active account that has never signed in. The invitation line is not required:
    /// an account created straight in the identity provider has none, and its owner still needs a link.
    /// </summary>
    private static bool IsWaitingForSignup(AccountInfo target) => target.IsActive && target.LastLogin is null;
}
