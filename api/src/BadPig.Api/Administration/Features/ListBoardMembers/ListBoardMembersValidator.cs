using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListBoardMembers;

internal sealed class ListBoardMembersValidator : ListQueryValidator<ListBoardMembersRequest>
{
    public ListBoardMembersValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("name", "title");
    }
}
