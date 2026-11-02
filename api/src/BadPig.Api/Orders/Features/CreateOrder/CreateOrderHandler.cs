using BadPig.Orders.Core;
using BadPig.Shared.Core;
using BadPig.Shared.Identity;
using BadPig.Shared.Payments;
using BadPig.Shared.Persistence;
using BadPig.Shop.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BadPig.Orders.Features.CreateOrder;

/// <summary>Reserves the stock, records the order and opens the payment page.</summary>
internal sealed class CreateOrderHandler(IValidator<CreateOrderRequest> validator, OrderAccess access, OrderSettlement settlement, IShopCatalog catalog,
    IStockKeeper stock, IPaymentProvider payments, IIdentityProvider identity, IUserContext currentUser, IDateTimeProvider clock,
    IOptions<OrderLinks> links, AppDbContext db) : IHandler<CreateOrderRequest, CreatedResponse>
{
    public async Task<Result<CreatedResponse>> HandleAsync(CreateOrderRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        if (!await access.CanOrderAsync(ct))
            return Error.Forbidden("Réservé aux adhérents.");

        var buyerId = Guid.Parse(currentUser.AccountId!);
        var orderId = Guid.NewGuid();
        var page = links.Value.OrderPage(orderId);
        if (page is null)
            return Error.Unexpected("L'adresse du site n'est pas configurée.");

        await settlement.ReleaseExpiredAsync(ct);
        if (await HasAnUnpaidOrderAsync(buyerId, ct))
            return Error.Conflict("Vous avez déjà une commande en attente de paiement.", "Payez-la ou annulez-la avant d'en faire une autre.");

        var wanted = request.Lines!.Select(l => new StockLine(Guid.Parse(l.VariantId!), l.Quantity!.Value)).ToList();
        var variants = await catalog.FindVariantsAsync(wanted.Select(w => w.VariantId).ToList(), ct);
        if (variants.Count != wanted.Count)
            return Error.NotFound();

        var notOffered = variants.Values.FirstOrDefault(v => !v.IsOffered);
        if (notOffered is not null)
            return Error.Conflict($"« {NameOf(notOffered)} » n'est plus proposé.");

        // The stock is held for all the lines or for none: any failure undoes the transaction.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (await stock.HoldAsync(wanted, ct) is { } missing)
            return Error.Conflict($"Stock insuffisant pour « {NameOf(variants[missing])} ».");

        var now = clock.UtcNow;
        var order = new Order
        {
            Id = orderId, BuyerId = buyerId, BuyerName = currentUser.DisplayName, Status = OrderStatuses.Pending, CreatedAt = now, ExpiresAt = now + OrderRules.Reservation,
        };
        foreach (var line in wanted)
        {
            var variant = variants[line.VariantId];
            order.Lines.Add(new OrderLine
            {
                Id = Guid.NewGuid(), OrderId = order.Id, VariantId = line.VariantId, ProductName = variant.ProductName,
                VariantLabel = variant.Label, UnitPriceCents = variant.PriceCents, Quantity = line.Quantity,
            });
        }

        order.TotalCents = order.Lines.Sum(l => l.UnitPriceCents * l.Quantity);
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        var lines = order.Lines.Select(l => new PaymentLine(PaymentName(l), l.UnitPriceCents, l.Quantity)).ToList();
        var email = await EmailOfAsync(buyerId, ct);
        var session = await payments.CreateSessionAsync(new PaymentRequest(order.Id, lines, email, page, page + "?canceled=1", order.ExpiresAt), ct);
        if (session.IsFailure)
            return session.Errors;

        order.PaymentSessionId = session.Value.Id;
        order.CheckoutUrl = session.Value.Url;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new CreatedResponse(order.Id.ToString());
    }

    private static string NameOf(CatalogVariant variant) => $"{variant.ProductName} - {variant.Label}";

    private static string PaymentName(OrderLine line)
    {
        var name = $"{line.ProductName} - {line.VariantLabel}";
        return name.Length <= 250 ? name : name[..250];
    }

    private Task<bool> HasAnUnpaidOrderAsync(Guid buyerId, CancellationToken ct) =>
        db.Orders.AnyAsync(o => o.BuyerId == buyerId && o.Status == OrderStatuses.Pending, ct);

    // The address only pre-fills the payment page: not finding it never blocks the order.
    private async Task<string?> EmailOfAsync(Guid buyerId, CancellationToken ct)
    {
        try
        {
            return (await identity.FindAsync(buyerId.ToString(), ct))?.Email;
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return null;
        }
    }
}
