using BadPig.Shared.Core;
using BadPig.Orders.Core;
using FluentValidation;

namespace BadPig.Shop.Features.ListShopOrders;

internal sealed class ListShopOrdersValidator : ListQueryValidator<ListShopOrdersRequest>
{
    public ListShopOrdersValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn(OrderSortFields.All);
        RuleFor(x => x.Status).Must(status => string.IsNullOrEmpty(status) || OrderStatuses.All.Contains(status))
            .WithMessage($"Statut inconnu : {string.Join(", ", OrderStatuses.All)}.");
    }
}
