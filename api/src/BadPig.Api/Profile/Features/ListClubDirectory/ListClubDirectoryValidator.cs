using BadPig.Shared.Core;

namespace BadPig.Profile.Features.ListClubDirectory;

internal sealed class ListClubDirectoryValidator : ListQueryValidator<ListClubDirectoryRequest>
{
    public ListClubDirectoryValidator() => RuleFor(x => x.Sort).MustBeSortableOn("name");
}
