using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.ListAnnouncementComments;

/// <summary>The comments of an announcement that the reader can see (members and board only), oldest first by default; an answer carries a quote of the comment it answers.</summary>
internal sealed class ListAnnouncementCommentsHandler(IValidator<ListAnnouncementCommentsRequest> validator, IUserContext currentUser,
    AnnouncementViewer viewer, AppDbContext db) : IHandler<ListAnnouncementCommentsRequest, PagedResponse<ListAnnouncementCommentsItem>>
{
    private const int QuoteLength = 120;

    public async Task<Result<PagedResponse<ListAnnouncementCommentsItem>>> HandleAsync(ListAnnouncementCommentsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        if (audience == AnnouncementAudience.Visitor)
            return Error.Forbidden("Réservé aux adhérents.");

        var announcementId = Guid.Parse(request.Id);
        var isReadable = await db.Announcements.VisibleTo(audience, viewer.Today).AnyAsync(a => a.Id == announcementId && a.IsPublished, ct);
        if (!isReadable)
            return Error.NotFound();

        var query = db.AnnouncementComments.AsNoTracking().Where(c => c.AnnouncementId == announcementId);
        if (request.SearchTerm() is { } term)
            query = query.Where(c => c.Text.ToLower().Contains(term) || c.AuthorName.ToLower().Contains(term));

        var ordered = request.IsDescending() ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt);
        var page = await ordered.ThenBy(c => c.Id).ToPageAsync(request, ct);
        var quoted = await QuotedCommentsAsync(page.Items, announcementId, ct);
        return page.Map(c => new ListAnnouncementCommentsItem(
            c.Id.ToString(), c.AuthorName, c.Text, c.CreatedAt, IsMine(c), CanDelete(c), QuoteOf(c, quoted)));
    }

    // The quoted comments may be on another page, or gone.
    private async Task<Dictionary<Guid, AnnouncementComment>> QuotedCommentsAsync(IEnumerable<AnnouncementComment> comments, Guid announcementId, CancellationToken ct)
    {
        var ids = comments.Where(c => c.ParentId is not null).Select(c => c.ParentId).Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var found = await db.AnnouncementComments.AsNoTracking().Where(c => c.AnnouncementId == announcementId && ids.Contains(c.Id)).ToListAsync(ct);
        return found.ToDictionary(c => c.Id);
    }

    private static ListAnnouncementCommentsQuote? QuoteOf(AnnouncementComment comment, Dictionary<Guid, AnnouncementComment> quoted)
    {
        if (comment.ParentId is not { } parentId)
            return null;

        return quoted.TryGetValue(parentId, out var parent)
            ? new ListAnnouncementCommentsQuote(parent.Id.ToString(), parent.AuthorName, Excerpt(parent.Text), false)
            : new ListAnnouncementCommentsQuote(parentId.ToString(), null, "", true);
    }

    private static string Excerpt(string text) => text.Length <= QuoteLength ? text : text[..QuoteLength].TrimEnd() + "…";

    private bool IsMine(AnnouncementComment comment) => CommentRules.IsAuthor(comment, currentUser);

    private bool CanDelete(AnnouncementComment comment) => CommentRules.CanDelete(comment, currentUser);
}
