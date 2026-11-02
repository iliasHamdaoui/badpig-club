using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.RemoveFromBoard;

internal sealed class RemoveFromBoardValidator : AbstractValidator<RemoveFromBoardRequest>
{
    public RemoveFromBoardValidator() => RuleFor(x => x.Id).MustBeGuid();
}
