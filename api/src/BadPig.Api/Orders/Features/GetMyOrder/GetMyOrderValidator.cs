using BadPig.Shared.Core;
using BadPig.Orders.Core;
using FluentValidation;

namespace BadPig.Orders.Features.GetMyOrder;

internal sealed class GetMyOrderValidator : AbstractValidator<GetMyOrderRequest>
{
    public GetMyOrderValidator()
    {
        RuleFor(x => x.Id).MustBeGuid();
    }
}
