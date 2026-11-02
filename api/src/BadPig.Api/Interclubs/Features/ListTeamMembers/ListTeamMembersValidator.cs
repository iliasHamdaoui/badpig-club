using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.ListTeamMembers;

internal sealed class ListTeamMembersValidator : ListQueryValidator<ListTeamMembersRequest>
{
    public ListTeamMembersValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Sort).MustBeSortableOn("name");
    }
}
