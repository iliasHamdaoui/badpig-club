using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.InviteUser;

/// <summary>
/// The board creates the account; the person receives an e-mail with a 3-day link to choose a password.
/// A board member also belongs to Members, an administrator to Members and Board (they stay members if they leave the board).
/// </summary>
internal sealed class InviteUserHandler(IValidator<InviteUserRequest> validator, IDateTimeProvider clock, 
    IUserContext currentUser, IIdentityProvider identity, AppDbContext db, IAdministrationAudit audit, ILogger<InviteUserHandler> log)
    : IHandler<InviteUserRequest, CreatedResponse>
{
    private const string AuditAction = "invitation.sent";

    public async Task<Result<CreatedResponse>> HandleAsync(InviteUserRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var name = request.Name!.Trim();
        var email = request.Email!.Trim().ToLowerInvariant();
        var wanted = request.Group switch
        {
            "board" => new[] { ClubGroup.Members, ClubGroup.Board },
            "admin" => new[] { ClubGroup.Members, ClubGroup.Board, ClubGroup.Admin },
            _ => new[] { ClubGroup.Members },
        };

        if (IsInvitingAnAdmin(wanted) && IsNotAdmin)
            return Error.Forbidden("Seul un administrateur peut inviter un administrateur.");

        if (!identity.CanSendSignupLink)
        {
            log.LogError("Invitations are not configured (invitation e-mail)");
            return Error.Unexpected("Invitations non configurées.");
        }

        if (await identity.FindByEmailAsync(email, ct) is not null)
            return Error.Conflict("Cette adresse e-mail est déjà utilisée par un compte.");

        AccountInfo created;
        try
        {
            created = await identity.CreateAsync(name, email, wanted, ct);
        }
        catch (IdentityConfigurationException e)
        {
            log.LogError(e, "Invitations are not configured (groups)");
            return Error.Unexpected("Invitations non configurées.");
        }

        db.Invitations.Add(new Invitation
        {
            TargetId = Guid.Parse(created.Id),
            InvitedById = Guid.Parse(currentUser.AccountId!),
            InvitedByName = currentUser.DisplayName,
            MembershipKind = request.MembershipKind ?? MembershipKinds.Internal,
            CreatedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        await audit.RecordAsync(AuditAction, created.Id, ct);
        if (wanted.Contains(ClubGroup.Board))
            await audit.RecordAsync(BoardAuditActions.Joined, created.Id, ct);

        var sent = await SignupLink.SendAsync(identity, created, log, ct);
        if (!sent.IsSuccess)
            return sent.Errors;

        return new CreatedResponse(created.Id);
    }

    private bool IsNotAdmin => !currentUser.IsInGroup(ClubGroup.Admin);

    private static bool IsInvitingAnAdmin(ClubGroup[] wanted) => wanted.Contains(ClubGroup.Admin);
}
