using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.CreateVariant;

internal sealed class CreateVariantValidator : AbstractValidator<CreateVariantRequest>
{
    public CreateVariantValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Label).MustBeAVariantLabel();
        RuleFor(x => x.Body.PriceCents).MustBeAPrice();
        RuleFor(x => x.Body.Quantity).MustBeAQuantity(0).When(x => x.Body.Quantity is not null);
        RuleFor(x => x.Body.LowStockThreshold).MustBeAThreshold().When(x => x.Body.LowStockThreshold is not null);
    }
}
