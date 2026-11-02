using BadPig.Shared.Payments;
using Microsoft.Extensions.Logging;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Orders.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Orders.Features.ReceivePaymentNotification;

/// <summary>Settles the order that a signed notification of the payment provider is about.</summary>
internal sealed class ReceivePaymentNotificationHandler(IValidator<ReceivePaymentNotificationRequest> validator, IPaymentProvider payments, OrderSettlement settlement, AppDbContext db, ILogger<ReceivePaymentNotificationHandler> log)
    : IHandler<ReceivePaymentNotificationRequest>
{
    public async Task<Result> HandleAsync(ReceivePaymentNotificationRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var notification = payments.ReadNotification(request.Payload, request.Signature);
        if (notification.IsFailure)
            return notification.Errors;

        if (notification.Value is not { } received)
            return Result.Success();

        var orderId = await db.Orders.AsNoTracking().Where(o => o.PaymentSessionId == received.SessionId).Select(o => (Guid?)o.Id).FirstOrDefaultAsync(ct);
        if (orderId is null)
        {
            log.LogWarning("Payment notification for an unknown payment page");
            return Result.Success();
        }

        if (received.State == PaymentState.Paid)
        {
            // Not pending any more (cancelled or expired meanwhile): the money was taken for an order that no longer holds stock. The board must look at it.
            if (!await settlement.MarkPaidAsync(orderId.Value, ct) && await IsNotPaidAsync(orderId.Value, ct))
                log.LogWarning("Order {OrderId} was paid but is no longer waiting for payment: check it and refund if needed", orderId);
        }
        else if (received.State == PaymentState.Expired)
        {
            await settlement.ReleaseAsync(orderId.Value, OrderStatuses.Expired, ct);
        }

        return Result.Success();
    }

    private async Task<bool> IsNotPaidAsync(Guid orderId, CancellationToken ct) =>
        !await db.Orders.AnyAsync(o => o.Id == orderId && (o.Status == OrderStatuses.Paid || o.Status == OrderStatuses.Delivered), ct);
}
