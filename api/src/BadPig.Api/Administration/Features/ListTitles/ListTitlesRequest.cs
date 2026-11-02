using BadPig.Shared.Core;

namespace BadPig.Administration.Features.ListTitles;

/// <summary>Search (title or holder name), sort (label, createdAt; default createdAt asc) and pagination.</summary>
public sealed record ListTitlesRequest : ListQuery;
