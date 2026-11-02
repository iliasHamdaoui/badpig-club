using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.DeleteSchoolHoliday;

internal sealed class DeleteSchoolHolidayValidator : AbstractValidator<DeleteSchoolHolidayRequest>
{
    public DeleteSchoolHolidayValidator()
    {
        RuleFor(x => x.SeasonId).MustBeGuid();
        RuleFor(x => x.Id).MustBeGuid();
    }
}
