using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.RemoveAnnouncementAttachment;

/// <summary>Removes an attachment of an announcement (the poster has its own feature).</summary>
internal sealed class RemoveAnnouncementAttachmentHandler(IValidator<RemoveAnnouncementAttachmentRequest> validator, AppDbContext db, IAnnouncementFileStore files)
    : IHandler<RemoveAnnouncementAttachmentRequest>
{
    public async Task<Result> HandleAsync(RemoveAnnouncementAttachmentRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var announcementId = Guid.Parse(request.Id);
        var fileId = Guid.Parse(request.FileId);
        var attachment = await db.AnnouncementFiles.FirstOrDefaultAsync(
            f => f.Id == fileId && f.AnnouncementId == announcementId && f.Role == AnnouncementFileRoles.Attachment, ct);
        if (attachment is null)
            return Error.NotFound();

        db.AnnouncementFiles.Remove(attachment);
        await db.SaveChangesAsync(ct);
        await files.DeleteAsync(attachment.StorageKey, ct);
        return Result.Success();
    }
}
