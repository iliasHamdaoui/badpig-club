using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.ListInterclubSeasons;

internal sealed class ListInterclubSeasonsValidator : ListQueryValidator<ListInterclubSeasonsRequest>
{
    public ListInterclubSeasonsValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("name", "start");
    }
}
