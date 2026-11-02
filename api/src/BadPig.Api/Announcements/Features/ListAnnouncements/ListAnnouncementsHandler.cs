using BadPig.Announcements.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Announcements.Features.ListAnnouncements;

/// <summary>The announcements the reader is allowed to see: pinned first, then by publication date.</summary>
internal sealed class ListAnnouncementsHandler(IValidator<ListAnnouncementsRequest> validator, IUserContext currentUser,
    AnnouncementViewer viewer, IRichText richText, AppDbContext db) : IHandler<ListAnnouncementsRequest, PagedResponse<ListAnnouncementsItem>>
{
    public async Task<Result<PagedResponse<ListAnnouncementsItem>>> HandleAsync(ListAnnouncementsRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var audience = await viewer.ResolveAsync(ct);
        var query = db.Announcements.AsNoTracking().Include(a => a.Files).VisibleTo(audience, viewer.Today);
        if (!string.IsNullOrEmpty(request.Kind))
            query = query.Where(a => a.Kind == request.Kind);

        if (!string.IsNullOrEmpty(request.Form))
            query = query.Where(a => a.Form == request.Form);

        if (request.Status == "published")
            query = query.Where(a => a.IsPublished);

        if (request.Status == "draft")
            query = query.Where(a => !a.IsPublished);

        if (request.SearchTerm() is { } term)
            query = query.Where(a => a.Title.ToLower().Contains(term) || a.Summary.ToLower().Contains(term));

        var desc = request.IsDescending(defaultDescending: IsSortedByDate(request));
        var pinnedFirst = query.OrderByDescending(a => a.IsPinned);
        IOrderedQueryable<Announcement>? ordered = request.Sort?.ToLowerInvariant() switch
        {
            null or "" or "publishedat" => desc ? pinnedFirst.ThenByDescending(a => a.PublishedAt) : pinnedFirst.ThenBy(a => a.PublishedAt),
            "createdat" => desc ? pinnedFirst.ThenByDescending(a => a.CreatedAt) : pinnedFirst.ThenBy(a => a.CreatedAt),
            "title" => desc ? pinnedFirst.ThenByDescending(a => a.Title) : pinnedFirst.ThenBy(a => a.Title),
            _ => null,
        };
        if (ordered is null)
            return Error.Validation("sort", "Tri invalide.");

        var page = await ordered.ThenByDescending(a => a.CreatedAt).ThenBy(a => a.Id).ToPageAsync(request, ct);
        var ids = page.Items.Select(a => a.Id).ToList();
        var likeCounts = await LikeCountsAsync(audience, ids, ct);
        var likedByMe = await LikedByMeAsync(audience, ids, ct);

        return page.Map(a =>
        {
            var preview = richText.ToPreview(a.BodyHtml, PreviewLength);
            return new ListAnnouncementsItem(
                a.Id.ToString(), a.Title, a.Summary, a.Kind, a.Form, a.IsPublished, a.IsPinned, a.PublishedAt,
                a.Files.FirstOrDefault(f => f.Role == AnnouncementFileRoles.Poster)?.Id.ToString(),
                a.Files.Count(f => f.Role == AnnouncementFileRoles.Attachment),
                a.ExpiresOn, AnnouncementExpiry.IsExpired(a.ExpiresOn, viewer.Today),
                audience == AnnouncementAudience.Visitor ? null : likeCounts.GetValueOrDefault(a.Id),
                likedByMe.Contains(a.Id),
                audience == AnnouncementAudience.Board ? a.AuthorName : null,
                preview.Html, preview.IsTruncated);
        });
    }

    // How much a card shows before "read more" (a text budget: characters plus a cost per block, see IRichText.ToPreview).
    private const int PreviewLength = 450;

    // Dates are listed from the most recent; a title from A to Z.
    private static bool IsSortedByDate(ListAnnouncementsRequest request) => !string.Equals(request.Sort, "title", StringComparison.OrdinalIgnoreCase);

    private async Task<Dictionary<Guid, int>> LikeCountsAsync(AnnouncementAudience audience, List<Guid> ids, CancellationToken ct)
    {
        if (audience == AnnouncementAudience.Visitor)
            return [];

        return await db.AnnouncementLikes.Where(l => ids.Contains(l.AnnouncementId))
            .GroupBy(l => l.AnnouncementId).Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, ct);
    }

    private async Task<HashSet<Guid>> LikedByMeAsync(AnnouncementAudience audience, List<Guid> ids, CancellationToken ct)
    {
        if (audience == AnnouncementAudience.Visitor)
            return [];

        var me = Guid.Parse(currentUser.AccountId!);
        var liked = await db.AnnouncementLikes.Where(l => l.MemberId == me && ids.Contains(l.AnnouncementId)).Select(l => l.AnnouncementId).ToListAsync(ct);
        return liked.ToHashSet();
    }
}
