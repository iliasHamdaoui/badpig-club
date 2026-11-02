using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.DeleteProduct;

internal sealed class DeleteProductValidator : AbstractValidator<DeleteProductRequest>
{
    public DeleteProductValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
