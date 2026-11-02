using BadPig.Shared.Payments;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Orders.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Orders.Features.GetMyOrder;

/// <summary>One order of the signed-in person; a pending one is first brought up to date with the payment provider.</summary>
internal sealed class GetMyOrderHandler(IValidator<GetMyOrderRequest> validator, IUserContext currentUser, IDateTimeProvider clock, OrderSettlement settlement, IPaymentProvider payments, AppDbContext db)
    : IHandler<GetMyOrderRequest, GetMyOrderResponse>
{
    public async Task<Result<GetMyOrderResponse>> HandleAsync(GetMyOrderRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var me = Guid.Parse(currentUser.AccountId!);
        var order = await LoadAsync(id, me, ct);
        if (order is null)
            return Error.NotFound();

        if (OrderRules.IsWaitingForPayment(order))
        {
            await RefreshAsync(order, ct);
            order = await LoadAsync(id, me, ct);
            if (order is null)
                return Error.NotFound();
        }

        return new GetMyOrderResponse(order.Id.ToString(), order.Status, order.TotalCents, order.CreatedAt, order.ExpiresAt, order.PaidAt, order.DeliveredAt,
            OrderRules.IsWaitingForPayment(order) ? order.CheckoutUrl : null,
            order.Lines.OrderBy(l => l.ProductName).ThenBy(l => l.VariantLabel)
                .Select(l => new GetMyOrderLine(l.ProductName, l.VariantLabel, l.UnitPriceCents, l.Quantity)).ToList());
    }

    private Task<Order?> LoadAsync(Guid id, Guid buyerId, CancellationToken ct) =>
        db.Orders.AsNoTracking().Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == buyerId, ct);

    // The notification of the payment provider may be late: ask it, so the buyer never sees "pending" for an order already paid.
    private async Task RefreshAsync(Order order, CancellationToken ct)
    {
        if (order.ExpiresAt <= clock.UtcNow)
        {
            await settlement.ReleaseExpiredAsync(ct);
            return;
        }

        if (order.PaymentSessionId is null)
            return;

        var state = await payments.GetSessionStateAsync(order.PaymentSessionId, ct);
        if (state == PaymentState.Paid)
            await settlement.MarkPaidAsync(order.Id, ct);
        else if (state == PaymentState.Expired)
            await settlement.ReleaseAsync(order.Id, OrderStatuses.Expired, ct);
    }
}
