using BadPig.Usage.Core;
using FluentValidation;

namespace BadPig.Usage.Features.GetUsageTrend;

internal sealed class GetUsageTrendValidator : AbstractValidator<GetUsageTrendRequest>
{
    public GetUsageTrendValidator() =>
        RuleFor(x => x.Days).Must(days => UsagePeriods.IsKnown(days)).WithMessage($"Période inconnue (valeurs possibles : {string.Join(", ", UsagePeriods.Allowed)} jours).");
}
