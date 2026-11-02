using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.SetAnnouncementPoster;

/// <summary>Sets the poster of an announcement; the previous poster, if any, is replaced.</summary>
internal sealed class SetAnnouncementPosterHandler(IValidator<SetAnnouncementPosterRequest> validator, IDateTimeProvider clock,
    AppDbContext db, IAnnouncementFileStore files) : IHandler<SetAnnouncementPosterRequest>
{
    public async Task<Result> HandleAsync(SetAnnouncementPosterRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.Include(a => a.Files).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        var upload = request.File!;
        if (!await HasAnImageSignatureAsync(upload, ct))
            return Error.Validation("file", "Le fichier n'est pas une image valide.");

        var key = await files.SaveAsync(upload.Content, ct);
        var previous = announcement.Files.FirstOrDefault(f => f.Role == AnnouncementFileRoles.Poster);
        try
        {
            // Two steps in one transaction: the old poster goes first (unique index: one poster per announcement).
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            if (previous is not null)
            {
                db.AnnouncementFiles.Remove(previous);
                await db.SaveChangesAsync(ct);
            }

            db.AnnouncementFiles.Add(new AnnouncementFile
            {
                Id = Guid.NewGuid(),
                AnnouncementId = id,
                Role = AnnouncementFileRoles.Poster,
                FileName = AnnouncementFileRules.CleanName(upload.FileName),
                ContentType = AnnouncementFileRules.ContentTypeOf(upload.FileName)!,
                SizeBytes = upload.Length,
                StorageKey = key,
                CreatedAt = clock.UtcNow,
            });
            announcement.UpdatedAt = clock.UtcNow;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            await files.DeleteAsync(key, ct);
            return Error.Conflict("L'annonce a été modifiée en même temps : recommencez.");
        }

        if (previous is not null)
            await files.DeleteAsync(previous.StorageKey, ct);

        return Result.Success();
    }

    private static async Task<bool> HasAnImageSignatureAsync(UploadedFile upload, CancellationToken ct)
    {
        var header = new byte[12];
        var read = await upload.Content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, ct);
        upload.Content.Seek(0, SeekOrigin.Begin);
        return AnnouncementFileRules.HasImageSignature(header.AsSpan(0, read));
    }
}
