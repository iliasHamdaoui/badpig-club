using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.ListProducts;

internal sealed class ListProductsValidator : ListQueryValidator<ListProductsRequest>
{
    public ListProductsValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("name", "createdAt");
    }
}
