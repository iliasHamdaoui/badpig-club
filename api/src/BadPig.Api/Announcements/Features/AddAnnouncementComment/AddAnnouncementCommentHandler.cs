using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BadPig.Announcements.Features.AddAnnouncementComment;

/// <summary>
/// A club member (or the board) comments, or answers a comment of, a published announcement they can read while comments are open.
/// The author of the announcement and the author of the answered comment receive an e-mail (never the person who writes, never twice).
/// </summary>
internal sealed class AddAnnouncementCommentHandler(IValidator<AddAnnouncementCommentRequest> validator, IDateTimeProvider clock,
    IUserContext currentUser, AnnouncementViewer viewer, AppDbContext db, IIdentityProvider identity, IEmailSender mail,
    IOptions<AnnouncementLinks> links, ILogger<AddAnnouncementCommentHandler> log) : IHandler<AddAnnouncementCommentRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(AddAnnouncementCommentRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        if (audience == AnnouncementAudience.Visitor)
            return Error.Forbidden("Réservé aux adhérents.");

        var announcementId = Guid.Parse(request.Id);
        var announcement = await db.Announcements.VisibleTo(audience, viewer.Today)
            .Where(a => a.Id == announcementId && a.IsPublished).Select(a => new { a.CommentsEnabled, a.Title, a.AuthorId, IsPoll = a.Form == AnnouncementForms.Poll }).FirstOrDefaultAsync(ct);
        if (announcement is null)
            return Error.NotFound();

        if (!announcement.CommentsEnabled)
            return Error.Conflict("Les commentaires sont fermés pour cette annonce.");

        Guid? parentId = null;
        string? quoted = null;
        Guid? quotedAuthorId = null;
        if (!string.IsNullOrEmpty(request.Body.ParentId))
        {
            var parentGuid = Guid.Parse(request.Body.ParentId);
            var parent = await db.AnnouncementComments.AsNoTracking()
                .Where(c => c.Id == parentGuid && c.AnnouncementId == announcementId)
                .Select(c => new { c.Id, c.AuthorId, c.Text }).FirstOrDefaultAsync(ct);
            if (parent is null)
                return Error.NotFound();

            parentId = parent.Id;
            quoted = parent.Text;
            quotedAuthorId = parent.AuthorId;
        }

        var comment = new AnnouncementComment
        {
            Id = Guid.NewGuid(),
            AnnouncementId = announcementId,
            ParentId = parentId,
            AuthorId = Guid.Parse(currentUser.AccountId!),
            AuthorName = currentUser.DisplayName,
            Text = request.Body.Text!.Trim(),
            CreatedAt = clock.UtcNow,
        };
        db.AnnouncementComments.Add(comment);
        await db.SaveChangesAsync(ct);
        await NotifyAsync(comment, announcement.Title, announcement.IsPoll, announcement.AuthorId, quotedAuthorId, quoted, ct);
        return new CreatedResponse(comment.Id.ToString());
    }

    // The person answered is told first; the author of the announcement is told too, unless they are the one answered (one e-mail) or the writer.
    private async Task NotifyAsync(AnnouncementComment comment, string title, bool isPoll, Guid? announcementAuthorId, Guid? answeredAuthorId, string? quoted, CancellationToken ct)
    {
        var link = links.Value.PageOf(comment.AnnouncementId);
        if (CanNotify(answeredAuthorId, comment))
        {
            var (subject, body) = CommentNotifications.OnYourComment(comment.AuthorName, title, comment.Text, Excerpt(quoted!), link);
            await SendAsync(answeredAuthorId!.Value, subject, body, ct);
        }

        if (CanNotify(announcementAuthorId, comment) && announcementAuthorId != answeredAuthorId)
        {
            var (subject, body) = CommentNotifications.OnYourAnnouncement(comment.AuthorName, title, isPoll, comment.Text, link);
            await SendAsync(announcementAuthorId!.Value, subject, body, ct);
        }
    }

    // Nobody to tell when the account no longer exists (null) or when the person writes to themselves.
    private static bool CanNotify(Guid? recipient, AnnouncementComment comment) => recipient is not null && recipient != comment.AuthorId;

    // A problem with the e-mail (identity provider down, mail server) never cancels the comment.
    private async Task SendAsync(Guid recipientId, string subject, string body, CancellationToken ct)
    {
        try
        {
            var person = await identity.FindAsync(recipientId.ToString(), ct);
            if (person is { IsActive: true })
                await mail.SendAsync(person, subject, body, ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            log.LogWarning(e, "Comment e-mail not sent to account {Target}", recipientId);
        }
    }

    private static string Excerpt(string text) => text.Length <= 200 ? text : text[..200].TrimEnd() + "…";
}
