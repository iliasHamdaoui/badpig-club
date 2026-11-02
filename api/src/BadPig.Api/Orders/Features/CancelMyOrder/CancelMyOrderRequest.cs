namespace BadPig.Orders.Features.CancelMyOrder;

/// <summary>Cancellation of one of my orders.</summary>
/// <param name="Id">Identifier of the order.</param>
public sealed record CancelMyOrderRequest(string Id);
