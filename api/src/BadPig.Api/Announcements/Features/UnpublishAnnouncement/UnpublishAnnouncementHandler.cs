using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.UnpublishAnnouncement;

/// <summary>Takes an announcement back to draft.</summary>
internal sealed class UnpublishAnnouncementHandler(IValidator<UnpublishAnnouncementRequest> validator, IDateTimeProvider clock, AppDbContext db)
    : IHandler<UnpublishAnnouncementRequest>
{
    public async Task<Result> HandleAsync(UnpublishAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        if (!announcement.IsPublished)
            return Result.Success();

        announcement.IsPublished = false;
        announcement.PublishedAt = null;
        announcement.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
