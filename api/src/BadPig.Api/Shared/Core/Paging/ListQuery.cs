namespace BadPig.Shared.Core;

/// <summary>
/// Query string common to every list endpoint: search, sorting and pagination. Each feature defines its own request as
/// <c>sealed record XRequest : ListQuery</c> and decides which fields can be sorted and what the search looks at.
/// </summary>
public record ListQuery
{
    /// <summary>Text to look for (case-insensitive, part of a word). Empty: no filter.</summary>
    public string? Search { get; init; }

    /// <summary>Name of the field to sort on (the allowed names depend on the list). Empty: the default order of the list.</summary>
    public string? Sort { get; init; }

    /// <summary>Asc or Desc. Empty: the default direction of the sorted field.</summary>
    public SortDirection? Direction { get; init; }

    /// <summary>Page number, starting at 1 (default 1).</summary>
    public int? Page { get; init; }

    /// <summary>Number of items per page (default 25, at most 100).</summary>
    public int? PageSize { get; init; }
}
