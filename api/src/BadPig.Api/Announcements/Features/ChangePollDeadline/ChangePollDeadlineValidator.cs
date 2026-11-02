using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Announcements.Features.ChangePollDeadline;

internal sealed class ChangePollDeadlineValidator : AbstractValidator<ChangePollDeadlineRequest>
{
    public ChangePollDeadlineValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.ClosesOn).NotNull().WithMessage("La date limite du sondage est obligatoire.");
    }
}
