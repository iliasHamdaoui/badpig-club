using BadPig.Shared.Core;
using BadPig.Shared.Persistence;
using BadPig.Orders.Core;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace BadPig.Orders.Features.ListMyOrders;

/// <summary>The orders of the signed-in person.</summary>
internal sealed class ListMyOrdersHandler(IValidator<ListMyOrdersRequest> validator, IUserContext currentUser, OrderSettlement settlement, AppDbContext db)
    : IHandler<ListMyOrdersRequest, PagedResponse<ListMyOrdersItem>>
{
    public async Task<Result<PagedResponse<ListMyOrdersItem>>> HandleAsync(ListMyOrdersRequest request, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return validation.Errors;

        await settlement.ReleaseExpiredAsync(ct);

        var me = Guid.Parse(currentUser.AccountId!);
        var query = db.Orders.AsNoTracking().Include(o => o.Lines).Where(o => o.BuyerId == me);
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(o => o.Status == request.Status);

        var descending = request.IsDescending(defaultDescending: request.Sort is null or "");
        var ordered = request.Sort?.ToLowerInvariant() == "totalcents"
            ? (descending ? query.OrderByDescending(o => o.TotalCents) : query.OrderBy(o => o.TotalCents))
            : (descending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt));
        var page = await ordered.ThenBy(o => o.Id).ToPageAsync(request, ct);
        return page.Map(o => new ListMyOrdersItem(o.Id.ToString(), o.Status, o.TotalCents, o.CreatedAt, o.Lines.Sum(l => l.Quantity)));
    }
}
