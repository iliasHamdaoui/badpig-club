using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.UpdateAnnouncement;

/// <summary>Replaces the title, the text, the type and the end date of an announcement (the question and explanation of a poll; its choices never change).</summary>
internal sealed class UpdateAnnouncementHandler(IValidator<UpdateAnnouncementRequest> validator, IDateTimeProvider clock,
    IRichText richText, AppDbContext db) : IHandler<UpdateAnnouncementRequest>
{
    public async Task<Result> HandleAsync(UpdateAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var announcement = await db.Announcements.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (announcement is null)
            return Error.NotFound();

        var body = request.Body;
        if (AnnouncementExpiry.Check(body.ExpiresOn, AnnouncementExpiry.TodayAt(clock.UtcNow)) is { } expiryError)
            return expiryError;

        if (await EndsBeforeThePollClosesAsync(id, body.ExpiresOn, ct))
            return Error.Validation("expiresOn", "L'annonce doit rester affichée au moins jusqu'à la clôture de son sondage.");

        var content = CleanText(announcement, body.BodyHtml);
        if (!content.IsSuccess)
            return content.Errors;

        announcement.Title = body.Title!.Trim();
        announcement.BodyHtml = content.Value.Html;
        announcement.Summary = content.Value.Summary;
        announcement.Kind = body.Kind!;
        announcement.ExpiresOn = body.ExpiresOn ?? announcement.ExpiresOn;
        announcement.CommentsEnabled = body.CommentsEnabled ?? announcement.CommentsEnabled;
        announcement.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
    // A text announcement needs its text; the explanation of a poll is optional.
    private Result<CleanedContent> CleanText(Announcement announcement, string? html)
    {
        if (!string.IsNullOrWhiteSpace(html))
            return AnnouncementContent.Clean(richText, html);

        return announcement.Form == AnnouncementForms.Poll ? new CleanedContent("", "") : Error.Validation("bodyHtml", "Le contenu est obligatoire.");
    }

    private async Task<bool> EndsBeforeThePollClosesAsync(Guid id, DateOnly? expiresOn, CancellationToken ct) =>
        expiresOn is { } date && await db.AnnouncementPolls.AnyAsync(p => p.AnnouncementId == id && p.ClosesOn > date, ct);
}
