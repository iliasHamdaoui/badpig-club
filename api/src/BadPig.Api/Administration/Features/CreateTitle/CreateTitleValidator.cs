using BadPig.Administration.Core;
using FluentValidation;

namespace BadPig.Administration.Features.CreateTitle;

internal sealed class CreateTitleValidator : AbstractValidator<CreateTitleRequest>
{
    public CreateTitleValidator() => RuleFor(x => x.Label).MustBeATitleLabel();
}
