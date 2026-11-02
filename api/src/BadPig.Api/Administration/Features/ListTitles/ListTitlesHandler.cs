using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ListTitles;

/// <summary>The board titles with their holder, searched, sorted and paginated.</summary>
internal sealed class ListTitlesHandler(IValidator<ListTitlesRequest> validator, AppDbContext db, IIdentityProvider identity)
    : IHandler<ListTitlesRequest, PagedResponse<ListTitlesItem>>
{
    public async Task<Result<PagedResponse<ListTitlesItem>>> HandleAsync(ListTitlesRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var holders = (await db.BoardTitles.AsNoTracking().ToListAsync(ct)).ToDictionary(b => b.TitleId, b => b.MemberId.ToString());
        var names = (await identity.ListAsync(ct)).ToDictionary(a => a.Id, a => a.Name);

        var query = db.Titles.AsNoTracking();
        if (request.SearchTerm() is { } term)
        {
            var byHolder = holders.Where(h => names.GetValueOrDefault(h.Value).HasText(term)).Select(h => h.Key).ToList();
            query = query.Where(t => t.Label.ToLower().Contains(term) || byHolder.Contains(t.Id));
        }

        var desc = request.IsDescending();
        IOrderedQueryable<Title>? ordered = request.Sort?.ToLowerInvariant() switch
        {
            null or "" or "createdat" => desc ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
            "label" => desc ? query.OrderByDescending(t => t.Label) : query.OrderBy(t => t.Label),
            _ => null,
        };
        if (ordered is null)
            return Error.Validation("sort", "Tri invalide.");

        var page = await ordered.ThenBy(t => t.Label).ThenBy(t => t.Id).ToPageAsync(request, ct);
        return page.Map(t =>
        {
            var holder = holders.GetValueOrDefault(t.Id);
            return new ListTitlesItem(t.Id.ToString(), t.Label, holder, holder is null ? null : names.GetValueOrDefault(holder));
        });
    }
}
