using BadPig.Shared.Core;
using BadPig.Orders.Core;
using FluentValidation;

namespace BadPig.Orders.Features.ListCatalog;

internal sealed class ListCatalogValidator : ListQueryValidator<ListCatalogRequest>
{
    public ListCatalogValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("name");
    }
}
