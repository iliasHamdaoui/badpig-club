using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.ListStockMovements;

internal sealed class ListStockMovementsValidator : ListQueryValidator<ListStockMovementsRequest>
{
    public ListStockMovementsValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Sort).MustBeSortableOn("createdAt");
    }
}
