namespace BadPig.Shop.Features.MarkOrderDelivered;

/// <summary>Handing over of an order.</summary>
/// <param name="Id">Identifier of the order.</param>
public sealed record MarkOrderDeliveredRequest(string Id);
