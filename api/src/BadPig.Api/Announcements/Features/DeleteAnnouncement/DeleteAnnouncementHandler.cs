using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.DeleteAnnouncement;

/// <summary>Deletes an announcement with its files (the likes go with it).</summary>
internal sealed class DeleteAnnouncementHandler(IValidator<DeleteAnnouncementRequest> validator, AppDbContext db, IAnnouncementFileStore files)
    : IHandler<DeleteAnnouncementRequest>
{
    public async Task<Result> HandleAsync(DeleteAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.Include(a => a.Files).FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        var keys = announcement.Files.Select(f => f.StorageKey).ToList();
        db.Announcements.Remove(announcement);
        await db.SaveChangesAsync(ct);
        foreach (var key in keys)
            await files.DeleteAsync(key, ct);

        return Result.Success();
    }
}
