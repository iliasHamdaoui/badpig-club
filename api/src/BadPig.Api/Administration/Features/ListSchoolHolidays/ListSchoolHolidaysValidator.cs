using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.ListSchoolHolidays;

internal sealed class ListSchoolHolidaysValidator : ListQueryValidator<ListSchoolHolidaysRequest>
{
    public ListSchoolHolidaysValidator()
    {
        RuleFor(x => x.Sort).MustBeSortableOn("startsOn", "name");
        RuleFor(x => x.SeasonId).MustBeGuid();
    }
}
