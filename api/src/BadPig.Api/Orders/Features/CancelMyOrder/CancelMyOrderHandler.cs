using BadPig.Shared.Payments;
using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Orders.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Orders.Features.CancelMyOrder;

/// <summary>Cancels an unpaid order of the signed-in person and releases its stock.</summary>
internal sealed class CancelMyOrderHandler(IValidator<CancelMyOrderRequest> validator, IUserContext currentUser, OrderSettlement settlement, IPaymentProvider payments, AppDbContext db)
    : IHandler<CancelMyOrderRequest>
{
    public async Task<Result> HandleAsync(CancelMyOrderRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var id = Guid.Parse(request.Id);
        var me = Guid.Parse(currentUser.AccountId!);
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id && o.BuyerId == me, ct);
        if (order is null)
            return Error.NotFound();

        if (!OrderRules.IsWaitingForPayment(order))
            return Error.Conflict("Seule une commande en attente de paiement peut être annulée.");

        // Paid just now, the notification not yet received: the order is settled as paid and cannot be cancelled.
        if (order.PaymentSessionId is not null && await payments.GetSessionStateAsync(order.PaymentSessionId, ct) == PaymentState.Paid)
        {
            await settlement.MarkPaidAsync(id, ct);
            return Error.Conflict("Cette commande vient d'être payée : elle ne peut plus être annulée.");
        }

        if (!await settlement.ReleaseAsync(id, OrderStatuses.Cancelled, ct))
            return Error.Conflict("Seule une commande en attente de paiement peut être annulée.");

        if (order.PaymentSessionId is not null)
            await payments.CloseSessionAsync(order.PaymentSessionId, ct);

        return Result.Success();
    }
}
