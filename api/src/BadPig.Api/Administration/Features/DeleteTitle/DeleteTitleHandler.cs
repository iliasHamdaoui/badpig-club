using FluentValidation;
using BadPig.Shared.Identity;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BadPig.Administration.Features.DeleteTitle;

/// <summary>Deletes a title; the person who held it loses it (recorded in the audit log).</summary>
internal sealed class DeleteTitleHandler(IValidator<DeleteTitleRequest> validator, 
    IUserContext currentUser, AppDbContext db, IIdentityProvider accounts, TitleRemovedNotification notification, IAdministrationAudit auditLog) : IHandler<DeleteTitleRequest>
{
    public async Task<Result> HandleAsync(DeleteTitleRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var title = Guid.TryParse(request.Id, out var titleId) ? await db.Titles.FirstOrDefaultAsync(t => t.Id == titleId, ct) : null;
        if (title is null)
            return Error.NotFound();

        var holder = (await db.BoardTitles.AsNoTracking().FirstOrDefaultAsync(b => b.TitleId == titleId, ct))?.MemberId.ToString();
        var label = title.Label;
        db.Titles.Remove(title); // the database removes the assignment too (ON DELETE CASCADE)
        await db.SaveChangesAsync(ct);

        if (holder is not null)
        {
            await auditLog.RecordAsync(BoardAuditActions.TitleRemoved(label), holder, ct);
            if (holder != currentUser.AccountId)
                await notification.SendAsync(await accounts.FindAsync(holder, ct), label, ct);
        }

        await auditLog.RecordAsync($"title.deleted.{label}", null, ct);
        return Result.Success();
    }
}
