using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.DeleteVariant;

internal sealed class DeleteVariantValidator : AbstractValidator<DeleteVariantRequest>
{
    public DeleteVariantValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
