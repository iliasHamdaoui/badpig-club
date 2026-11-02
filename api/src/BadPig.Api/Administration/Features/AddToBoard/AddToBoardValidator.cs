using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Administration.Features.AddToBoard;

internal sealed class AddToBoardValidator : AbstractValidator<AddToBoardRequest>
{
    public AddToBoardValidator() => RuleFor(x => x.Id).MustBeGuid();
}
