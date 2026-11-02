using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.UnpinAnnouncement;

/// <summary>Removes the pin of an announcement.</summary>
internal sealed class UnpinAnnouncementHandler(IValidator<UnpinAnnouncementRequest> validator, AppDbContext db)
    : IHandler<UnpinAnnouncementRequest>
{
    public async Task<Result> HandleAsync(UnpinAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        announcement.IsPinned = false;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
