using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.ListTeamCandidates;

internal sealed class ListTeamCandidatesValidator : ListQueryValidator<ListTeamCandidatesRequest>
{
    public ListTeamCandidatesValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Sort).MustBeSortableOn("name");
    }
}
