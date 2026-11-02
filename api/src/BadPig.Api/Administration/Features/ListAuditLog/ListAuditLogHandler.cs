using FluentValidation;
using BadPig.Administration.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Administration.Features.ListAuditLog;

/// <summary>
/// Journal of the administrative operations of the board, searched, sorted and paginated. The names of the target people
/// are read back from the accounts: a deleted account no longer has a name (no copy is kept).
/// </summary>
internal sealed class ListAuditLogHandler(IValidator<ListAuditLogRequest> validator, AppDbContext db, IIdentityProvider identity)
    : IHandler<ListAuditLogRequest, PagedResponse<ListAuditLogItem>>
{
    public async Task<Result<PagedResponse<ListAuditLogItem>>> HandleAsync(ListAuditLogRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var names = (await identity.ListAsync(ct)).ToDictionary(a => a.Id, a => a.Name);

        var query = db.AdministrationAuditLog.AsNoTracking();
        if (request.SearchTerm() is { } term)
        {
            var targets = names.Where(n => n.Value.HasText(term)).Select(n => Guid.Parse(n.Key)).ToList();
            query = query.Where(e => e.ActorName.ToLower().Contains(term) || e.Action.ToLower().Contains(term)
                || (e.TargetId != null && targets.Contains(e.TargetId.Value)));
        }

        var sort = request.Sort?.ToLowerInvariant();
        var desc = request.IsDescending(defaultDescending: sort is null or "" or "date");
        IOrderedQueryable<AdministrationAuditEntry>? ordered = sort switch
        {
            null or "" or "date" => desc ? query.OrderByDescending(e => e.OccurredAt) : query.OrderBy(e => e.OccurredAt),
            "actor" => desc ? query.OrderByDescending(e => e.ActorName) : query.OrderBy(e => e.ActorName),
            "action" => desc ? query.OrderByDescending(e => e.Action) : query.OrderBy(e => e.Action),
            _ => null,
        };
        if (ordered is null)
            return Error.Validation("sort", "Tri invalide.");

        var page = await ordered.ThenByDescending(e => e.Id).ToPageAsync(request, ct);
        return page.Map(e =>
        {
            var targetId = e.TargetId?.ToString();
            return new ListAuditLogItem(
                e.Id, e.OccurredAt, e.ActorName, e.Action, targetId,
                targetId is not null && names.TryGetValue(targetId, out var name) ? name : null);
        });
    }
}
