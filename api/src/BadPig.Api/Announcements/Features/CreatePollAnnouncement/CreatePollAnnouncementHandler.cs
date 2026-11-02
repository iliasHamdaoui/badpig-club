using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;

namespace BadPig.Announcements.Features.CreatePollAnnouncement;

/// <summary>Creates a poll: a standalone announcement (draft) whose title is the question, with its choices and closing date.</summary>
internal sealed class CreatePollAnnouncementHandler(IValidator<CreatePollAnnouncementRequest> validator, IDateTimeProvider clock, IUserContext currentUser,
    IRichText richText, AppDbContext db) : IHandler<CreatePollAnnouncementRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreatePollAnnouncementRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var today = AnnouncementExpiry.TodayAt(clock.UtcNow);
        if (PollRules.CheckClosingDate(request.ClosesOn, today) is { } dateError)
            return dateError;

        var closesOn = request.ClosesOn!.Value;
        if (AnnouncementExpiry.Check(request.ExpiresOn, today) is { } expiryError)
            return expiryError;

        if (request.ExpiresOn is { } expiresOn && expiresOn < closesOn)
            return Error.Validation("expiresOn", "Le sondage doit rester affiché au moins jusqu'à sa date limite.");

        var content = CleanOptionalText(request.BodyHtml);
        if (!content.IsSuccess)
            return content.Errors;

        var id = Guid.NewGuid();
        db.Announcements.Add(new Announcement
        {
            Id = id,
            Title = request.Title!.Trim(),
            BodyHtml = content.Value.Html,
            Summary = content.Value.Summary,
            Kind = request.Kind!,
            Form = AnnouncementForms.Poll,
            CommentsEnabled = request.CommentsEnabled ?? true,
            AuthorId = Guid.Parse(currentUser.AccountId!),
            AuthorName = currentUser.DisplayName,
            ExpiresOn = request.ExpiresOn ?? Latest(AnnouncementExpiry.DefaultEnd(today), closesOn),
            CreatedAt = clock.UtcNow,
            UpdatedAt = clock.UtcNow,
        });
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.SaveChangesAsync(ct); // the announcement first: the poll refers to it
        db.AnnouncementPolls.Add(new AnnouncementPoll
        {
            AnnouncementId = id,
            AllowMultiple = request.AllowMultiple,
            IsAnonymous = request.IsAnonymous,
            ClosesOn = closesOn,
            Options = request.Options!.Select((label, position) => new PollOption { Id = Guid.NewGuid(), AnnouncementId = id, Label = label.Trim(), Position = position }).ToList(),
        });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new CreatedResponse(id.ToString());
    }

    private Result<CleanedContent> CleanOptionalText(string? html) =>
        string.IsNullOrWhiteSpace(html) ? new CleanedContent("", "") : AnnouncementContent.Clean(richText, html);

    private static DateOnly Latest(DateOnly a, DateOnly b) => a >= b ? a : b;
}
