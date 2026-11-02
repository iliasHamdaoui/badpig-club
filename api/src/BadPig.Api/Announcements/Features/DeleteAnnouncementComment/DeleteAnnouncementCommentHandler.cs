using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.DeleteAnnouncementComment;

/// <summary>A member deletes their own comment; any board member deletes any comment.</summary>
internal sealed class DeleteAnnouncementCommentHandler(IValidator<DeleteAnnouncementCommentRequest> validator, IUserContext currentUser,
    AnnouncementViewer viewer, AppDbContext db) : IHandler<DeleteAnnouncementCommentRequest>
{
    public async Task<Result> HandleAsync(DeleteAnnouncementCommentRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        if (audience == AnnouncementAudience.Visitor)
            return Error.Forbidden("Réservé aux adhérents.");

        var announcementId = Guid.Parse(request.Id);
        var commentId = Guid.Parse(request.CommentId);
        var isReadable = await db.Announcements.VisibleTo(audience, viewer.Today).AnyAsync(a => a.Id == announcementId && a.IsPublished, ct);
        var comment = isReadable ? await db.AnnouncementComments.FirstOrDefaultAsync(c => c.Id == commentId && c.AnnouncementId == announcementId, ct) : null;
        if (comment is null)
            return Error.NotFound();

        if (!CommentRules.CanDelete(comment, currentUser))
            return Error.Forbidden("Vous ne pouvez supprimer que vos propres commentaires.");

        db.AnnouncementComments.Remove(comment);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
