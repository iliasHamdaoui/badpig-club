namespace BadPig.Shop.Features.ListShopAuditLog;

/// <summary>One line of the shop audit log.</summary>
/// <param name="Id">Line number (increasing).</param>
/// <param name="OccurredAt">Date and time of the action (UTC).</param>
/// <param name="Actor">Name of the board member who did the action.</param>
/// <param name="Action">Action code: product.created, product.updated, product.deleted, variant.created, variant.updated, variant.deleted, variant.restocked, variant.stock_corrected, order.delivered.</param>
/// <param name="TargetId">Identifier of the article, variant or order concerned, or null.</param>
/// <param name="Subject">Name of the article or "article – variant" as it was when the action was done; null for an order.</param>
/// <param name="Details">What changed (quantity, price…), or null.</param>
public sealed record ListShopAuditLogItem(long Id, DateTimeOffset OccurredAt, string Actor, string Action, string? TargetId, string? Subject, string? Details);
