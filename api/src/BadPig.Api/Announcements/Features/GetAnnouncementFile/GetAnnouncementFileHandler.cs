using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.GetAnnouncementFile;

/// <summary>The file of an announcement the reader is allowed to see.</summary>
internal sealed class GetAnnouncementFileHandler(IValidator<GetAnnouncementFileRequest> validator, AnnouncementViewer viewer,
    AppDbContext db, IAnnouncementFileStore files) : IHandler<GetAnnouncementFileRequest, GetAnnouncementFileResponse>
{
    public async Task<Result<GetAnnouncementFileResponse>> HandleAsync(GetAnnouncementFileRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        var announcementId = Guid.Parse(request.Id);
        var fileId = Guid.Parse(request.FileId);
        var file = await db.Announcements.AsNoTracking().VisibleTo(audience, viewer.Today)
            .Where(a => a.Id == announcementId)
            .SelectMany(a => a.Files)
            .FirstOrDefaultAsync(f => f.Id == fileId, ct);
        if (file is null)
            return Error.NotFound();

        var content = files.Open(file.StorageKey);
        if (content is null)
            return Error.NotFound();

        return new GetAnnouncementFileResponse(content, file.ContentType, file.FileName, file.Role == AnnouncementFileRoles.Poster);
    }
}
