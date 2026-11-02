using BadPig.Administration.Core;
using FluentValidation;

namespace BadPig.Administration.Features.CreateSeason;

internal sealed class CreateSeasonValidator : AbstractValidator<CreateSeasonRequest>
{
    public CreateSeasonValidator()
    {
        RuleFor(x => x.StartsOn).NotNull().WithMessage("La date de début est obligatoire.");
        RuleFor(x => x.EndsOn).Must((x, ends) => ends is null || x.StartsOn is null || ends > x.StartsOn)
            .WithMessage("La date de fin doit être après la date de début.");
        RuleFor(x => x.Name).MustBeASeasonName();
        RuleFor(x => x.GraceDays).MustBeAGracePeriod();
        RuleFor(x => x.InternalFee).NotNull().WithMessage("La cotisation des licenciés du club est obligatoire.").MustBeAFee();
        RuleFor(x => x.ExternalFee).NotNull().WithMessage("La cotisation des externes est obligatoire.").MustBeAFee();
    }
}
