using BadPig.Orders.Core;
using BadPig.Shared.Core;
using FluentValidation;

namespace BadPig.Shop.Features.ListShopOrders;

/// <summary>The orders received, for the board (read through the orders module).</summary>
internal sealed class ListShopOrdersHandler(IValidator<ListShopOrdersRequest> validator, IOrderBook orders)
    : IHandler<ListShopOrdersRequest, PagedResponse<ListShopOrdersItem>>
{
    public async Task<Result<PagedResponse<ListShopOrdersItem>>> HandleAsync(ListShopOrdersRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        var page = await orders.ListAsync(request.Status, request, ct);
        return page.Map(o => new ListShopOrdersItem(o.Id.ToString(), o.BuyerName, o.Status, o.TotalCents, o.CreatedAt, o.PaidAt, o.DeliveredAt, o.DeliveredByName,
            o.Lines.Select(l => new ListShopOrdersLine(l.ProductName, l.VariantLabel, l.UnitPriceCents, l.Quantity)).ToList()));
    }
}
