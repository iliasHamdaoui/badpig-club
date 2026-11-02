using BadPig.Administration.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.UpdateSeason;

internal sealed class UpdateSeasonValidator : AbstractValidator<UpdateSeasonRequest>
{
    public UpdateSeasonValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Name).NotEmpty().WithMessage("Le nom de la saison est obligatoire.").MustBeASeasonName();
        RuleFor(x => x.Body.StartsOn).NotNull().WithMessage("La date de début est obligatoire.");
        RuleFor(x => x.Body.EndsOn).NotNull().WithMessage("La date de fin est obligatoire.")
            .Must((x, ends) => ends is null || x.Body.StartsOn is null || ends > x.Body.StartsOn)
            .WithMessage("La date de fin doit être après la date de début.");
        RuleFor(x => x.Body.GraceDays).NotNull().WithMessage("La tolérance est obligatoire.").MustBeAGracePeriod();
        RuleFor(x => x.Body.InternalFee).NotNull().WithMessage("La cotisation des licenciés du club est obligatoire.").MustBeAFee();
        RuleFor(x => x.Body.ExternalFee).NotNull().WithMessage("La cotisation des externes est obligatoire.").MustBeAFee();
    }
}
