using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Shop.Features.RemoveProductImage;

internal sealed class RemoveProductImageValidator : AbstractValidator<RemoveProductImageRequest>
{
    public RemoveProductImageValidator() => RuleFor(x => x.Id).MustBeGuid();
}
