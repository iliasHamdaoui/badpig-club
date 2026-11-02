using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.RestockVariant;

internal sealed class RestockVariantValidator : AbstractValidator<RestockVariantRequest>
{
    public RestockVariantValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Quantity).MustBeAQuantity(1);
        RuleFor(x => x.Body.Note).MustBeAStockNote();
    }
}
