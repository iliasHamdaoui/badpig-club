using BadPig.Shared.Core;
using BadPig.Orders.Core;
using FluentValidation;

namespace BadPig.Orders.Features.ListMyOrders;

internal sealed class ListMyOrdersValidator : ListQueryValidator<ListMyOrdersRequest>
{
    public ListMyOrdersValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("createdAt", "totalCents");
        RuleFor(x => x.Status).Must(status => string.IsNullOrEmpty(status) || OrderStatuses.All.Contains(status))
            .WithMessage($"Statut inconnu : {string.Join(", ", OrderStatuses.All)}.");
    }
}
