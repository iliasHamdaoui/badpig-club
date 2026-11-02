using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.UpdateProduct;

internal sealed class UpdateProductValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Name).MustBeAProductName();
        RuleFor(x => x.Body.Description).MustBeAProductDescription();
        RuleFor(x => x.Body.IsActive).NotNull().WithMessage("Indiquez si l'article est proposé.");
    }
}
