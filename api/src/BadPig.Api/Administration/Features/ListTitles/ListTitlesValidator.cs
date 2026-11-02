using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListTitles;

internal sealed class ListTitlesValidator : ListQueryValidator<ListTitlesRequest>
{
    public ListTitlesValidator() => RuleFor(x => x.Sort).MustBeSortableOn("label", "createdAt");
}
