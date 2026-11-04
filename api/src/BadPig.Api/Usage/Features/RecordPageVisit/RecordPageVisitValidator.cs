using System.Text.RegularExpressions;
using FluentValidation;

namespace BadPig.Usage.Features.RecordPageVisit;

internal sealed partial class RecordPageVisitValidator : AbstractValidator<RecordPageVisitRequest>
{
    public const int MaxSeconds = 30 * 60;

    // A route template: letters, digits, "/", ":", "-" and "_", starting with "/".
    [GeneratedRegex("^/[A-Za-z0-9/:_-]{0,98}$")]
    private static partial Regex RouteTemplate();

    public RecordPageVisitValidator()
    {
        RuleFor(x => x.Path).NotEmpty().WithMessage("La page est obligatoire.")
            .Must(path => path is null || RouteTemplate().IsMatch(path)).WithMessage("Le nom de la page n'est pas valide.");
        RuleFor(x => x.Views).Must(views => views is 0 or 1).WithMessage("Le nombre de vues est 0 ou 1.");
        RuleFor(x => x.Seconds).Must(seconds => seconds is >= 0 and <= MaxSeconds).WithMessage("La durée est comprise entre 0 seconde et 30 minutes.");
    }
}
