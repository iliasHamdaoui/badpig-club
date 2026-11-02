using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.CorrectVariantStock;

internal sealed class CorrectVariantStockValidator : AbstractValidator<CorrectVariantStockRequest>
{
    public CorrectVariantStockValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
        RuleFor(x => x.Body.Quantity).MustBeAQuantity(0);
        RuleFor(x => x.Body.Note).MustBeAStockNote();
        RuleFor(x => x.Body.Note).Must(note => !string.IsNullOrWhiteSpace(note)).WithMessage("Expliquez la correction (le motif est obligatoire).");
    }
}
