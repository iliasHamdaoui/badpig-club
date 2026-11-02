using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListSeasons;

internal sealed class ListSeasonsValidator : ListQueryValidator<ListSeasonsRequest>
{
    public ListSeasonsValidator() => RuleFor(x => x.Sort).MustBeSortableOn("startsOn", "name");
}
