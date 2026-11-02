using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListAuditLog;

internal sealed class ListAuditLogValidator : ListQueryValidator<ListAuditLogRequest>
{
    public ListAuditLogValidator() => RuleFor(x => x.Sort).MustBeSortableOn("date", "actor", "action");
}
