using BadPig.Shared.Core;
using BadPig.Interclubs.Core;
using FluentValidation;

namespace BadPig.Interclubs.Features.ChooseProposedDate;

internal sealed class ChooseProposedDateValidator : AbstractValidator<ChooseProposedDateRequest>
{
    public ChooseProposedDateValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.DateId).MustBeGuid();
    }
}
