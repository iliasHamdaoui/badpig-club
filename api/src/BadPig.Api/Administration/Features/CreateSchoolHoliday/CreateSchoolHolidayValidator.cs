using BadPig.Administration.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.CreateSchoolHoliday;

internal sealed class CreateSchoolHolidayValidator : AbstractValidator<CreateSchoolHolidayRequest>
{
    public CreateSchoolHolidayValidator()
    {
        RuleFor(x => x.SeasonId).MustBeGuid();
        RuleFor(x => x.Body.Name).MustBeAHolidayName();
        RuleFor(x => x.Body.StartsOn).NotNull().WithMessage("La date de début est obligatoire.");
        RuleFor(x => x.Body.EndsOn).NotNull().WithMessage("La date de fin est obligatoire.");
        RuleFor(x => x.Body.EndsOn).Must((x, ends) => ends is null || x.Body.StartsOn is null || ends >= x.Body.StartsOn)
            .WithMessage("La date de fin doit être après la date de début (ou le même jour).");
    }
}
