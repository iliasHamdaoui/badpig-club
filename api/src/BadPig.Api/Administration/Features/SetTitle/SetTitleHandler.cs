using FluentValidation;
using BadPig.Shared.Identity;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.SetTitle;

/// <summary>Only an active person of the Board group can have a title; the title moves to the new person if it had a holder.</summary>
internal sealed class SetTitleHandler(IValidator<SetTitleRequest> validator, 
    IUserContext currentUser, IIdentityProvider accounts, AppDbContext db, TitleRemovedNotification removed, IEmailSender mail, IAdministrationAudit auditLog) : IHandler<SetTitleRequest>
{
    public async Task<Result> HandleAsync(SetTitleRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var target = await accounts.FindAsync(request.Id, ct);
        if (!IsActiveBoardMember(target))
            return Error.Conflict("Seule une personne active du bureau peut avoir un titre.");

        var me = currentUser.AccountId;
        if (string.IsNullOrWhiteSpace(request.TitleId))
        {
            var memberId = Guid.Parse(target.Id);
            var row = await db.BoardTitles.Include(b => b.Title).FirstOrDefaultAsync(b => b.MemberId == memberId, ct);
            var previous = row?.Title?.Label;
            if (row is not null)
            {
                db.BoardTitles.Remove(row);
                await db.SaveChangesAsync(ct);
            }

            if (previous is not null)
            {
                await auditLog.RecordAsync(BoardAuditActions.TitleRemoved(previous), target.Id, ct);
                if (target.Id != me)
                    await removed.SendAsync(target, previous, ct);
            }

            return Result.Success();
        }

        var title = Guid.TryParse(request.TitleId, out var titleId)
            ? await db.Titles.AsNoTracking().FirstOrDefaultAsync(t => t.Id == titleId, ct)
            : null;
        if (title is null)
            return Error.NotFound();

        var previousHolder = await AssignAsync(Guid.Parse(target.Id), title.Id, ct);
        if (previousHolder is not null)
        {
            await auditLog.RecordAsync(BoardAuditActions.TitleRemoved(title.Label), previousHolder, ct);
            if (previousHolder != me)
                await removed.SendAsync(await accounts.FindAsync(previousHolder, ct), title.Label, ct);
        }

        await auditLog.RecordAsync($"title.assigned.{title.Label}", target.Id, ct);
        if (target.Id != me)
            await mail.SendAsync(target, $"Vous êtes {title.Label} de BadPig", $"Le bureau vous a attribué le titre de {title.Label}.", ct);

        return Result.Success();
    }

    private static bool IsActiveBoardMember([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] AccountInfo? account) =>
        account is not null && account.IsActive && account.IsInGroup(ClubGroup.Board);

    /// <summary>
    /// Gives the title to the person (who loses the previous one). If the title had another holder, that person loses it:
    /// their identifier is returned so that the audit log keeps a trace.
    /// </summary>
    private async Task<string?> AssignAsync(Guid member, Guid titleId, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        // Two steps: the title is first taken away from its previous holder (unique index on the title).
        var previous = await db.BoardTitles.FirstOrDefaultAsync(b => b.TitleId == titleId && b.MemberId != member, ct);
        if (previous is not null)
        {
            db.BoardTitles.Remove(previous);
            await db.SaveChangesAsync(ct);
        }

        var mine = await db.BoardTitles.FirstOrDefaultAsync(b => b.MemberId == member, ct);
        if (mine is null)
        {
            db.BoardTitles.Add(new BoardTitle { MemberId = member, TitleId = titleId });
        }
        else
        {
            mine.TitleId = titleId;
        }

        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return previous?.MemberId.ToString();
    }
}
