using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Orders.Features.GetProductImage;

internal sealed class GetProductImageValidator : AbstractValidator<GetProductImageRequest>
{
    public GetProductImageValidator() => RuleFor(x => x.Id).MustBeGuid();
}
