using BadPig.Shared.Core;
using BadPig.Orders.Core;
using FluentValidation;

namespace BadPig.Orders.Features.CancelMyOrder;

internal sealed class CancelMyOrderValidator : AbstractValidator<CancelMyOrderRequest>
{
    public CancelMyOrderValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
