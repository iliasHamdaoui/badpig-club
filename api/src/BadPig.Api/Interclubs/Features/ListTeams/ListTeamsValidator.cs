using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.ListTeams;

internal sealed class ListTeamsValidator : ListQueryValidator<ListTeamsRequest>
{
    public ListTeamsValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("name", "season");
        RuleFor(x => x.SeasonId).Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Identifiant invalide.");
    }
}
