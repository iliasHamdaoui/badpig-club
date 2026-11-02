namespace BadPig.Orders.Features.CreateOrder;

/// <summary>JSON body of an order.</summary>
/// <param name="Lines">The articles (1 to 20 lines, each variant once).</param>
public sealed record CreateOrderRequest(IReadOnlyList<CreateOrderLine>? Lines);
