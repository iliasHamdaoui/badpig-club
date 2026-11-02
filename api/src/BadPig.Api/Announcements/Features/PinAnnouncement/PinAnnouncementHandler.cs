using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.PinAnnouncement;

/// <summary>Pins an announcement at the top of the list.</summary>
internal sealed class PinAnnouncementHandler(IValidator<PinAnnouncementRequest> validator, AppDbContext db)
    : IHandler<PinAnnouncementRequest>
{
    public async Task<Result> HandleAsync(PinAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        announcement.IsPinned = true;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
