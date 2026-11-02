using BadPig.Shared.Core;

namespace BadPig.Shop.Features.ListShopAuditLog;

/// <summary>Search (author, action code, article or details), sort (date, actor, action, subject; default date desc) and pagination.</summary>
public sealed record ListShopAuditLogRequest : ListQuery;
