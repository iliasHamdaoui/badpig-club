using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.DeleteTitle;

internal sealed class DeleteTitleValidator : AbstractValidator<DeleteTitleRequest>
{
    public DeleteTitleValidator() => RuleFor(x => x.Id).MustBeGuid();
}
