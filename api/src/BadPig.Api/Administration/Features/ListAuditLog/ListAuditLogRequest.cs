using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListAuditLog;

/// <summary>Search (author, action code or concerned person), sort (date, actor, action; default date desc) and pagination.</summary>
public sealed record ListAuditLogRequest : ListQuery;
