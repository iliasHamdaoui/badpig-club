using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListUsers;

internal sealed class ListUsersValidator : ListQueryValidator<ListUsersRequest>
{
    public ListUsersValidator() => RuleFor(x => x.Sort).MustBeSortableOn("name", "email", "status", "membership");
}
