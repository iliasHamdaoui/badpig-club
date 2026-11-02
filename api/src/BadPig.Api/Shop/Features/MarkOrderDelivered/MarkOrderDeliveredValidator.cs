using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.MarkOrderDelivered;

internal sealed class MarkOrderDeliveredValidator : AbstractValidator<MarkOrderDeliveredRequest>
{
    public MarkOrderDeliveredValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
