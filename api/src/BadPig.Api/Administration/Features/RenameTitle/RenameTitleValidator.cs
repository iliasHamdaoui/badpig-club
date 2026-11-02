using BadPig.Administration.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.RenameTitle;

internal sealed class RenameTitleValidator : AbstractValidator<RenameTitleRequest>
{
    public RenameTitleValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Label).MustBeATitleLabel();
    }
}
