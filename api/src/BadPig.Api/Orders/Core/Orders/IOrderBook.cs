using BadPig.Shared.Core;

namespace BadPig.Orders.Core;

/// <summary>What the board's shop management may do with the orders received. The orders module owns its tables.</summary>
public interface IOrderBook
{
    /// <summary>The orders of everyone, optionally of one status; search on the buyer name, sort on <see cref="OrderSortFields"/> (default most recent first). Unpaid orders whose time is over are settled first.</summary>
    Task<PagedResponse<BoardOrder>> ListAsync(string? status, ListQuery query, CancellationToken ct);

    /// <summary>Marks a paid order as handed over to the member. NotFound if it does not exist, Conflict if it is not paid.</summary>
    Task<Result> MarkDeliveredAsync(Guid orderId, CancellationToken ct);
}
