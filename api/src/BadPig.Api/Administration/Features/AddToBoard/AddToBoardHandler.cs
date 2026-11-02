using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.AddToBoard;

/// <summary>Adds a person to the Board group.</summary>
internal sealed class AddToBoardHandler(IValidator<AddToBoardRequest> validator, 
    IUserContext currentUser, IIdentityProvider identity, IEmailSender mail, IAdministrationAudit audit, ILogger<AddToBoardHandler> log)
    : IHandler<AddToBoardRequest>
{
    public async Task<Result> HandleAsync(AddToBoardRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var target = await identity.FindAsync(request.Id, ct);
        if (!AccountRules.IsClubMember(target))
            return Error.NotFound();

        var boardCount = (await identity.ListAsync(ct)).Count(a => a.IsActive && a.IsInGroup(ClubGroup.Board));
        if (!CanAddToBoard(target, boardCount))
            return Error.Forbidden("Vous ne pouvez pas modifier le bureau pour ce compte.");

        try
        {
            await identity.AddToGroupAsync(target.Id, ClubGroup.Board, ct);
        }
        catch (IdentityConfigurationException e)
        {
            log.LogError(e, "Board management is not configured (group identifiers)");
            return Error.Unexpected("Gestion du bureau non configurée.");
        }

        await audit.RecordAsync(BoardAuditActions.Joined, target.Id, ct);
        await mail.SendAsync(target, "Vous faites partie du bureau de BadPig", "Vous avez été ajouté(e) au bureau du club.", ct);
        log.LogInformation("Account {Target}: {Action} by {Actor}", target.Id, BoardAuditActions.Joined, currentUser.AccountId);
        return Result.Success();
    }

    private bool CanAddToBoard(AccountInfo target, int boardCount) =>
        BoardRules.AllowedAction(target, currentUser.AccountId, currentUser.IsInGroup(ClubGroup.Admin), boardCount) == BoardActions.Add;
}
