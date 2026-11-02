using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListSeasonStatistics;

internal sealed class ListSeasonStatisticsValidator : ListQueryValidator<ListSeasonStatisticsRequest>
{
    public ListSeasonStatisticsValidator() => RuleFor(x => x.Sort).MustBeSortableOn("startsOn", "name", "totalCount", "totalRevenue");
}
