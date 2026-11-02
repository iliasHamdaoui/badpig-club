using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.ListFixtures;

internal sealed class ListFixturesValidator : ListQueryValidator<ListFixturesRequest>
{
    public ListFixturesValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("date", "team", "opponent");
        RuleFor(x => x.TeamId).Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Identifiant invalide.");
        RuleFor(x => x.SeasonId).Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Identifiant invalide.");
        RuleFor(x => x.When).Must(when => string.IsNullOrEmpty(when) || FixturePeriods.All.Contains(when))
            .WithMessage($"Période inconnue : {string.Join(", ", FixturePeriods.All)}.");
    }
}
