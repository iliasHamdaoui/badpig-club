using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;

namespace BadPig.Announcements.Features.CreateAnnouncement;

/// <summary>Creates an announcement as a draft; the text is cleaned before it is kept.</summary>
internal sealed class CreateAnnouncementHandler(IValidator<CreateAnnouncementRequest> validator, IDateTimeProvider clock, IUserContext currentUser,
    IRichText richText, AppDbContext db) : IHandler<CreateAnnouncementRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreateAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var content = AnnouncementContent.Clean(richText, request.BodyHtml!);
        if (!content.IsSuccess)
            return content.Errors;

        var today = AnnouncementExpiry.TodayAt(clock.UtcNow);
        if (AnnouncementExpiry.Check(request.ExpiresOn, today) is { } expiryError)
            return expiryError;

        var announcement = new Announcement
        {
            Id = Guid.NewGuid(),
            Title = request.Title!.Trim(),
            BodyHtml = content.Value.Html,
            Summary = content.Value.Summary,
            Kind = request.Kind!,
            CommentsEnabled = request.CommentsEnabled ?? true,
            AuthorId = Guid.Parse(currentUser.AccountId!),
            AuthorName = currentUser.DisplayName,
            ExpiresOn = request.ExpiresOn ?? AnnouncementExpiry.DefaultEnd(today),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
        };
        db.Announcements.Add(announcement);
        await db.SaveChangesAsync(ct);
        return new CreatedResponse(announcement.Id.ToString());
    }
}
