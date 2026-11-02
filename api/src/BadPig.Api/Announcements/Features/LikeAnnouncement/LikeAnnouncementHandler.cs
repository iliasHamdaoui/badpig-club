using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.LikeAnnouncement;

/// <summary>A member likes an announcement. Only members with an up-to-date membership (or the board), on a published announcement they can read.</summary>
internal sealed class LikeAnnouncementHandler(IValidator<LikeAnnouncementRequest> validator, IDateTimeProvider clock, IUserContext currentUser,
    AnnouncementViewer viewer, AppDbContext db) : IHandler<LikeAnnouncementRequest>
{
    public async Task<Result> HandleAsync(LikeAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        if (audience == AnnouncementAudience.Visitor)
            return Error.Forbidden("Réservé aux adhérents.");

        var id = Guid.Parse(request.Id);
        var isReadable = await db.Announcements.VisibleTo(audience, viewer.Today).AnyAsync(a => a.Id == id && a.IsPublished, ct);
        if (!isReadable)
            return Error.NotFound();

        var member = Guid.Parse(currentUser.AccountId!);
        if (await db.AnnouncementLikes.AnyAsync(l => l.AnnouncementId == id && l.MemberId == member, ct))
            return Result.Success();

        db.AnnouncementLikes.Add(new AnnouncementLike { AnnouncementId = id, MemberId = member, LikedAt = clock.UtcNow });
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // The same like was recorded at the same time (primary key): it is there, which is what was asked.
        }

        return Result.Success();
    }
}
