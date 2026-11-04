using BadPig.Shared.Core;
using BadPig.Usage.Core;
using FluentValidation;

namespace BadPig.Usage.Features.ListUsageStatistics;

internal sealed class ListUsageStatisticsValidator : ListQueryValidator<ListUsageStatisticsRequest>
{
    public ListUsageStatisticsValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("views", "seconds", "average", "path");
        RuleFor(x => x.Days)
            .Must(days => UsagePeriods.IsKnown(days))
            .WithMessage($"Période inconnue (valeurs possibles : {string.Join(", ", UsagePeriods.Allowed)} jours).");
    }
}
