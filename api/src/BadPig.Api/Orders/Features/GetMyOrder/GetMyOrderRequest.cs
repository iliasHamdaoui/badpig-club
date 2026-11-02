namespace BadPig.Orders.Features.GetMyOrder;

/// <summary>Reading of one of my orders.</summary>
/// <param name="Id">Identifier of the order.</param>
public sealed record GetMyOrderRequest(string Id);
