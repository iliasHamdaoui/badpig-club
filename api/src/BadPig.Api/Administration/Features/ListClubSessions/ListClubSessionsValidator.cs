using FluentValidation;

namespace BadPig.Administration.Features.ListClubSessions;

internal sealed class ListClubSessionsValidator : AbstractValidator<ListClubSessionsRequest>
{
    public ListClubSessionsValidator() =>
        RuleFor(x => x.Count).Must(count => count is null or (>= 1 and <= 10)).WithMessage("Le nombre de séances est compris entre 1 et 10.");
}
