using BadPig.Orders.Core;
using BadPig.Shared.Core;
using BadPig.Shop.Core;
using FluentValidation;

namespace BadPig.Shop.Features.MarkOrderDelivered;

/// <summary>Marks a paid order as handed over (done by the orders module, which owns the orders).</summary>
internal sealed class MarkOrderDeliveredHandler(IValidator<MarkOrderDeliveredRequest> validator, IOrderBook orders, IShopAudit audit) : IHandler<MarkOrderDeliveredRequest>
{
    public const string AuditAction = "order.delivered";

    public async Task<Result> HandleAsync(MarkOrderDeliveredRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var orderId = Guid.Parse(request.Id);
        var result = await orders.MarkDeliveredAsync(orderId, ct);
        if (result.IsSuccess)
            await audit.RecordAsync(AuditAction, orderId, null, null, ct); // no buyer name: the order identifier is enough
        return result;
    }
}
