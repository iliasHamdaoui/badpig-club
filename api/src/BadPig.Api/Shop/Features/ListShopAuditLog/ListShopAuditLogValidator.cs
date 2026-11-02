using BadPig.Shared.Core;

namespace BadPig.Shop.Features.ListShopAuditLog;

internal sealed class ListShopAuditLogValidator : ListQueryValidator<ListShopAuditLogRequest>
{
    public ListShopAuditLogValidator() => RuleFor(x => x.Sort).MustBeSortableOn("date", "actor", "action", "subject");
}
