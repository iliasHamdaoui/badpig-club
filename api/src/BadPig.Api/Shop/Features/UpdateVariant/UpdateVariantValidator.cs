using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.UpdateVariant;

internal sealed class UpdateVariantValidator : AbstractValidator<UpdateVariantRequest>
{
    public UpdateVariantValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Label).MustBeAVariantLabel();
        RuleFor(x => x.Body.PriceCents).MustBeAPrice();
        RuleFor(x => x.Body.LowStockThreshold).MustBeAThreshold();
        RuleFor(x => x.Body.IsActive).NotNull().WithMessage("Indiquez si la variante est proposée.");
    }
}
