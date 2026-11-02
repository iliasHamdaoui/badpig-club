using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.PublishAnnouncement;

/// <summary>Publishes an announcement: it takes the date of the publication (nothing changes if it is already published).</summary>
internal sealed class PublishAnnouncementHandler(IValidator<PublishAnnouncementRequest> validator, IDateTimeProvider clock, AppDbContext db)
    : IHandler<PublishAnnouncementRequest>
{
    public async Task<Result> HandleAsync(PublishAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        if (announcement.IsPublished)
            return Result.Success();

        announcement.IsPublished = true;
        announcement.PublishedAt = clock.UtcNow;
        announcement.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
