using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.RemoveFromBoard;

/// <summary>Removes a person from the Board group: they stay a member and lose their title, if any.</summary>
internal sealed class RemoveFromBoardHandler(IValidator<RemoveFromBoardRequest> validator, 
    IUserContext currentUser, IIdentityProvider identity, AppDbContext db, IEmailSender mail, IAdministrationAudit audit,
    ILogger<RemoveFromBoardHandler> log) : IHandler<RemoveFromBoardRequest>
{
    private const string AuditAction = "board.left";

    public async Task<Result> HandleAsync(RemoveFromBoardRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var target = await identity.FindAsync(request.Id, ct);
        if (!AccountRules.IsClubMember(target))
            return Error.NotFound();

        var boardCount = (await identity.ListAsync(ct)).Count(a => a.IsActive && a.IsInGroup(ClubGroup.Board));
        if (!CanRemoveFromBoard(target, boardCount))
        {
            return IsLastBoardMember(target, boardCount)
                ? Error.Conflict("Il doit rester au moins un membre du bureau.")
                : Error.Forbidden("Vous ne pouvez pas modifier le bureau pour ce compte.");
        }

        try
        {
            // The person stays a member: make sure they are in "Membres" before removing them from the board.
            if (IsNotInMembersGroup(target))
                await identity.AddToGroupAsync(target.Id, ClubGroup.Members, ct);

            await identity.RemoveFromGroupAsync(target.Id, ClubGroup.Board, ct);
        }
        catch (IdentityConfigurationException e)
        {
            log.LogError(e, "Board management is not configured (group identifiers)");
            return Error.Unexpected("Gestion du bureau non configurée.");
        }

        // Out of the board: the person also loses their title, if any.
        var memberId = Guid.Parse(target.Id);
        var row = await db.BoardTitles.Include(b => b.Title).FirstOrDefaultAsync(b => b.MemberId == memberId, ct);
        var lostTitle = row?.Title?.Label;
        if (row is not null)
        {
            db.BoardTitles.Remove(row);
            await db.SaveChangesAsync(ct);
        }

        if (lostTitle is not null)
            await audit.RecordAsync(BoardAuditActions.TitleRemoved(lostTitle), target.Id, ct);

        await audit.RecordAsync(AuditAction, target.Id, ct);
        await mail.SendAsync(target, "Vous ne faites plus partie du bureau de BadPig",
            "Vous avez été retiré(e) du bureau du club. Vous restez adhérent(e) et gardez l'accès à l'espace membres." +
            (lostTitle is null ? "" : $"\n\nVous n'êtes donc plus {lostTitle}."), ct);
        log.LogInformation("Account {Target}: {Action} by {Actor}", target.Id, AuditAction, currentUser.AccountId);
        return Result.Success();
    }

    private bool CanRemoveFromBoard(AccountInfo target, int boardCount) =>
        BoardRules.AllowedAction(target, currentUser.AccountId, currentUser.IsInGroup(ClubGroup.Admin), boardCount) == BoardActions.Remove;

    private static bool IsLastBoardMember(AccountInfo target, int boardCount) => target.IsInGroup(ClubGroup.Board) && boardCount <= 1;

    private static bool IsNotInMembersGroup(AccountInfo target) => !target.IsInGroup(ClubGroup.Members);
}
