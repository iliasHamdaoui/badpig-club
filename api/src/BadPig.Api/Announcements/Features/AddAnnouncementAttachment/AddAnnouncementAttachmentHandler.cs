using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.AddAnnouncementAttachment;

/// <summary>Adds an attachment (10 at most per announcement).</summary>
internal sealed class AddAnnouncementAttachmentHandler(IValidator<AddAnnouncementAttachmentRequest> validator, IDateTimeProvider clock,
    AppDbContext db, IAnnouncementFileStore files) : IHandler<AddAnnouncementAttachmentRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(AddAnnouncementAttachmentRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.Include(a => a.Files).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        if (HasTooManyAttachments(announcement))
            return Error.Conflict($"Une annonce a {AnnouncementFileRules.MaxAttachments} pièces jointes au plus.");

        var upload = request.File!;
        var attachment = new AnnouncementFile
        {
            Id = Guid.NewGuid(),
            AnnouncementId = id,
            Role = AnnouncementFileRoles.Attachment,
            FileName = AnnouncementFileRules.CleanName(upload.FileName),
            ContentType = AnnouncementFileRules.ContentTypeOf(upload.FileName)!,
            SizeBytes = upload.Length,
            StorageKey = await files.SaveAsync(upload.Content, ct),
            CreatedAt = clock.UtcNow,
        };
        db.AnnouncementFiles.Add(attachment);
        announcement.UpdatedAt = clock.UtcNow;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            await files.DeleteAsync(attachment.StorageKey, ct);
            return Error.Conflict("L'annonce a été modifiée en même temps : recommencez.");
        }

        return new CreatedResponse(attachment.Id.ToString());
    }

    private static bool HasTooManyAttachments(Announcement announcement) =>
        announcement.Files.Count(f => f.Role == AnnouncementFileRoles.Attachment) >= AnnouncementFileRules.MaxAttachments;
}
