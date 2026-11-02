using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.UnlikeAnnouncement;

/// <summary>A member takes back a like. Only members with an up-to-date membership (or the board), on a published announcement they can read.</summary>
internal sealed class UnlikeAnnouncementHandler(IValidator<UnlikeAnnouncementRequest> validator, IUserContext currentUser,
    AnnouncementViewer viewer, AppDbContext db) : IHandler<UnlikeAnnouncementRequest>
{
    public async Task<Result> HandleAsync(UnlikeAnnouncementRequest request, CancellationToken ct)
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
        await db.AnnouncementLikes.Where(l => l.AnnouncementId == id && l.MemberId == member).ExecuteDeleteAsync(ct);
        return Result.Success();
    }
}
