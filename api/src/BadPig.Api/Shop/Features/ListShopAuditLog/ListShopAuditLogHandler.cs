using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Shop.Features.ListShopAuditLog;

/// <summary>The journal of the board's actions on the shop, searched, sorted and paginated.</summary>
internal sealed class ListShopAuditLogHandler(IValidator<ListShopAuditLogRequest> validator, AppDbContext db)
    : IHandler<ListShopAuditLogRequest, PagedResponse<ListShopAuditLogItem>>
{
    public async Task<Result<PagedResponse<ListShopAuditLogItem>>> HandleAsync(ListShopAuditLogRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var query = db.ShopAuditLog.AsNoTracking();
        if (request.SearchTerm() is { } term)
        {
            query = query.Where(e => e.ActorName.ToLower().Contains(term) || e.Action.ToLower().Contains(term)
                || (e.Subject != null && e.Subject.ToLower().Contains(term)) || (e.Details != null && e.Details.ToLower().Contains(term)));
        }

        var sort = request.Sort?.ToLowerInvariant();
        var desc = request.IsDescending(defaultDescending: sort is null or "" or "date");
        IOrderedQueryable<ShopAuditEntry> ordered = sort switch
        {
            "actor" => desc ? query.OrderByDescending(e => e.ActorName) : query.OrderBy(e => e.ActorName),
            "action" => desc ? query.OrderByDescending(e => e.Action) : query.OrderBy(e => e.Action),
            "subject" => desc ? query.OrderByDescending(e => e.Subject) : query.OrderBy(e => e.Subject),
            _ => desc ? query.OrderByDescending(e => e.OccurredAt) : query.OrderBy(e => e.OccurredAt),
        };

        var page = await ordered.ThenByDescending(e => e.Id).ToPageAsync(request, ct);
        return page.Map(e => new ListShopAuditLogItem(e.Id, e.OccurredAt, e.ActorName, e.Action, e.TargetId?.ToString(), e.Subject, e.Details));
    }
}
