using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.RemoveAnnouncementPoster;

/// <summary>Removes the poster of an announcement.</summary>
internal sealed class RemoveAnnouncementPosterHandler(IValidator<RemoveAnnouncementPosterRequest> validator, AppDbContext db, IAnnouncementFileStore files)
    : IHandler<RemoveAnnouncementPosterRequest>
{
    public async Task<Result> HandleAsync(RemoveAnnouncementPosterRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.Include(a => a.Files).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        var poster = announcement.Files.FirstOrDefault(f => f.Role == AnnouncementFileRoles.Poster);
        if (poster is null)
            return Result.Success();

        db.AnnouncementFiles.Remove(poster);
        await db.SaveChangesAsync(ct);
        await files.DeleteAsync(poster.StorageKey, ct);
        return Result.Success();
    }
}
